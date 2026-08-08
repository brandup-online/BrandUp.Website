import { DOM } from "@brandup/ui";

const LOADER_CLASS = "bp-page-loader";
const SHOW_CLASS = "show";
const FINISH_CLASS = "finish";

/** Задержка перед появлением полосы: даёт браузеру отрисовать сброшенное состояние. */
const SHOW_DELAY = 10;
/** Ширина, с которой полоса появляется: с нуля она первые доли секунды визуально незаметна. */
const START_WIDTH = "15%";
/** Ширина, до которой полоса едет после появления. */
const SHOW_WIDTH = "70%";
/** Через сколько операция считается долгой и полоса дотягивается до конца. */
const STUCK_DELAY = 1700;
/** Минимальное время показа полосы: без него быстрые операции дают мигание. */
const MIN_VISIBLE_TIME = 500;
/** Длительность завершающей анимации, должна совпадать с transition класса finish. */
const FINISH_DELAY = 180;

/**
 * Полоса загрузки: линия вверху окна, которая ползёт вправо, пока идёт навигация или отправка формы.
 *
 * Жизненный цикл одной операции:
 * 1. `begin` сбрасывает полосу в 0% и возвращает номер операции;
 * 2. через SHOW_DELAY полоса появляется и едет до SHOW_WIDTH, а если операция затянулась
 *    дольше STUCK_DELAY — до 100%;
 * 3. `end` с номером этой операции доводит полосу до 100% и прячет — но не раньше, чем
 *    пройдёт MIN_VISIBLE_TIME от начала.
 *
 * Операции перекрываются: старт новой навигации отменяет текущую, и отменённая завершается уже
 * после того, как новая показала свою полосу. Поэтому `end` привязан к номеру операции и молча
 * выходит, если полоса уже принадлежит следующей.
 */
export class NavigationLoader {
    private readonly __elem: HTMLElement;
    private __operation = 0;
    private __startTime = 0;
    private __showTimeout = 0;
    private __stuckTimeout = 0;
    private __finishTimeout = 0;
    private __hideTimeout = 0;
    private __destroyed = false;

    constructor(container: HTMLElement) {
        this.__elem = DOM.tag("div", { class: LOADER_CLASS });
        container.appendChild(this.__elem);
    }

    /**
     * Начать показ полосы, прервав показ предыдущей операции.
     * @returns Номер операции, который нужно передать в `end`.
     */
    begin(): number {
        const token = ++this.__operation;
        if (this.__destroyed)
            return token;

        this.__clearTimeouts();

        this.__elem.classList.remove(SHOW_CLASS, FINISH_CLASS);
        this.__elem.style.width = "0%";

        // Фиксируем сброс отдельным пересчётом стилей: без него он склеивается с возвратом класса
        // show ниже (SHOW_DELAY меньше кадра), и переход считается от прежней ширины — если
        // предыдущая операция дотянула полосу до 100%, она поехала бы назад к SHOW_WIDTH.
        void this.__elem.offsetWidth;

        this.__showTimeout = window.setTimeout(() => {
            // Стартовая ширина ставится до класса show — в базовом правиле у width нет transition,
            // поэтому она применяется мгновенно, и полоса появляется сразу заметной. Отдельный
            // пересчёт стилей делает её точкой отсчёта для перехода к SHOW_WIDTH.
            this.__elem.style.width = START_WIDTH;
            void this.__elem.offsetWidth;

            this.__elem.classList.add(SHOW_CLASS);
            this.__elem.style.width = SHOW_WIDTH;
        }, SHOW_DELAY);

        this.__stuckTimeout = window.setTimeout(() => {
            this.__elem.classList.add(SHOW_CLASS);
            this.__elem.style.width = "100%";
        }, STUCK_DELAY);

        this.__startTime = Date.now();

        return token;
    }

    /**
     * Завершить показ полосы.
     * @param token Номер операции, полученный от `begin`. Если операцию уже сменила следующая
     * или показ не начинался, вызов ничего не делает.
     */
    end(token: number | undefined) {
        if (!token || token !== this.__operation || this.__destroyed)
            return;

        // Таймер появления намеренно не снимается: операция могла уложиться в SHOW_DELAY, и без
        // него быстрые переходы проходили бы вообще без индикатора. Полоса всё равно пробудет на
        // экране не меньше MIN_VISIBLE_TIME.
        window.clearTimeout(this.__stuckTimeout);
        window.clearTimeout(this.__finishTimeout);
        window.clearTimeout(this.__hideTimeout);

        const delay = Math.max(0, MIN_VISIBLE_TIME - (Date.now() - this.__startTime));
        this.__finishTimeout = window.setTimeout(() => {
            this.__elem.classList.add(FINISH_CLASS);
            this.__elem.style.width = "100%";

            this.__hideTimeout = window.setTimeout(() => {
                this.__elem.classList.remove(SHOW_CLASS, FINISH_CLASS);
                this.__elem.style.width = "0%";
            }, FINISH_DELAY);
        }, delay);
    }

    /**
     * Оставить полосу на экране: страница уходит на полную перезагрузку в браузере, и прятать
     * индикатор нельзя — документ ещё грузится. Парный `end` после этого ничего не сделает.
     */
    keep() {
        this.__operation++;
    }

    /** Убрать полосу из документа и погасить все отложенные шаги анимации. */
    destroy() {
        if (this.__destroyed)
            return;

        this.__destroyed = true;
        this.__clearTimeouts();
        this.__elem.remove();
    }

    private __clearTimeouts() {
        window.clearTimeout(this.__showTimeout);
        window.clearTimeout(this.__stuckTimeout);
        window.clearTimeout(this.__finishTimeout);
        window.clearTimeout(this.__hideTimeout);
    }
}
