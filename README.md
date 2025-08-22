# Christofel

This project is written in C# using .NET 8.

Christofel is a modular Discord bot using Remora.Discord.
The code can be thought of as being split into multiple parts.
There is the bot and then there is the library that allows usage
of plugins that have their own runtime. Those plugins live inside
the same process as the base application.

## How to run locally

To run Christofel application locally, first database has to be started.
For that there is Nix flake or Docker compose prepared in this repository.
With docker, the programmer still has to migrate the database manually,
while with nix, process-compose is exposed in such a way to run
all migrations upon startup.

There is flake.nix in this repository. It contains
services-flake services. To start these services, use
`nix run .#christofel-services`. This will start the database,
configure initial database, and user. Lastly, it will apply
migrations. Observe output of all of the processes to see
if everything succeeded. The migration process needs to be able to
build the project. The database created is bound to localhost:3306.
The user is christofel, its password is also christofel, and the
database is named christofel as well.

To run the application, it's necessary to create a basic config.json.
An example is in `docker/config.json`. What it has to contain depends on
the plugins you are going to be running. The config is taken out of $(pwd)/config.json.

## Christofel the Bot

Christofel is a bot that is used on CTU FEE (Czech technical university, Faculty of electrical engineering) Discord server.

It is used for basic moderation and for authentication of the users using oauth2.

Christofel can attach (and detach) plugins during runtime.
All of the plugins that are used in CTU FEE guild are in this repository.

## Christofel the Library

Christofel uses Remora.Discord along with some Microsoft extensions (namely Configuration, DependencyInjection, Logging).

The main feature of the library (and base application) is to be able to attach plugins during runtime.
This allows for hot swapping individual parts of the bot without restarting the whole application.
The library can be found in `Christofel.Plugins`, `Christofel.Plugins.Abstractions` projects. This should be a library that every
plugin will reference. To attach a plugin, it must have a class implementing `IPlugin`. For more
information about how to setup a plugin, documentation can be used.

Another important library is `Christofel.Common`. It exposes interfaces that hold the shared state of the application.
That means the bot itself for interacting with Discord, LoggerFactory, database context, permissions and more.

Optionally `Christofel.Helpers` may be used for some common operations (this library can be hot-swapped without restarting,
unlike `Christofel.Common`) and `Christofel.CommandsLib` when commands are to be created in the given plugin.

`Christofel.Application` is a project containing startup of Christofel, it manages its lifetime and
supports attaching plugins using a command and/or loads them at startup depending on the configuration.
The application contains implementations of some of the interfaces that are in `Christofel.Common`. These implementations
are hidden from other plugins, they should not reference the application.

The most important thing about attaching plugins is that only those libraries that are needed to be shared
are shared and the rest is loaded using a different context. That means that every plugin can have different
version of the same library that is used in another plugin. Shared libraries may be specified, and by default,
only `Christofel.Common` and `Christofel.Plugins.Abstractions` libraries have to be shared for ABI compatibility.

Each plugin has its lifetime that exposes some `CancellationToken`s for registering callbacks
and a method to stop and destroy the plugin (or application). In case of an error, it should
be reported back to the main application. The plugins that are detached will be erased
from the memory eventually, but it may take some time, so reattaching a plugin multiple
times may mean higher memory usage for extended amount of time.

Christofel library could be used without our plugins for CTU FEE guile so it may become
usable on different guilds. At some point Nuget libraries may be made and the projects may be splitted to multiple repositories.
