// Dragging runs entirely in the browser: on Blazor Server every pointermove would otherwise be a
// round-trip over SignalR and the bar would visibly lag behind the cursor.

import { createLogger } from "/browser/logging.js";
import { ofInstance, ofType } from "/browser/guards.js";
import { getItem, setItem } from "/browser/storage.js";

const log = createLogger(import.meta.url);

/** What the component sends across with an initialize call, once every field has been checked. */
interface SplitterOptions {
  cssVariable: string;
  storageKey: string;
  minSize: number;
  isHorizontal: boolean;
  maxSizeRatio: number;
  defaultSizeRatio: number;
}

const disposers = new WeakMap<HTMLElement, () => void>();

export function initialize(uElement: unknown, uOptions: unknown): void {
  const element = ofInstance(uElement, HTMLElement);
  const options = (uOptions ?? {}) as Partial<Record<keyof SplitterOptions, unknown>>;
  const cssVariable = ofType(options.cssVariable, "string");
  const storageKey = ofType(options.storageKey, "string");
  const minSize = ofType(options.minSize, "number");
  const isHorizontal = ofType(options.isHorizontal, "boolean");
  const maxSizeRatio = ofType(options.maxSizeRatio, "number");
  const defaultSizeRatio = ofType(options.defaultSizeRatio, "number");

  if (
    element === null ||
    cssVariable === null ||
    storageKey === null ||
    minSize === null ||
    isHorizontal === null ||
    maxSizeRatio === null ||
    defaultSizeRatio === null
  ) {
    return;
  }

  const root = document.documentElement;

  // The size the user asked for is kept apart from the size that fits: a small window renders
  // clamped, but growing the window back restores what the user chose.
  let desiredSize = 0;
  const viewportSize = (): number => (isHorizontal ? window.innerWidth : window.innerHeight);

  const clamp = (value: number): number => Math.min(Math.max(value, minSize), viewportSize() * maxSizeRatio);
  const render = (): void => root.style.setProperty(cssVariable, `${clamp(desiredSize)}px`);
  const renderedSize = (): number => parseFloat(getComputedStyle(root).getPropertyValue(cssVariable));
  const setSize = (value: number): void => {
    desiredSize = value;
    render();
  };

  // The splitter works without storage - the size is only forgotten between visits - so a
  // failure reaching it must never reach the drag or the keystroke that caused it; the storage
  // module already swallows that failure and reports it once.
  const remember = (): void => setItem(storageKey, String(desiredSize));

  /** The size of an earlier visit, or null if none was stored or it cannot be read back. */
  const recall = (): number | null => {
    // Anything but a number is a value this script never wrote: storage is shared with every
    // other script on the origin, and survives the versions of this one that come and go.
    const stored = parseFloat(getItem(storageKey) ?? "");

    return Number.isFinite(stored) ? stored : null;
  };

  setSize(recall() ?? viewportSize() * defaultSizeRatio);

  let startPosition = 0;
  let startSize = 0;
  let startDirection = 1;

  // A handle divides the pane it sizes from a pane that takes whatever is left over, so a drag
  // only ever moves those two. The sized one is the neighbour that does not grow, and the side it
  // sits on says which way its size follows the pointer; reading that from the layout keeps the
  // component from being told what the stylesheet already says.
  const direction = (): number => {
    const preceding = element.previousElementSibling;

    return preceding !== null && parseFloat(getComputedStyle(preceding).flexGrow) === 0 ? 1 : -1;
  };

  const onPointerMove = (event: PointerEvent): void =>
    setSize(startSize + (startDirection * ((isHorizontal ? event.clientX : event.clientY) - startPosition)));

  const onPointerUp = (event: PointerEvent): void => {
    element.releasePointerCapture(event.pointerId);
    element.removeEventListener("pointermove", onPointerMove);
    element.removeEventListener("pointerup", onPointerUp);
    element.classList.remove("is-dragging");
    root.style.userSelect = "";
    remember();
  };

  const onPointerDown = (event: PointerEvent): void => {
    if (event.button !== 0) {
      return;
    }

    startPosition = isHorizontal ? event.clientX : event.clientY;
    startSize = renderedSize();
    startDirection = direction();

    element.setPointerCapture(event.pointerId);
    element.addEventListener("pointermove", onPointerMove);
    element.addEventListener("pointerup", onPointerUp);
    element.classList.add("is-dragging");

    // Without this the drag selects whatever text it sweeps across.
    root.style.userSelect = "none";
    event.preventDefault();
  };

  const onKeyDown = (event: KeyboardEvent): void => {
    // An arrow moves the handle, exactly as a drag does; which pane grows is the handle's
    // business, not the key's.
    const step = (event.shiftKey ? 50 : 10) * direction();

    if (event.key === (isHorizontal ? "ArrowRight" : "ArrowDown")) {
      setSize(renderedSize() + step);
    } else if (event.key === (isHorizontal ? "ArrowLeft" : "ArrowUp")) {
      setSize(renderedSize() - step);
    } else {
      return;
    }

    remember();
    event.preventDefault();
  };

  // Re-clamp so a size chosen on a large window does not swallow a small one; the chosen size
  // itself survives, so growing the window back restores it.
  element.addEventListener("pointerdown", onPointerDown);
  element.addEventListener("keydown", onKeyDown);
  window.addEventListener("resize", render);

  disposers.set(element, () => {
    element.removeEventListener("pointerdown", onPointerDown);
    element.removeEventListener("keydown", onKeyDown);
    window.removeEventListener("resize", render);
  });
}

export function dispose(uElement: unknown): void {
  const element = ofInstance(uElement, HTMLElement);
  if (element === null) {
    return;
  }

  const disposer = disposers.get(element);
  if (disposer) {
    disposer();
    disposers.delete(element);
  }
}
