# Configuration

Christofel uses `Microsoft.Extensions.Configuration` for configuration management.
Configuration is loaded from `config.json` and environment-specific `config.{environment}.json` files.
The files are loaded from the current working directory (PWD), with the environment determined by the `ENV` environment variable.

## Runtime Configuration Updates

Support for changing configuration at runtime should be added where possible:
- Use `IOptionsSnapshot<T>` for scoped and transient services
- Use `IOptionsMonitor<T>` with proper update handling for singleton services

```{note}
Some settings like guild ID and bot token cannot be changed at runtime due to deep integration with application state.
```

## Usage in Code

In plugins using dependency injection, configure options using `Configure` extension methods on `IServiceCollection`.
Access configuration through `IChristofelState.Configuration`.

For more information about the options pattern, see the [official documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/options).

## Configuration Structure

For a complete example configuration file, see [config.json](https://github.com/ctu-fee-group/Christofel/blob/dev/src/config.json) in the repository.

## Configuration Fields

### ConnectionStrings
- `ChristofelBase` - Connection string for `ChristofelBaseContext` database

### Bot
- `GuildId` - Main guild ID where the bot operates
- `Token` - Discord application token
- `DiscordNet` - Direct configuration of `DiscordSocketClientOptions`
  - See `DiscordSocketClientOptions` in Discord.NET documentation
  - Set `AlwaysAcknowledgeInteractions` to `false` if ephemeral responses are needed

### Plugins
- `Folder` - Directory relative to executable containing plugin assemblies
- `AutoLoad` - Array of plugin names to load automatically on startup

### Logging
Standard `Microsoft.Extensions.Logging` configuration. See [.NET logging documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging) for details.

- `File` - File logging provider configuration ([Karambolo.Extensions.Logging.File](https://github.com/adams85/filelogger))
- `Console` - Console logger provider configuration
- `Discord` - Custom Discord channel logging
  - `MaxQueueSize` - Maximum queued log messages
  - `Channels` - Array of Discord channels for log output
    - `GuildId` - Target guild ID
    - `ChannelId` - Target channel ID
    - `MinLevel` - Minimum log level for this channel
