# Christofel.Helpers

Christofel.Helpers is a hot-swappable utility library. Unlike `Christofel.Common` which maintains ABI compatibility,
this library can be updated and reloaded without restarting the application.

This library serves as the foundation for productive Christofel plugin development,
providing the essential utilities and infrastructure needed to create plugins.

## Overview

This library contains utilities, extensions, and helper services commonly needed when developing Christofel plugins. The hot-swappable nature allows for rapid iteration and deployment of utility improvements during development.

**Key characteristics:**
- Hot-swappable without application restart
- Comprehensive plugin development utilities
- Service registration and dependency injection helpers
- Extensions for Discord and database operations
- Localization
- Scheduling

## Plugin Infrastructure

### ChristofelDIPlugin

Base class for creating dependency injection-based plugins. Located in the `Christofel.BaseLib.Plugins` namespace for backward compatibility.

```csharp
public abstract class ChristofelDIPlugin : DIRuntimePlugin<IChristofelState, IPluginContext>
{
    protected override IPluginContext InitializeContext() => new PluginContext();
}
```

See [plugins](../base/plugins.md) for more information.

## Service Collection Extensions

### AddDiscordState

Central extension method for registering all Discord-related services in a plugin's dependency injection container.

**Registers:**
- `IChristofelState` and all its properties
- Discord API clients (REST, Gateway)
- Database contexts with proper connection strings
- Logging infrastructure
- Caching services
- JSON serializer configuration

```csharp
protected override IServiceCollection ConfigureServices(IServiceCollection services)
{
    return services.AddDiscordState(State);
}
```

### Additional DI Extensions

- Configuration integration with `IOptions<T>` pattern
- Database context registration with connection string management
- HTTP client configuration for Discord APIs

## Date and Time Management

### DateTimeProvider System

Provides timezone-aware date and time operations based on application
configuration. The main idea behind these is to make the time critical
tasks better testable, by not relying on the `DateTime.Now` global property.

**Components:**
- `IDateTimeProvider` - Main interface for timezone-aware operations
- `IDateTimeBaseProvider` - System and UTC time access
- `DateTimeProvider` - Default implementation with timezone support
- `TimeOptions` - Configuration for timezone and culture settings

**Features:**
- Culture-specific date formatting
- Consistent time handling across all plugins
- Easier testability

## Cron Job System

For recurring tasks, a simple cron job system is provided.
Each cron job has a common interface, `ICronJob`.
Note that it should also implement `IStartable` and `IStoppable`
for starting the cron job upon starting a plugin.
There is an implementation that should cover most use cases.

### SimpleCronJob

Abstract base class for implementing scheduled background tasks.
Uses interval-based scheduling of cron jobs. The cron jobs are
initially started on start of the application, and then recurrently
executed with the given interval.

### CronRepository

A repository for registering cron jobs using service collection.
There is a helper extension method `AddCron<TCron>` that will add
this repository automatically. This repository can be used for querying
cron jobs of a plugin.

## Job Queue Infrastructure

Mainly for the authentication, there is a job queue implementation.
This is because Discord has rate limits and as such, it makes no sense to
call a lot of discord api calls, such as adding roles to a lot of users at once.
That is why there is a queue of role changes that schedules them consecutively.

### ThreadPoolJobQueue<TJob>

Abstract base class for background job processing using the thread pool.

```csharp
public abstract class ThreadPoolJobQueue<TJob> : IJobQueue<TJob>
{
    protected abstract Task ProcessJobAsync(TJob job, CancellationToken ct);
}
```

**Characteristics:**
- Creates threads only when jobs are pending
- Queue-based job management
- Lifetime-aware (stops with plugin shutdown)
- Generic job type support
- Proper resource cleanup

## Extension Methods

### User and Role Extensions

Extensions for working with Discord users and roles in the context of Christofel permissions.

`IUserExtensions`
- `ToDiscordTarget()` - Converts Discord user to permission target
- `GetAllDiscordTargets()` - Extracts all permission targets from guild members
  - Extracts all roles of a user, etc.

`IRoleExtensions`
- `ToDiscordTarget()` - Converts Discord role to permission target

### Database Query Extensions

LINQ extensions for common database operations.

`UserQueryExtensions:`
- `Authenticated()` - Look up only authenticated users

`PermissionAssignmentQueryExtensions`
- Permission resolution helpers
- Obtain permissions for target or multiple targets

## Localization System

For user-facing messages, there is a localization system. This is mostly
to cover non-czech speaking users. For those, there are english variants
of messages. The system available in Helpers uses key-based system where
individual json keys are looked up for translations. There are no translations
in code, not even the english version, every message comes externally from
a json file. If the message is not available, there is a fallback to the key only,
this is not meant to be readable by end users though.

### JSON-Based Localization

Complete localization infrastructure using JSON resource files.

