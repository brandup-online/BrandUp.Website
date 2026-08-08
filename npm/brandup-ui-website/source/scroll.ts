import { HistoryState } from "./types";

/** Не чаще одной записи в историю за это время при непрерывной прокрутке. */
const SAVE_THROTTLE = 150;
/** Пауза после ухода фокуса: за это время фокус успевает устояться, а клавиатура — закрыться. */
const FOCUS_OUT_DELAY = 300;

/**
 * Позиция прокрутки в состоянии истории: сохраняется для текущей записи и восстанавливается,
 * когда пользователь возвращается к ней кнопкой «назад».
 *
 * Запись идёт через `history.replaceState`, а он на мобильных заставляет браузер пересчитать
 * вьюпорт. Пока поднята экранная клавиатура, это выглядит как её мигание, поэтому во время
 * ввода текста запись откладывается: позицию всё равно запишет уход фокуса, переход на другую
 * страницу или выгрузка документа.
 */
export class NavigationScroll {
    private readonly __getNavigationId: () => string | undefined;
    private readonly __scrollHandler: () => void;
    private readonly __focusOutHandler: () => void;
    private readonly __pageHideHandler: () => void;
    private __saveTimeout = 0;
    private __focusOutTimeout = 0;
    private __saveScheduled = false;

    /**
     * @param getNavigationId Идентификатор текущей навигации: позиция пишется только в ту запись
     * истории, которой она принадлежит.
     */
    constructor(getNavigationId: () => string | undefined) {
        this.__getNavigationId = getNavigationId;

        this.__scrollHandler = () => {
            if (this.__saveScheduled)
                return;

            this.__saveScheduled = true;
            this.__saveTimeout = window.setTimeout(() => {
                this.__saveScheduled = false;

                // Позиция читается в момент срабатывания таймера, поэтому она актуальная.
                if (!this.__isEditing())
                    this.save();
            }, SAVE_THROTTLE);
        };
        window.addEventListener("scroll", this.__scrollHandler, { passive: true });

        this.__focusOutHandler = () => {
            // focusout срабатывает и при переходе фокуса между полями, когда клавиатура остаётся
            // поднятой, и до того, как вьюпорт вернётся после её закрытия. Поэтому проверяем фокус
            // отложенно и пишем позицию, только когда ввод действительно завершён.
            window.clearTimeout(this.__focusOutTimeout);
            this.__focusOutTimeout = window.setTimeout(() => {
                if (!this.__isEditing())
                    this.save();
            }, FOCUS_OUT_DELAY);
        };
        window.addEventListener("focusout", this.__focusOutHandler);

        this.__pageHideHandler = () => this.save();
        window.addEventListener("pagehide", this.__pageHideHandler);
    }

    /** Записать текущую позицию прокрутки в состояние текущей записи истории. */
    save() {
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

    /**
     * Вернуть прокрутку на сохранённую позицию записи истории.
     * @param state Состояние записи, к которой вернулся пользователь: пришло из события popstate,
     * то есть уже принадлежит именно той записи, которую мы отрисовали.
     */
    restore(state: HistoryState | undefined) {
        const scroll = state?.brandup_website?.scroll;
        if (!scroll)
            return;

        window.scroll({ top: scroll.y, left: scroll.x, behavior: "instant" });
    }

    /** Отписаться от событий и снять отложенные записи. */
    destroy() {
        window.removeEventListener("scroll", this.__scrollHandler);
        window.removeEventListener("focusout", this.__focusOutHandler);
        window.removeEventListener("pagehide", this.__pageHideHandler);

        window.clearTimeout(this.__saveTimeout);
        window.clearTimeout(this.__focusOutTimeout);
    }

    /** Набирается ли текст: у поля ввода на мобильном поднята экранная клавиатура. */
    private __isEditing() {
        const elem = document.activeElement as HTMLElement | null;
        if (!elem)
            return false;

        return elem.isContentEditable || elem.tagName === "INPUT" || elem.tagName === "TEXTAREA";
    }
}
