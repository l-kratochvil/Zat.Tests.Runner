import { ofInstance } from "/browser/guards.js";

export function scrollToEnd(uElement: unknown): void {
  if (uElement === null || uElement === undefined) {
    return;
  }

  const element = ofInstance(uElement, HTMLElement);
  if (element === null) {
    return;
  }

  element.scrollTop = element.scrollHeight;
}