**Components:**
- `JsonStringLocalizer` - String localization from JSON files
- `JsonStringLocalizerFactory` - Creates localizers for different resources
- `JsonResourceManager` - Manages JSON resource files
- `LocalizationOptions` - Configuration for supported languages

### Culture Management

**ICultureProvider:**
- Determines user's preferred culture
- Integration with Discord user preferences

**DefaultCultureProvider:**
- Fallback culture resolution
- Integration with `DateTimeProvider` for consistent formatting

### Usage Example

The ManagementPlugin demonstrates typical localization setup and usage:

**Service registration:**
```csharp
protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
{
    return serviceCollection
        // Other services...
        .AddJsonLocalization()
        .Configure<LocalizationOptions>(State.Configuration.GetSection("Localization"))
        .AddSingleton<ICultureProvider, DefaultCultureProvider>();
}
```

**Configuration (config.json):**
```json
{
  "Localization": {
    "ResourcesPath": "resources",
    "DefaultLanguage": "cs",
    "SupportedLanguages": ["cs", "en"]
  }
}
```

**Resource files:**
- `resources/Christofel.Management.ManagementPlugin.json` (English):
```json
[
  {
    "Name": "SELFTIMEOUT_SUCCESSFUL",
    "Value": "User {0} has timeouted themselves for {1}, the timeout will expire at {2}."
  }
]
```

- `resources/Christofel.Management.ManagementPlugin.cs.json` (Czech):
```json
[
  {
    "Name": "SELFTIMEOUT_SUCCESSFUL",
    "Value": "Uživatel {0} si nastavil timeout na {1}, timeout vyprší {2}."
  }
]
```

**Usage in commands:**
```csharp
public class SelfManagementCommands : CommandGroup
{
    private readonly LocalizedStringLocalizer<ManagementPlugin> _localizer;

    public SelfManagementCommands(LocalizedStringLocalizer<ManagementPlugin> localizer)
    {
        _localizer = localizer;
    }

    [Command("selftimeout")]
    public async Task<Result<IReadOnlyList<IMessage>>> HandleSelfTimeoutAsync(TimeoutUntilSpecification timeoutUntil)
    {
        // Command logic...

        return await _feedback.SendContextualSuccessAsync(
            _localizer.Translate(
                "SELFTIMEOUT_SUCCESSFUL",
                $"<@{userId}>",
                Markdown.Timestamp(timeoutUntil, TimestampStyle.RelativeTime),
                Markdown.Timestamp(timeoutUntil, TimestampStyle.ShortDateTime)));
    }
}
```

The system automatically selects the appropriate language based on user culture and falls back to the default language if a translation is not available.

## Storage Abstractions

### Thread-Safe Storage

Collection abstractions for concurrent access scenarios.

ThreadSafeListStorage<T>`
- Lock-based storage implementation
- Suitable for low-volume read/write scenarios
- Uses `List<T>` internally with synchronization

`ThreadSafeImmutableArrayStorage<T>`
- Optimized for high-read, low-write scenarios
- Copy-on-write semantics
- Better performance for frequent reads

`IThreadSafeStorage<T>`
- Common interface for both implementations
- Provides `Add`, `Remove`, and enumeration operations

## Configuration and Options

### BotOptions

Central configuration class for bot-wide settings.

```csharp
public class BotOptions
{
    public ulong GuildId { get; set; }
}
```

Accessed through `IOptions<BotOptions>` pattern with support for configuration changes.

## Error Types and Validation

### Custom Error Types

`FormatError`
- Used for formatting and parsing errors
- Integrates with Remora.Results pattern

`UnexpectedContextError`
- Indicates invalid execution contexts
- Provides structured error information

### Helper Utilities

`AllowedMentionsHelper`
- `All` and `None` mentions allowed

`EmojiFormatter`
- Discord emoji formatting utilities
- Custom emoji handling

## Database Integration

### ReadOnly Database Access

`ReadonlyDbContextFactory<T>`
- Creates read-only database contexts
- Prevents accidental write operations
- Proper connection string management
- Used for query-only database operations
- Less heavy - no tracking

There is an extension method `.AddReadOnlyDbContext<TContext>()`
for service collection. The context itself has to already implement
the interface, such as
```csharp
    /// <inheritdoc/>
    IQueryable<TEntity> IReadableDbContext.Set<TEntity>()
        where TEntity : class => Set<TEntity>().AsNoTracking();
```

## Architecture Integration

### Hot-Swapping Support

The library maintains backward compatibility through legacy namespace usage while supporting dynamic reloading:

- Uses `Christofel.BaseLib.*` namespaces for compatibility
- Core interfaces remain stable in `Christofel.Common`
- Service registrations can be updated without affecting running plugins

### Plugin Development Integration

- Complete DI container setup through `AddDiscordState()`
- Lifecycle management with plugin start/stop integration
- Configuration hot-reload support through `IOptionsMonitor<T>`
