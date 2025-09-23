# Gotchas

There are a few gotchas when working with runtime-attachable plugins.

## Hot-swapping libraries

When you want to attach an updated library to running application, you will have to bump version of the library.
Otherwise, the version in memory will be used, and this will cause errors with ABI incompatibilities.
In worst cases this might make the plugin unattachable and thus the application might have to be restarted
to attach a fixed version of the plugin, referring new version of the library.

When updating a library that happens to be used by other libraries, the other libraries have to have
the version bumped as well! This is for example problem when updating the Christofel.Helpers library,
you also need to update CommandsLib, CoursesLib etc. Check in code for up to date list.
