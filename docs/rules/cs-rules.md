# C# rules

## Contents

- [Options are immutable](#options-are-immutable)

## Options are immutable

An Options type exposes its values as `{ get; init; }`, so a service's configuration cannot change
underneath it. The instance is what is immutable — `IOptionsMonitor<T>` may still hand out a new one
on reload.

The binder sets `init` properties by reflection, so configuration binding works as before; mutating
an existing instance does not. Configure through `IConfiguration`, in tests too, instead of
`services.Configure<T>(options => …)`. A positional `record` cannot bind — the binder needs a
parameterless constructor.
