# Plugins

Plugins are an important concept in Christofel.
They can be loaded or unloaded during runtime of Christofel application.
Additionally, at the start of application the plugins specified in config
are automatically loaded. After that, user can request load of a plugin
through a command in Discord. Christofel doesn't support any other managing
way, only through Discord.

Plugins have to implement [lifetime](lifetime)
for the Application to be able to handle their
state like error or stop and successfully detaching them.

All runtime plugins are required to have a [lifetime](lifetime).
Currently only runtime plugins (`IRuntimePlugin`) are supported in Christofel,
through `RuntimePluginService` class.

```{note}
When plugin's lifetime Cancels `Stopped` CancellationToken,
automatic detach sequence will be initiated and the plugin
will be unloaded from memory.
```

For assembly to be counted as a Plugin, it must
have a public class that implements `IPlugin` interface
from `Christofel.Plugins.Abstractions`. Additionally it
should actually be `IRuntimePlugin` that it implements
as other types of plugins aren't supported in the application.
Christofel contains
some useful helper classes to get started faster. These are located
in `Christofel.Helpers` and `Christofel.Plugins`. The most important ones
are `DIPlugin` and `PluginLifetimeHandler`. Both of these are demonstrated
below.

## Runtime plugins

Runtime plugins are the kinds of plugins that have their own lifetime.
That means they can be attached and detached. You can see the interface
of every runtime plugin in `IRuntimePlugin`.

Such plugin has its own `Lifetime`, method `RunAsync` that is called upon start of the plugin,
and method `RefereshAsync` that should look for configuration changes that cannot be captured
automatically, and refresh respective parts of the plugin so that.

Additionally a runtime plugin can have its own context, that is what `IRuntimePlugin<TState, TContext>` is about.
The state is the state of the application, given to the plugin upon initialization. That is what the `InitAsync`
method is for. This state is shared between all plugins. In Christofel, it (`IChristofelState`) holds information about the
discord bot (gateway connection, discord apis http client), the base context configuration, configuration from `config.json`,
logger factory, permission service and lifetime of the application. Then, the plugin has its own `Context` property,
the context may be used by the application.
In Christofel, the context is used for responding to gateway events, specifically
`PluginResopnder` is called whenever a new event is received by the base application that is connected to the gateway.
There is an abstract implementation of a runtime plugin available, `DIRuntimePlugin<TState, TContext>`.
This is an implementation relying on the dependency injection from `Microsoft.Extensions.DependencyInjection`.

### Services

In Christofel, to run code on start of a plugin, on refresh of a plugin and on stop of a plugin,
special interfaces have been made - `IStartable`, `IRefreshable` and `IStoppable`. Those interfaces
have methods similar to methods of a runtime plugin. It is responsibility of a runtime plugin to call
such methods from its startable, refreshable and stoppable services.

With the DI runtime plugin, this is done automatically when you register a service under one of those
interfaces. An extension method `AdStateful<TStateful>` might be used for this, so if you make
a class that should do something on start, like register slash commands, you implement `IStartable`,
and then add it to the service collection using `.AddStateful<SlashCommandRegistration>()`.

## How to create a simple plugin

### Dependency Injection plugin
Creating plugin with a service collection using dependency injection
from `Microsoft.Extensions.DependencyInjection` should be quite easy, thanks
to the class `DIRuntimePlugin` that has been prepared in `Christofel.Helpers`.
This class handles lifetime state of the plugin by itself.
The plugin has to only implement configuration of services.

Working plugin class is presented below along with some comments
to better explain the code.

```{code-block} csharp
:lineno-start: 1
public class MyPlugin : DIRuntimePlugin<State, Context>
{
    // LifetimeHandler stores lifetime and exposes
    // methods that can change Lifetime state
    private readonly PluginLifetimeHandler _lifetimeHandler;

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

    // This one is used in DIRuntimePlugin to manage lifetime
    protected override LifetimeHandler LifetimeHandler => _lifetimeHandler;

    // Called during Init when configuring ServiceCollection before building ServiceProvider
    protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
    {
        return serviceCollection
            // Registers all classes that are needed for the plugin
            // namely: IChristofelState, IConfiguration, IPermissionService, IPermissionResolver, IBot, DiscordSocketClient, IApplicationLifetime, ILoggerFactory, ILogger<>
            // and Discord api services
            .AddDiscordState(State)
            .AddSingleton<ICurrentPluginLifetime>(_lifetimeHandler.LifetimeSpecific)
            // All services can be registered here.
            //.AddStateful<MyService>() // Let's say MyService is IStartable, then it should be registered with AddStateful.
            //.AddTransient<DatabaseChecker>()
            // Configure may be used to register options and to get a section from configuration
            // State.Configuration can be used, this is state from the base application
            // Respond to gateway events.
            .AddSingleton<PluginResponder>()
            // Register other services that respond to events, see Remora.Discord documentation.
            //.AddResponder<MyMessageResponder>()
            //.Configure<SomeOptions>(State.Configuration.GetSection("MySection"));
            ;
    }

    // This may do custom initialization of services if any is needed
    protected override Task InitializeServices(IServiceProvider services, CancellationToken token = new CancellationToken())
    {
        // set _logger for handling of errors or stopped state
        _logger = services.GetRequiredService<ILogger<HelloworldPlugin>>();
        // Respond to gateway events - this will be called by the base application when it receives an event.
        ((PluginContext)Context).PluginResponder = services.GetRequiredService<PluginResponder>();
        return Task.CompletedTask;
    }
}
```

For more information, look at `DIRuntimePlugin` methods and check out plugins already existing in the library.

### Custom plugin

```{note}
Plugins other than runtime plugins are not supported by Christofel. Christofel
will try to instantiate the plugin class, but it will fail as it won't be able
to find the handler for other types of plugins. Additionally,
only the `IRuntimePlugin<IChristofelState, IPluginContext>` type of runtime plugins is supported.
```

For custom plugin, interface `IRuntimePlugin<IChristofelState, IPluginContext>` must be implemented.
Lifetime support must be provided.

This example provides basic implementation.
Any approach can be used as long as few rules
are observed.

1. InitAsync and RunAsync should not do anything heavy and should end as soon as possible. If heavy operation is needed, just spin it in a new thread
2. Plugin must react to stop request and destroy its resources if it's possible
3. Name must match assembly name (or rather name of the dll)

```{code-block} csharp
:lineno-start: 1
public class CustomPlugin : IRuntimePlugin<IChristofelState, IPluginContext>
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
