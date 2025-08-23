# Database

Christofel uses Entity Framework Core with MariaDB for data persistence.

## Architecture

### Schema Organization
The database uses logical schema separation through table prefixes (since MariaDB doesn't support true schemas).
Each schema corresponds to different parts of the application:

- **Base Schema**: Shared entities used across plugins
- **Plugin Schemas**: Plugin-specific data in separate contexts

### ChristofelBaseContext
The shared database context contains core entities including user authentication data, permission assignments,
and various role assignment configurations used by the CTU authentication system.
This context is shared between all plugins that need access to user and permission data.

## Context Design Patterns

### Plugin-Specific Contexts
Plugins should create their own database contexts for plugin-specific data,
This ensures data isolation and prevents conflicts between plugins.

### Cross-Context References (untested, TODO)
To reference entities from other contexts, inherit from the target context to access its entity definitions. This allows creating foreign key relationships to shared entities like users and permissions.

## Database Setup

Configure the connection string in `config.json` under `ConnectionStrings.ChristofelBase`.
Each context manages its own migrations. Those migrations aren't applied automatically.

## Usage

To use the base context in plugins, use `.AddChristofelDatabase(State)` in service configuration.
For plugin-specific contexts, use `AddChristofelDbContextFactory` and `AddReadOnlyDbContext` for query-only operations to improve performance.

## Best Practices

- Use separate contexts for unrelated data (per plugin)
- Use read-only contexts for reading
- Follow the principle of minimal data storage - avoid storing sensitive or unnecessary information
