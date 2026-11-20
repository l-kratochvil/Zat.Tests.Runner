// Wraps window.localStorage so a caller never has to guard it by hand: a write throws in Safari's
// private mode and on a full quota, and blocked site data makes reaching window.localStorage at
// all a SecurityError. Both functions catch that and answer as if nothing were stored - a caller
// works without persistence rather than break over it.

import { createLogger } from "/browser/logging.js";

const log = createLogger(import.meta.url);

const reportedKeys = new Set<string>();

function reportFailure(action: string, key: string, error: unknown): void {
  // Once per key is enough: a caller that stores under a key typically does so on every change it
  // makes, and a storage that refuses one call refuses the next just the same.
  if (reportedKeys.has(key)) {
    return;
  }

  reportedKeys.add(key);
  log.warn(`Failed to ${action} under ${key}.`, error instanceof Error ? error.stack : String(error));
}

/** Stores a value under a key, or does nothing if storage cannot be reached or refuses the write. */
export function setItem(key: string, value: string): void {
  try {
    window.localStorage.setItem(key, value);
  } catch (error: unknown) {
    reportFailure("store a value", key, error);
  }
}

/** Reads the value stored under a key, or null if none was stored or storage cannot be reached. */
export function getItem(key: string): string | null {
  try {
    return window.localStorage.getItem(key);
  } catch (error: unknown) {
    reportFailure("read the value stored", key, error);

    return null;
  }
}
