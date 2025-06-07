# Plugins

Plugins are important concept of Christofel.
They can be loaded or unloaded at any time wished.

Plugins have to implement [lifetime](lifetime)
for the Application to be able to handle their
state like error or stop and successfully detaching them.

```{note}
When plugin's lifetime Cancels `Stopped` CancellationToken,
automatic detach sequence will be initiated and the plugin
will be unloaded from memory.
```

For assembly to be counted as Plugin it must
have public class that implements `IPlugin` interface
from `Christofel.BaseLib`. Implementation of this plugin
can be whatever the programmer wants. Christofel contains
some useful classes to get started faster. These are located
in `Christofel.BaseLib.Implementations`. The most important ones
are `DIPlugin` and `PluginLifetimeHandler`. Both of these are demonstrated
below.

## How to create a simple plugin


### Dependency Injection plugin
Creating plugin with dependency injection is quite easy, because
class `DIPlugin` was prepared in `Christofel.BaseLib.Implementations`.
This class handles lifetime state of the plugin by itself. 

Working plugin class is presented below along with some comments
to better explain the code.

```{code-block} csharp
:lineno-start: 1
public class MyPlugin : DIPlugin
{
    // LifetimeHandler stores lifetime and exposes
    // methods that can change Lifetime state
    private PluginLifetimeHandler _lifetimeHandler;

    // Hold the application logger
    private ILogger<HelloworldPlugin>? _logger;

    public MyPlugin()
    {
        // Create LifetimeHandler passing it default action handlers
        _lifetimeHandler = new PluginLifetimeHandler(
            // Error Handler that is called when Errored state is set 
            // This default one exposed by DIPlugin
            // logs the contents to _logger and requests a stop
            DefaultHandleError(() => _logger),
            // Stop Request handler that is called on StopRequest method
            // This one logs the information to _logger
            // and then calls StopAsync and DestroyAsync methods
            // of DIPlugin.
            DefaultHandleStopRequest(() => _logger));
    }

    public override string Name => "Example"; // Name must match the assembly name (or rather match the name of the dll)
    public override string Description => "Just an example"; // Short description of the plugin do be displayed to user if he wishes
    public override string Version => "v1.0.0"; // Version of the plugin for verification purposes. Can expose the assembly version

    // Refreshable, Stoppable and Startable should return services that are used
    // We are not using any services here, so they return empty Enumerables.
    // Any services exposed from here will get their methods called on RunAsync, RefreshAsync, StopAsync respectively

    protected override IEnumerable<IStartable> Startable => Enumerable.Empty<IStartable>(); // Calls StartAsync on them when RunASync is called on this plugin
    protected override IEnumerable<IRefreshable> Refreshable => Enumerable.Empty<IRefreshable>(); // Calls RefreshAsync on them when RefreshAsync is called on this plugin
    protected override IEnumerable<IStoppable> Stoppable => Enumerable.Empty<IStoppable>(); // Calls StopAsync on them when StopAsync is called on this plugin


    // This one is used in DIPlugin to manage lifetime
    protected override LifetimeHandler LifetimeHandler => _lifetimeHandler;

    // Called during Init when configuring ServiceCollection before building ServiceProvider
    protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
    {
        return serviceCollection
            // Registers all classes that are needed for the plugin
            // namely: IChristofelState, IConfiguration, IPermissionService, IPermissionResolver, IBot, DiscordSocketClient, IApplicationLifetime, ILoggerFactory, ILogger<>
            .AddDiscordState(State)
            .AddSingleton<ICurrentPluginLifetime>(_lifetimeHandler.LifetimeSpecific)
            // All services can be registered here
            //.AddSingleton<MyService>
            // Configure may be used to register options and to get a section from configuration
            // State.Configuration can be used
            //.Configure<SomeOptions>(State.Configuration.GetSection("MySection"));
            ;
    }

    // This may do custom initialization of services if any is needed
    protected override Task InitializeServices(IServiceProvider services, CancellationToken token = new CancellationToken())
    {
        // set _logger for handling of errors or stopped state
        _logger = services.GetRequiredService<ILogger<HelloworldPlugin>>();
        return Task.CompletedTask;
    }
}
```

For more information, look at `DIPlugin` methods and check out plugins already existing in the library.

### Custom plugin

For custom plugin, interface `IPlugin` must be implemented.
Lifetime support must be provided.

This example provides basic implementation.
Any approach can be used as long as few rules
are observed.

1. InitAsync and RunAsync should not do anything heavy and should end as soon as possible. If heavy operation is needed, just spin it in a new thread
2. Plugin must react to stop request and destroy its resources if it's possible
3. Name must match assembly name (or rather name of the dll)

```{code-block} csharp
:lineno-start: 1
public class CustomPlugin : IPlugin
{
    private readonly PluginLifetimeHandler _lifetimeHandler;
    private IChristofelState? _state;

    public CustomPlugin()
    {
        _lifetimeHandler = new PluginLifetimeHandler((e) =>
        {
            // handle error
            Console.WriteLine(e);
            _lifetimeHandler?.RequestStop();
        }, () =>
        {
            if (_lifetimeHandler?.State == LifetimeState.Running)
            {
                // There is nothing needed to be destroyed, so just
                // enumerate the states so we trigger stopping, stopped
                // cancellation token callbacks
                _lifetimeHandler.NextState(); // Stopping
                _lifetimeHandler.NextState(); // Stopped
                _lifetimeHandler.NextState(); // Destroyed
            } else {
                // Somehow handle state where the plugin wasn't started
            }
        });
    }

    // This is the same as in DI Plugin
    public string Name => "CustomPluginExample";
    public string Description => "Just an example";
    public string Version => "v1.0.0";
    public ILifetime Lifetime => _lifetimeHandler.Lifetime;

    // Called as initialization call passing us the state
    public Task InitAsync(IChristofelState state, CancellationToken token = new CancellationToken())
    {
        if (_lifetimeHandler.State != LifetimeState.Startup)
        {
            // Already initialized
            return Task.CompletedTask;
        }

        // LifetimeHandler moves to the next state and manages
        // actions that should happen
        _lifetimeHandler.NextState(); // Move to Initializing
        _state = state;
        _lifetimeHandler.NextState(); // move to Initialized

        return Task.CompletedTask;
    }

    // Usually called right after InitAsync, this should start the operation of current
    // plugin.
    public Task RunAsync(CancellationToken token = new CancellationToken())
    {
        if (_lifetimeHandler.State != LifetimeState.Initialized)
        {
            // Not initialized or already past running
            return Task.CompletedTask;
        }

        _lifetimeHandler.NextState(); // move to Starting
        _state?.LoggerFactory
            .CreateLogger("CustomPlugin")
            .LogInformation("Hello world from custom plugin.");
        _lifetimeHandler.NextState(); // move to Running

        return Task.CompletedTask;
    }

    public Task RefreshAsync(CancellationToken token = new CancellationToken())
    {
        // Nothing we can refresh
        return Task.CompletedTask;
    }
}
```

### Command handlers

Christofel contains helpers for handling slash commands in `Christofel.CommandsLib` assembly.
More information will follow as this library will settle.

