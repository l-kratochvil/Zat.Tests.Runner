// What .NET reaches when it wires the browser up: the JsBridge component calls the functions here
// from its lifecycle, and they pass the call on to the module that does the work. Logging is the
// only thing registered today; another module needing a reference back into .NET is registered
// from here too, rather than being called by the component directly.
//
// This is the .NET boundary, so the arguments arrive untyped and are checked before they are used
// (see docs/rules/js-ts-rules.md). Past this point they are ordinary typed values.
//
// The imports name the modules by the URL they are served under, exactly as the import map
// spells them: any other spelling would load a second copy of logging.js, holding a reference of
// its own that nobody ever registers.

import { ofType } from "/browser/guards.js";
import {
  register as registerLogging,
  unregister as unregisterLogging,
  type BrowserLoggerReference,
} from "/browser/logging.js";

/**
 * Hands the logging module the reference its diagnostics are sent through.
 *
 * @param logger - A DotNetObjectReference<BrowserLogger>, as it arrives from .NET.
 */
export function register(logger: unknown): void {
  // BrowserLoggerReference is an interface, so there is no class to hold the value against:
  // ofInstance has nothing to be given. What makes a reference usable is the single method every
  // call back into .NET goes through, so that is what is asked about.
  const candidate = logger as { invokeMethodAsync?: unknown } | null | undefined;

  if (ofType(candidate?.invokeMethodAsync, "function") === null) {
    return;
  }

  registerLogging(logger as BrowserLoggerReference);
}

/** Tells the logging module to give the reference up, before .NET disposes it. */
export function unregister(): void {
  unregisterLogging();
}
