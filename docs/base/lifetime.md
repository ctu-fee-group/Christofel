# Lifetime

A lifetime means a set of states that a runnable unit can have during the time this unit runs.
It depicts states from the startup to disposal of the service.

There is a lifetime for the whole application and then one per plugin.
It can be used for checking what state a plugin is in, registering callbacks
after certain state is reached. You can register callbacks on
starting, before stopping, after stopped, or when there is an error state that
prevents the plugin from operating properly.

The state machine progresses through these states:
Startup → Initializing → Initialized → Starting → Running → Stopping → Stopped → Destroyed.

Lifetime interfaces are located in `Christofel.Plugins.Abstractions`, the main interface
is `ILifetime` and there are a few others for helping to distinguish use-cases.
That can be useful in DI for example. These are `ILifetime<T>`, `IApplicationLifetime`, `ICurrentPluginLifetime`.
They expose the same interface as `ILifetime`, so they don't have any special features other than identifying
what the lifetime belongs to. `ICurrentPluginLifetime` might be requested inside services of a plugin
to get lifetime of the plugin the service belongs to.

Callbacks can be registered using `CancellationToken`s that are exposed in
`ILifetime`. These are exposed as `Started`, `Stopping` (before stop), `Stopped` (after stop) and `Errored` (unhandled exception).

Property `State` is also exposed, showing what state the service is in at the current time.
It can be used for busy waiting until specified state happens, so it should be up to date every time it's accessed.

Method `RequestStop` is used for requesting plugin to be stopped. This method should finish quickly.
It's only a request for a stop, stop will be done as soon as possible. If stop is not possible,
the plugin will hang in memory forever. This should generally be avoided - the plugins should be made in such a way to
allow detaching.

## Lifetime handler
A lifetime handler is a manager that manages given lifetime - advances its states, registers callbacks
and ensures they are called at appropriate time.

There is an abstract implementation - `LifetimeHandler<T>`. It is located in `Christofel.Plugins`.
`PluginLifetimeHandler` may be used as a default handler for a plugin (using `ICurrentPluginLifetime`).

Handlers have to be thread-safe, it holds the lifetime and cancels the appropriate
`CancellationToken` when advancing states.

## Plugin lifetime

Each method should check whether it's in correct state (`Startup` before `InitAsync`, `Initialized` before `RunAsync`).
- on `InitAsync`, advance to `Initializing`, initialize, advance to `Initialized`
- on `RunAsync`, set `Starting,` start, advance to `Running`
- `RequestStop` should move through the states to `Destroyed`, disposing(destroying) every dependency

Take into account that `RequestStop` should destroy everything even if the plugin is not running.

`DIPlugin` handles most of the states by itself. If you override the methods, don't forget to manage the lifetime correctly.
