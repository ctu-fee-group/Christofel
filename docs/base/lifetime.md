# Lifetime

Lifetime in Christofel must be used for the whole application and for each plugin.
It can be used for checking on the state of the plugin, registering callbacks
after starting, before stopping, after stopped, after errored.

Lifetime interfaces are located in `Christofel.BaseLib`, the main interface
is `ILifetime` and there are a few others for helping to distinguish use-cases.
That can be useful in DI for example. These are `ILifetime<T>`, `IApplicationLifetime`, `ICurrentPluginLifetime`.
They expose the same interface as `ILifetime`, so they don't have any special features other than identifying
what the lifetime belongs to.

Callbacks can be registered using `CancellationToken`s that are exposed in
`ILifetime`. These are exposed as `Started`, `Stopping`, `Stopped` and `Errored`.
The names should be self-explanatory.

Property `State` is also exposed, showing what state the service is in at the current time.
It can be used for waiting to specified state, so it should be up to date every time it's accessed.

Method `RequestStop` is used for requesting a stop. This method should finish quickly.
It's only a request for a stop, stop will be done as soon as possible. If stop is not possible,
the plugin will hang in memory forever.

## Lifetime handler
`LifetimeHandler<T>` can be used for managing lifetime easily. It is located in
`Christofel.BaseLib.Implementations`. `PluginLifetimeHandler` can be used as a default
handler for `ICurrentPluginLifetime`.

Handler should be thread-safe, it holds the lifetime and triggers CancellationToken Cancels when needed.

## Plugin lifetime

Each method should check whether it's in correct state (Startup before `InitAsync`, Initialized before `RunAsync`).
- on `InitAsync`, set Initializing, initialize, set Initialized
- on `RunAsync`, set Starting, start, set Running
- `RequestStop` should move through the states to `Destroyed`, destroy every dependency

Take into account that `RequestStop` should destroy everything even if the plugin is not running.

`DIPlugin` handles most of the states by itself. If you override the methods, don't forget to manage the lifetime correctly
