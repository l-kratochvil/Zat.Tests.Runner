// A check that fails reports the mismatch and answers null; it is up to the caller to leave. That
// keeps the decision where the context is - only the function itself knows whether a value it did
// not get is a broken call or an ordinary one.

import { createLogger, type Logger } from "/browser/logging.js";

/**
 * The typeof names worth asking about, each mapped to what a value of that name is.
 *
 * "object" is missing because it answers for null and for every class alike, so it would let
 * through the values a caller is most likely to get wrong - ofInstance is the question to ask
 * about those. "undefined" is missing because a value that is only ever absent carries nothing to
 * check.
 */
interface TypeByName {
  string: string;
  number: number;
  boolean: boolean;
  bigint: bigint;
  symbol: symbol;
  // typeof cannot see a signature, so this says no more than "callable"; what it takes and returns
  // stays the caller's own claim.
  function: (...args: never[]) => unknown;
}

// Built on the first failure rather than while this module is evaluating. logging.ts is free to
// start guarding its own parameters through this module, which would leave the two importing each
// other; asking for the logger only once a guard has already failed means both modules are fully
// evaluated by then, and the cycle is never observed.
let log: Logger | null = null;

/**
 * Checks a value against a typeof name.
 *
 * @returns The value, typed as what was asked for, or null if it is something else.
 */
export function ofType<TName extends keyof TypeByName>(value: unknown, type: TName): TypeByName[TName] | null {
  if (typeof value === type) {
    return value as TypeByName[TName];
  }

  reject(`Expected ${type}, got ${describe(value)}.`);

  return null;
}

/**
 * Checks a value against a class.
 *
 * @param constructor - The class itself, as a value. A type parameter alone could not do this:
 * TypeScript erases it, leaving nothing at runtime to check against.
 * @returns The value, typed as an instance, or null if it is not one.
 */
export function ofInstance<T>(value: unknown, constructor: abstract new (...args: never[]) => T): T | null {
  if (value instanceof constructor) {
    return value;
  }

  reject(`Expected ${constructor.name}, got ${describe(value)}.`);

  return null;
}

function reject(message: string): void {
  log ??= createLogger(import.meta.url);

  // The stack is the only thing naming the function that was called wrongly: a guard is handed a
  // value, never the caller it came from.
  log.warn(message, new Error().stack);
}

/** Names what a value turned out to be, closely enough to recognise the mistake from the log. */
function describe(value: unknown): string {
  if (value === null) {
    return "null";
  }

  if (typeof value !== "object") {
    return typeof value;
  }

  // A class name says far more than "object", but an object need not have one: a null prototype
  // leaves no constructor to ask.
  return (value as { constructor?: { name?: string } }).constructor?.name ?? "object";
}
