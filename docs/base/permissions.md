# Permissions

Permissions are stored in `ChristofelBaseContext`. Each permission
has its name and target (could be specific user, role or everyone).

Permissions should follow dot notation and be grouped. By grouping,
more permisisons can be assigned in one entry by using wildcards.

Suppose we have a permission `management.messages.slowmode`, then any of
`*`, `management.*`, `management.messages.*`, `management.messages.slowmode`
would grant this permission to the target.

Permissions can only be granted, they can't be revoked, so using more specific permissions
is preferred to having general permissions, because you cannot revoke a more specific permission
when it is already granted, even with wildcards.

## Slash command permissions

When using slash commands, or interactions in general,
an attribute `RequirePermission` can be used for requiring a given permission.
This attribute should be used on the group itself and on every command/subcommand.
Additionally to make Discord automatically give people access to the command,
`DiscordDefaultMemberPermissions` attribute can be used. This should then be used
based on the predicted permission that will have access to the commands.
Still, when the user doesn't have Christofel permissions, they won't be able
to use the command.

The permissions generally follow `<plugin>.<group>.<command>` schema.

## Working with permissions
For permissions `IPermissionService` and `IPermissionResolver` are exposed
in the shared state (`IChristofelState`). The purpose of permission service
is to hold state of permission so they can be listed by administrator.
Each permission has its name, display name and description.
NOTE that this is not really used throught the plugins, at least not yet.

Permission resolver is used for checking whether a target has permissions
or for getting all targets for specified permission.

## (Incomplete) list of permissions
- `application` - Permissions for base application
  - `quit` - Permission for `/quit` command
  - `refresh` - Permission for `/refresh` command
  - `plugins`
    - `control` - Permission for `/plugin` command allowing attaching, detaching and listing plugins
- `helloworld` - Permissions for Helloworld plugin
  - `ping` - Permission for `/ping` command
