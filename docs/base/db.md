# Database

EF Core is used for connecting to the database.
MariaDB is the chosen SQL server.

## Structure

The database is split into schemas. Because MariaDB doesn't support schemas,
this is implemented by table prefixes with the schema name. The schemas logically
split the database based on the structure of the real application.
For exapmle, if plugins has its own data, it should have them in its own schema.
The `ChristofelBaseContext` is context shared between the plugins. It contains
mainly information about permissions and about authenticated users.
On top of that it also contains the authentication roles and information on how they are supposed to be assigned,
but that is important only for the authentication process.

## Referring to other contexts (untested, TODO)

Since there is just one database, it should be possible to refer
to entities from other contexts. One way to do that is by inheriting
from the other context to get information about all its tables and their structure.
Then foreign keys can be made in regular way.

## Usage

To use the base context, you can use `AddChristofelDatabase(State)` in configuration
of services of a plugin, see [plugins](plugins). To use your own context,
`AddChristofelDbContextFactory` and `AddReadOnlyDbContext` might be used.
