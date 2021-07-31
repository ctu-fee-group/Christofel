# Configuration
For configuration, `Microsoft.Extensions.Configuration` is used.
The configuration is stored in `config.json` and `config.{environment}.json`
in the same directory where executable is. Variable `environment` is retrieved
from environment variable `ENV`.

Support for changing the configuration at runtime should be added where it's possible.
This support is not added for changing the main guild id or bot token.

## Usage in code
In plugins, where DI is used, the configuration can be added using
`Configure` extension methods for `IServiceCollection`. The
configuration that should be used is located in `IChristofelState.Configuration`.
More information about how options work can be found here: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0

## Documented Fields
Access to nested levels is achieved using dots in between level names.

- `ConnectionStrings`
  - `ChristofelBase` - Connection string for `ChristofelBaseContext`
- `Bot`
  - `GuildId` - main guild id where the bot should be used
  - `Token` - application token
  `DiscordNet` - direct configuration of `DiscordSocketClientOptions`
    - see `DiscordSocketClientOptions` in Discord.NET
    - if ephemeral responses are needed, `AlwaysAcknowledgeInteractions` have to be `false`.
- `Plugins`
  - `Folder` - folder relative to the executable where plugins reside
    - `AutoLoad` - array of strings that controls what plugins will be loaded on startup automatically.
- `Logging`
  - this is a configuration of `Microsoft.Extensions.Logging`, for more information try this: https://docs.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line
  - `File`
    - Configuration of `Karambolo.Extensions.Logging.File` provider
    - documentation https://github.com/adams85/filelogger
  - `Console`
    - default `Console` logger provider configuration
  - `Discord`
    - custom Discord logger provider
    - `MaxQueueSize` - maximum number of messages in queue for processing
    - `Channels` - specifies array of channels that the bot should log into
      - entry example
        - `GuildId` - specifies in which guild the channel is located
        - `ChannelId` - specifies which channel to log into
        - `MinLevel` - minimal log level, can be used to separate channels for logging info and only errors
