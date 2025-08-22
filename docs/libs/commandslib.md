# Christofel.CommandsLib

Christofel.CommandsLib is a library that extends `Remora.Commands` and `Remora.Discord.Commands` to provide Discord slash command functionality
with integrated Christofel permission system, validation, and plugin lifecycle management.
This library is expected to be used inside of Christofel plugins.

## Overview

- **Permission-aware commands** using Christofel's database-backed permission system
- **Automatic command lifecycle management** that integrates with plugin hot-swapping
- **Validation system** using `FluentValidation` with user-friendly error messages
- **Error handling** with contextual Discord responses

## Key Components

### Command Registration and Management

`ChristofelSlashService`
- Central service for registering slash commands with Discord API
- Reimplementation of Remora's `SlashCommandService`
- Registers commands one-by-one instead of the bulk registration to prevent sharing state between plugins
  - The bulk endpoint removes commands that weren't mentioned. This would mean plugins have to use a common place to register commands.

`ChristofelCommandRegistrator`
- Implements `IStartable`, `IRefreshable`, `IStoppable` for plugin lifecycle integration
- **On Start**: Registers all commands with Discord
- **On Stop**: Cleans up commands when plugin is detached (unless application is shutting down).

```{note}
There is a limitation of 100 command registrations per day per guild. If you reattach a plugin, the
commands are removed and registered again. This isn't done if the whole application is restarted.
```

### Permission System

`RequirePermissionAttribute`
```csharp
[RequirePermission("admin.manage")]
[Command("manage")]
public async Task<Result> HandleManageAsync() { ... }
```

will be used to check if user has permission for executing the given command.
Every parent node of a command is checked for permissions as well, so user has
to have all the permissions required. For example if a class `AdminCommands` has
`[RequirePermission("admin")]`, and then a command has `RequirePermission("admin.manage")`,
the user has to have both `admin` and `admin.manage` permissions.

**Permission Resolution Flow**:
1. `RequirePermissionCondition` checks if user has required Christofel permission (there isn't a response to users who do not have permissions deliberately, to not let them be aware of valid commands)
2. `ChristofelCommandPermissionResolver` queries the database for user/role assignments

### Validation Framework

`CommandValidator`
FluentValidation-based parameter validation:
```csharp
var validator = new CommandValidator()
    .MakeSure("count", count, c => c.InclusiveBetween(1, 100))
    .MakeSure("userId", userId, u => u.NotEmpty().WithMessage("User ID required"));

if (!validator.IsValid)
    return await _validationFeedback.SendValidationErrorAsync(validator.Errors);
```

`ValidationFeedbackService`
- Converts validation errors to user-friendly Discord embeds
- Provides contextual error messages for command parameters

### Error Handling Events

`ErrorExecutionEvent`
- Logs command execution errors for debugging

`WrongParametersExecutionEvent`
- Provides helpful error messages when command parameters are incorrect
- Configurable through `WrongParametersEventOptions`

`ParsingErrorExecutionEvent`
- Handles command parsing failures with user-friendly responses

## Usage in Plugins

### Basic Setup

```csharp
protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
{
    return serviceCollection
        // Register command services
        .AddChristofelCommands()

        // Add your command groups, see Remora.Commands documentation for more
        .AddCommandTree()
            .WithCommandGroup<MyCommandGroup>()
        .Finish();
}
```

### Creating Command Groups

Command groups hold commands. A command group has to be registered to plugin's
service collection.

```csharp
[RequirePermission("myplugin.mycommandgroup")]
public class MyCommandGroup : CommandGroup
{
    private readonly FeedbackService _feedback;
    private readonly ValidationFeedbackService _validationFeedback;

    public MyCommandGroup(
        FeedbackService feedback,
        ValidationFeedbackService validationFeedback)
    {
        _feedback = feedback;
        _validationFeedback = validationFeedback;
    }

    [Command("hello")]
    [Description("Say hello to a user")]
    [RequirePermission("myplugin.mycommandgroup.hello")]
    public async Task<Result<IReadOnlyList<IMessage>>> HandleHelloAsync(
        [Description("User to greet")] [DiscordTypeHint(TypeHint.User)] Snowflake user,
        [Description("Number of times")] int times = 1)
    {
        // Validation
        var validator = new CommandValidator()
            .MakeSure("times", times, t => t.InclusiveBetween(1, 10));

        if (!validator.IsValid)
            return await _validationFeedback.SendValidationErrorAsync(validator.Errors);

        // Command logic
        var message = $"Hello {user.Mention}! " + string.Join(" ", Enumerable.Repeat("👋", times));
        return await _feedback.SendContextualSuccessAsync(message);
    }
}
```

## Integration Points

### With Christofel.Common
- Uses `IPermissionsResolver` for database-backed permission checking
- Integrates with `IChristofelState` for application context
    - Commands are added to the guild specified in application's state `BotOptions`

### With Plugin System
- Commands are automatically registered/unregistered with plugin lifecycle
- Supports hot-swapping - commands update when plugins reload
- Respects application shutdown - commands aren't deleted when bot stops
  - Prevents hitting rate limits

## Best Practices

### Command Design
- Use descriptive command and parameter names
- Always include `[Description]` attributes for better UX
- Group related commands in command groups
- Use consistent permission naming (e.g., `"<pluginname>.<groupname>.<commandname>"`)

### Validation
- Validate all user inputs using `CommandValidator`
- Provide meaningful error messages with `.WithMessage()`
- Use appropriate validation ranges for numeric inputs
- Check for null/empty values on optional parameters

### Error Handling
- Let the execution events handle logging and user feedback
- Return `Result` types for consistent error handling
- Return validation error from the christofel validator itself so that a message is made for the user with validation errors
- Use `FeedbackService` for informing users about errors, but don't give too many information about the internal state. All errors are logged, so provide enough information in the returned error itself. If the user is administrator, they can check the log. So even for commands for administrators, do not include any stack traces and such
- Use `FeedbackService` for success messages

### Permissions
- Follow the principle of least privilege
- Use hierarchical permission naming (`management.users.ban`, `management.users.kick`)
