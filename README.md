# Christofel

This project is written in C# using .NET 5.

Christofel is a modular Discord bot using Remora.Discord.

## Christofel the Bot

Christofel is a bot that is used on CTU FEE (Czech technical university, Faculty of electrical engineering) Discord server.

It is used for basic moderation and for authentication of the users using oauth2.

Christofel can attach (and detach) plugins during runtime at time.
All of the plugins that are used on our server are in this repository.

## Christofel the Library

Christofel uses Remora.Discord along with some Microsoft extensions (namely Configuration, DependencyInjection, Logging).

The main feature of library (and base application) is to be able to attach plugins during runtime.
The library can be found in `Christofel.Plugins`, `Christofel.Plugins.Abstractions` projects. This should be a library that every
plugin will reference. To attach a plugin, it must have a class implementing IPlugin. For more
information about how to setup a plugin, documentation can be used.

Another important library is `Christofel.Common`. It exposes interfaces that hold the shared state of the application.
That means the bot itself for interacting with Discord, LoggerFactory, database context, permissions and more.

Optionally `Christofel.Helpers` may be used for some common operations and `Christofel.CommandsLib` when commands
should be used in the given plugin.

`Christofel.Application` is a project containing startup of Christofel, it manages its lifetime and
supports attaching plugins using a command and/or loads them at startup depending on the configuration.
The application contains implementations of some of the interfaces that are in base library. These implementations
are hidden from other plugins, they should not reference the application.

The most important thing about attaching plugins is that only those libraries that are needed to be shared
are shared and the rest is loaded using a different context. That means that every plugin can have different
version of the same library that is used in another plugin. Shared libraries may be specified, and by default,
only `Christofel.Common` library has to be shared.

Each plugin has its lifetime that exposes some CancellationTokens for registering callbacks
and a method to stop and destroy the plugin (or application). In case of an error, it should
be reported back to the main application. The plugins that are detached will be erased
from the memory eventually, but it may take some time, so reattaching a plugin multiple
times may leave higher memory usage for a while.

Christofel library could be used without our plugins for CTU FEE server so it may become
usable on different servers. At some point Nuget libraries may be made and the projects may be splitted to multiple repositories.
