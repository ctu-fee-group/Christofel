# Permissions

Permissions are stored in base database. Each permission
has its name and target (could be specific user, role or everyone).

Permissions should follow dot notation and be grouped. By grouping,
more permisisons can be assigned in one entry by using wildcards.

Suppose we have a permission `management.messages.slowmode`, then any of
`*`, `management.*`, `management.messages.*`, `management.messages.slowmode`
would grant this permission to the target.

## Working with permissions
For permissions `IPermissionService` and `IPermissionResolver` are exposed
in the shared state (`IChristofelState`). The purpose of permission service
is to hold state of permission so they can be listed by administrator.
Each permission has its name, display name and description.

Permission resolver is used for checking whether target has permissions
or for getting all targets for specified permission.

## List of permissions
- `helloworld` - Permissions for Helloworld plugin
  - `ping` - Permission for `/ping` command
- `application` - Permissions for base application
  - `quit` - Permission for `/quit` command
  - `refresh` - Permission for `/refresh` command
  - `plugins`
    - `control` - Permission for `/plugin` command allowing attaching, detaching and listing plugins
