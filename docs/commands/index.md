# Commands

Command handling utils can be found in `Christofel.CommandsLib`.
Now only commands can be handled using this library, but in
the future it's possible that this library will be generalized to be used
with all interactions.

## Key parts of the library
- InteractionHandler
- CommandGroups
- CommandRegistrator
- CommandHolder
- CommandHandlerCreators
- CommandExecutors
- validation

Each key part is discussed briefly in the following text.

## InteractionHandler
InteractionHandler is a handler of interactions.
Currently it supports commands only.

It should receive `CommandHolder` with all the commands
that it should handle. `InteractionReceived` is registered
during start phase. If the interaction is a slash command,
it tries to find it in the `CommandHolder` and execute it
using the `CommandExecutor` saved along with the command.

## Registering CommandGroups using DI
To register interaction handler along with
all the dependencies and command groups, you can
use the extension methods.

```{code-block} csharp
collection
    .AddDefaultInteractionHandler(c => c
        .AddCommandGroup<MyCommandGroup>
        .AddCommandGroup<AnotherCommandGroup>
    )
```

## Command groups
Command groups are used to add commands to command holder.
Every command group should implement `ICommandGroup`.

That exposes only one method,
`public Task SetupCommandsAsync(ICommandHolder holder, CancellationToken token = new CancellationToken());`

This method should build the commands (using `SlashCommandInfoBuilder`) and add them to the `holder`
using `holder.AddCommand`.

It is preferred that command handlers are also located in command group.

For examples, see command groups in the project.

## CommandRegistrator
Command registrator is used to register all the commands.
It gets all the registered command groups using `CommandGroupProvider`
that is given to it using constructor.

On start, it proceeds to register all the commands from the command holder.
On refresh, it refreshes permissions of these commands, if needed.
On stop, it unregisters all the commands from the holder.

## CommandHolder
Command holder is used for holding information about slash commands.
Information held about each command is `SlashCommandInfo` containing
all the information and `ICommandExecutor` containing the executor
to execute the command with. That is used by the `InteractionHandler`.

All the commands should be registered from the command groups.


## CommandHandlerCreators
Handler creator is the only feature in this text
that does not need to be used to create and handle commands.
It is just a helper that executes methods
with given arguments so it is easier to handle commands.

It creates `SlashCommandHandler` that is stored in `SlashCommandInfo.Handler`
and is used to handle the command.

Currently there are two implementations, one for regular commands with arguments
(`PlainCommandHandlerCreator`)
and second one for commands with subcommands or subcommand groups
(`SubCommandHandlerCreator`).

The exposed method from these may be used in many ways. Preferred usage is
with extension methods.

Two most common usages are
```{code-block} csharp
// Names must match names of the arguments of the command!
Task HandleAttach(SocketSlashCommand, long, CancellationToken token);
Task HandleDetach(SocketSlashCommand, long, CancellationToken token);
Task HandlePlain(SocketSlashCommand, string, IChannel?, CancellationToken token);

SlashCommandHandler subCommandHandler = new SubCommandHandlerCreator()
    .CreateHandlerForCommand(
        ("attach", (CommandDelegate<long>)HandleAttach),
        // HandleAttach will be called if the subcommand is matched to attach.
        // CommandDelegate<long> is just a delegate returning Task and having the correct
        // arguments for the command. Any delegate may be used as long as it exposes
        // the right arguments. CommandDelegate is exposed just for common usage of just
        // a few arguments where each method has different arguments so it does not
        // make sense to create custom delegate.
        ("detach", (CommandDelegate<long>)HandleDetach),
    );
    
SlashCommandHandler plainHandler = new PlainCommandHandlerCreator()
    .CreateHandlerForCommand((CommandDelegate<string, IChannel?>)HandlePlain);
    // Just one delegate that is used to handle everything.
```

The key thing is that argument names of the method must match (case insensitive)
to the names of the arguments of the command.

CommandHandlerCreators should be used when creating commands.
There is a method `WithHandler` for `SlashCommandInfoBuilder`
that adds the handler. Handler created using `CreateHandlerForCommand`
may be used in `WithHandler`.


## CommandExecutor
Command executors expose execute method that is supposed
to execute the command. Every command has its `Handler`
in `SlashCommandInfo` that should be used for handling,
but some operations may come before, like checking permissions,
auto deferring the command, spinning the command on new thread.
That is where command executors come in play.

There are some default executors and custom can be added. They should
all be done using decorator design.

`CommandExecutorBuilder` may be used for building executors.
The default one exposes all the default possibilities and then
builds the final `ICommandExecutor`. Custom builders can be made
by inheriting from `CommandExecutorBuilder` and overriding `Build`.

Executors are given to command holder upon adding new commands.

## Command Validation
Validation is done using `CommandVerifier<T>`.

Type `T` is returned type that holds object data such as
`IUserMessage`, `IMessageChannel` etc.


### Validating command
`CommandVerifier<>` exposes by itself only one method
that may be used with validation. That is `FinishVerificationAsync`.

Other methods that may be used for validation are exposed using extension methods
and custom validators can be written.

Each validator may request data input from another validator and/or output data.
Output data will be located in the `Result` of the verification.
That is where generic `T` comes in play. Class of that type
should implement all interfaces that are needed for the validators.
Each validator should have description with what type it needs.

Properties of `CommandVerifier` should not be accessed directly.
These are meant for the validation methods only.

Because explaining everything would be complicated, example with comments is here instead
```{code-block} csharp
class MyData : IHasMessageChannel {
  public IMessageChannel Channel { get; set; }
}

// basic validator extensions are located in Christofel.CommandsLib.Verifier.Verifiers
Verified<MyData> verified = await new CommandVerifier<MyData>(client, command, logger)
    .VerifyMessageChannel(channel) // Verifies that IChannel channel is an IMessageChannel, sets MyData.Channel
    .FinishVerification(); // FinishVerification sends response to the command in case of failure
    
if (verified.Success) {
  // Data validated successfully
  
  MyData data = verified.Result;
  if (data.Channel == null) {
    // This should never happen if the validation was correct and all validation methods were called
    throw new InvalidOperationException("Validation failed");
  }
  
  // Data are validated and we can do anything with them
}
```

### Writing a validator
In order to write custom command validator, you should
put it in a class with extension methods.

Every validator should be async. Fluent API can be achieved
by using `CommandVerifier<>.QueueWork` for adding validators.

Exposed function could look like so:
```{code-block} csharp
public static CommandVerifier<T> VerifyMessageId<T>(CommandVerifier<T> verifier, string messageId, string parameterName) {
    where T : class, IHasMessageId, new() // note the IHasMessageId, that is a requirement for this method so we can set the id
{
    verifier.QueueWork(() => VerifyMessageIdAsync(verifier, messageId, parameterName));
    return verifier;
}
```

And the async function for verification:
```{code-block} csharp
public static Task VerifyMessageIdAsync<T>(CommandVerifier<T> verifier, string messageId, string parameterName) {
    where T : class, IHasMessageId, new() // note the IHasMessageId, that is a requirement for this method so we can set the id
{
    // For some validators (if they depend on another one) it may be needed to check
    // if the validation is successful so far
    if (!verifier.Success)
    {
        return Task.CompletedTask;;
    }


    // do the validation
    if (!ulong.TryParse(messageId, out ulong messageIdUlong))
    {
        // We could not parse it, add it so it is displayed to the user
        verifier.SetFailed(parameterName, "Could not convert message id to ulong");
        return Task.CompletedTask;
    }
    
    // Thanks to the IHasMessageId restriction
    verifier.Result.MessageId = messageId;

    return Task.CompletedTask;
}
```
