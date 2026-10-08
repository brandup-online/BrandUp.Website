import type { HistoryState } from "./types";

/** Не чаще одной записи в историю за это время при непрерывной прокрутке. */
const SAVE_THROTTLE = 150;
/** Пауза после ухода фокуса: за это время фокус успевает устояться, а клавиатура — закрыться. */
const FOCUS_OUT_DELAY = 300;
/** Как часто проверять, дорос ли документ до сохранённой позиции. */
const RESTORE_INTERVAL = 100;
/** Сколько ждать, пока документ дорастёт до сохранённой позиции, прежде чем оставить его как есть. */
const RESTORE_TIMEOUT = 5000;
/** События, которыми пользователь сам берётся за прокрутку: после них позицию больше не навязываем. */
const USER_SCROLL_EVENTS = ["wheel", "touchstart", "keydown", "mousedown"];

/**
 * Позиция прокрутки в состоянии истории: сохраняется для текущей записи и восстанавливается,
 * когда пользователь возвращается к ней кнопкой «назад».
 *
 * Запись идёт через `history.replaceState`, а он на мобильных заставляет браузер пересчитать
 * вьюпорт. Пока поднята экранная клавиатура, это выглядит как её мигание, поэтому во время
 * ввода текста запись откладывается: позицию всё равно запишет уход фокуса, переход на другую
 * страницу или выгрузка документа.
 *
 * Собственное восстановление браузера (`history.scrollRestoration = "auto"`) отключается: оно
 * срабатывает на `popstate` раньше, чем отрисована страница записи, и применяет её позицию к
 * чужому содержимому, а после нашего восстановления Chrome ещё и держит окно у низа документа
 * при каждой вставке контента, пока пользователь сам не прокрутит, — список с подгрузкой по
 * прокрутке из-за этого догружается до конца. Раз браузер больше не восстанавливает позицию
 * и при перезагрузке, её восстанавливает первая навигация — из того же состояния истории.
 *
 * Документ к моменту восстановления часто короче сохранённой позиции, и прокрутка упирается в его
 * текущий низ. Так бывает на возврате к странице, которая догружает содержимое сама (список с
 * подгрузкой по прокрутке дорастает до прежней длины, только пока окно держится у низа), и на
 * первой навигации: она идёт до события `load`, и картинки без размеров ещё не дали документу
 * полную высоту. Поэтому восстановление повторяется, пока документ растёт, — до сохранённой
 * позиции, первого ввода пользователя или таймаута. Позиция на это время не записывается: иначе в
 * историю попала бы обрезанная, и следующий возврат восстановил бы уже её.
 */
export class NavigationScroll {
    private readonly __getNavigationId: () => string | undefined;
    private readonly __scrollHandler: () => void;
    private readonly __focusOutHandler: () => void;
    private readonly __pageHideHandler: () => void;
    private readonly __prevRestoration: ScrollRestoration;
    private __saveTimeout = 0;
    private __focusOutTimeout = 0;
    private __saveScheduled = false;
    /** Прекращает идущее восстановление позиции; задан, пока документ до неё не дорос. */
    private __stopRestore?: () => void;

    /**
     * @param getNavigationId Идентификатор текущей навигации: позиция пишется только в ту запись
     * истории, которой она принадлежит.
     */
    constructor(getNavigationId: () => string | undefined) {
        this.__getNavigationId = getNavigationId;

        this.__prevRestoration = window.history.scrollRestoration;
        window.history.scrollRestoration = "manual";

        this.__scrollHandler = () => {
            if (this.__saveScheduled)
                return;

            this.__saveScheduled = true;
            this.__saveTimeout = window.setTimeout(() => {
                this.__saveScheduled = false;

                // Позиция читается в момент срабатывания таймера, поэтому она актуальная.
                if (!this.__isEditing() && !this.__stopRestore)
                    this.__write();
            }, SAVE_THROTTLE);
        };
        window.addEventListener("scroll", this.__scrollHandler, { passive: true });

        this.__focusOutHandler = () => {
            // focusout срабатывает и при переходе фокуса между полями, когда клавиатура остаётся
            // поднятой, и до того, как вьюпорт вернётся после её закрытия. Поэтому проверяем фокус
            // отложенно и пишем позицию, только когда ввод действительно завершён.
            window.clearTimeout(this.__focusOutTimeout);
            this.__focusOutTimeout = window.setTimeout(() => {
                if (!this.__isEditing() && !this.__stopRestore)
                    this.__write();
            }, FOCUS_OUT_DELAY);
        };
        window.addEventListener("focusout", this.__focusOutHandler);

        this.__pageHideHandler = () => this.save();
        window.addEventListener("pagehide", this.__pageHideHandler);
    }

