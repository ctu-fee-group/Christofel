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

## Discord Integration

Christofel implements a dual permission system that works alongside Discord's native permissions:

### Two-Layer Permission System

1. **Discord Native Permissions** - Built-in Discord role/channel permissions
2. **Christofel Permissions** - Database-backed granular permission system

### How They Work Together

**Command Visibility**: Discord's `DiscordDefaultMemberPermissions` attribute controls who can *see* slash commands in Discord's interface.

**Command Execution**: Christofel's `RequirePermission` attribute controls who can *actually execute* commands.

```csharp
[DiscordDefaultMemberPermissions(DiscordPermission.ManageMessages)]  // Discord: Who sees the command
[RequirePermission("management.messages.slowmode")]                  // Christofel: Who can use it
public async Task<Result> SlowmodeCommand() { ... }
```

### Permission Flow
1. User types slash command
2. Discord checks if user has Discord permissions → Shows/hides command
3. User executes command
4. Christofel checks database permissions → Allows/denies execution

**Important**: Even if a user has Discord permissions to see a command, they still need Christofel permissions to execute it. Users without Christofel permissions will receive no response (to avoid revealing valid commands).

## Slash Command Permissions

Use `RequirePermission` attribute on command groups and individual commands:
- Apply to command groups for inherited permissions
- Apply to individual commands for specific permissions
- All parent permissions must be granted (hierarchical checking)

The permissions generally follow `<plugin>.<group>.<command>` schema.

## Working with permissions
For permissions `IPermissionService` and `IPermissionResolver` are exposed
in the shared state (`IChristofelState`). The purpose of permission service
is to hold state of permission so they can be listed by administrator.
Each permission has its name, display name and description.
NOTE that this is not really used throughout the plugins, at least not yet.

Permission resolver is used for checking whether a target has permissions
or for getting all targets for specified permission.

## Permission Hierarchy Best Practices

### Naming Convention
Follow the standard pattern: `<plugin>.<group>.<command>`

### Design Principles

**Least Privilege**: Start with specific permissions, use wildcards carefully
- Prefer `management.users.ban` over `management.*`
- Wildcards grant all current and future permissions in that namespace

**Logical Grouping**: Group related functionality
- `management.users.*` for user management
- `management.messages.*` for message management

**Scalable Structure**:
```
plugin.feature.action     # Specific action
plugin.feature.*          # All actions in feature
plugin.*                  # Everything (use carefully)
```

### Permission Assignment
- **Discord Roles**: Assign broader permissions to trusted roles
- **Individual Users**: Use for exceptions and temporary access
- **Remember**: Permissions can only be granted, not revoked

## List of Permissions
- `application` - Base application controls
  - `quit` - Shutdown bot
  - `refresh` - Reload configuration
  - `plugins.control` - Manage plugin loading/unloading
- `helloworld` - Example plugin
  - `ping` - Basic ping command
