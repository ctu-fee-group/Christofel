# Christofel

This project is written in C# using .NET 5.

Only most important principles can be found in this readme.
For the whole documentation `goto` https://christofel.readthedocs.io/en/latest/

Christofel is a Discord bot using Remora.Discord.

## Christofel the Bot

Christofel is a bot that is used on CTU FEE (Czech technical university, Faculty of electrical engineering) Discord server.

It is used for basic moderation and for authenticating the users using oauth2.

Christofel can attach (and detach) plugins during runtime at any time its needed.
All of the plugins that are used on our server are in this repository.

## Christofel the Library

Christofel uses Remora.Discord along with some Microsoft extensions (namely Configuration, DependencyInjection, Logging).

The main feature of library (and base application) is to be able to attach plugins during runtime.
The library can be found in `Christofel.BaseLib` project. This should be a library that every
plugin will reference. To attach a plugin, it must have a class implementing IPlugin. For more
information about how to setup a plugin, documentation can be used.

BaseLib exposes interfaces that hold the shared state of the application. That means the DiscordSocketClient
for interacting with Discord, LoggerFactory, database context, permissions and more.

`Christofel.Application` is a project containing startup of Christofel, manages its lifetime and
supports attaching plugins using a command and/or loads them at startup depending on the configuration.
The application contains implementations of some of the interfaces that are in base library. These implementations
are hidden from other plugins, they should not reference the application.

The most important thing about attaching plugins is that only those libraries that are needed to be shared
are shared and the rest is loaded using AssemblyLoadContext. That means that every plugin can have different
version of the same library that is used in another plugin. Shared libraries are only those that BaseLib is dependent
on.
> This fact is being accounted for in BaseLib so that it contains as minimal implementation as needed. The rest of the implementation
> can be found in `Christofel.BaseLib.Implementations` and each plugin can have its version loaded.
> Basic helper classes for slash commands can be found in Christofel.CommandsLib that uses Remora.Discord.Commands

Each plugin has its lifetime that exposes some CancellationTokens for registering callbacks
and a method to stop and destroy the plugin (or application).

Christofel library could be used without our plugins for CTU FEE server so it may become
usable on different servers.