    /** Записать текущую позицию прокрутки в состояние текущей записи истории: уходим со страницы. */
    save() {
        if (this.__stopRestore) {
            // Ушли раньше, чем документ дорос до сохранённой позиции: в записи она и остаётся,
            // а не та, докуда окно успело прокрутиться.
            this.__stopRestore();
            return;
        }

        this.__write();
    }

    /**
     * Вернуть прокрутку на сохранённую позицию записи истории.
     * @param state Состояние записи, к которой вернулся пользователь: из события popstate или,
     * при первой навигации, пережившее перезагрузку.
     */
    restore(state: HistoryState | undefined) {
        this.__stopRestore?.();

        const scroll = state?.brandup_website?.scroll;
        if (!scroll)
            return;

        const apply = () => window.scroll({ top: scroll.y, left: scroll.x, behavior: "instant" });
        const reached = () => Math.abs(window.scrollX - scroll.x) < 1 && Math.abs(window.scrollY - scroll.y) < 1;

        apply();
        if (reached())
            return;

        // Документ ещё короче сохранённой позиции: повторяем, пока он растёт. Таймер, а не
        // ResizeObserver: при `html, body { height: 100% }` их размер не меняется, хотя
        // прокручиваемая высота растёт.
        const stop = () => {
            window.clearInterval(interval);
            window.clearTimeout(timeout);
            for (const type of USER_SCROLL_EVENTS)
                window.removeEventListener(type, stop, { capture: true });
            this.__stopRestore = undefined;

            // Отложенную запись поставили наши же повторы: сработав после остановки, она записала
            // бы позицию, до которой документ успел дорасти. Прокрутка пользователя поставит новую.
            window.clearTimeout(this.__saveTimeout);
            this.__saveScheduled = false;
        };
        const interval = window.setInterval(() => {
            apply();
            if (reached())
                stop();
        }, RESTORE_INTERVAL);
        const timeout = window.setTimeout(stop, RESTORE_TIMEOUT);
        // На погружении: компонент, который глушит ввод через stopPropagation (редактор, выпадающий
        // список, попап), иначе не остановил бы восстановление, и окно до таймаута прыгало бы назад.
        for (const type of USER_SCROLL_EVENTS)
            window.addEventListener(type, stop, { passive: true, capture: true });

        this.__stopRestore = stop;
    }

    /** Отписаться от событий и снять отложенные записи. */
    destroy() {
        window.removeEventListener("scroll", this.__scrollHandler);
        window.removeEventListener("focusout", this.__focusOutHandler);
        window.removeEventListener("pagehide", this.__pageHideHandler);

        window.history.scrollRestoration = this.__prevRestoration;

        window.clearTimeout(this.__saveTimeout);
        window.clearTimeout(this.__focusOutTimeout);
        this.__stopRestore?.();
    }

    private __write() {
        // Снимаем отложенную запись: метод вызывается и напрямую (уход фокуса, навигация,
        // выгрузка), и по таймеру троттлинга — второй проход ничего не добавит.
        window.clearTimeout(this.__saveTimeout);
        this.__saveScheduled = false;

        const navigationId = this.__getNavigationId();
        const state: HistoryState | null = window.history.state;
        if (!navigationId || state?.brandup_website?.id !== navigationId)
            return;

        const saved = state.brandup_website.scroll;
        if (saved && saved.x === window.scrollX && saved.y === window.scrollY)
            return;

        state.brandup_website.scroll = { x: window.scrollX, y: window.scrollY };
        window.history.replaceState(state, "");
    }

    /** Набирается ли текст: у поля ввода на мобильном поднята экранная клавиатура. */
    private __isEditing() {
        const elem = document.activeElement as HTMLElement | null;
        if (!elem)
            return false;

        return elem.isContentEditable || elem.tagName === "INPUT" || elem.tagName === "TEXTAREA";
    }
}
