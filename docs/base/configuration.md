(base_configuration=)
## Configuration of Christofel
Configuration is just basic key value mapping.
It is done using json and database at the same time.
First value is looked up in json (higher priority) and then database is checked
if the value is not found.

Generally everything should be stored in database except for fields
that are needed to start the bot. Bot token has to be in json file for example.

### Json
Json configuration is stored in file `config.json` in the same path where main executable is.

### Database
Database configuration is just basic table having name and value string fields.
It is located in base database and default table name is `Configuration`.

### Usage in code
Interfaces `IReadableConfig`, `IWritableConfig` can be used to distinguish
between configs that do only read or write, `IWRConfig` is made for saying it
supports both.

Configs have Converters that convert string to specified type.
Converters can be registered using `RegisterConverter` method.

Other methods should be self-explanatory.

### Documented Fields
Access to nested levels is achieved using dots in between level names.

- `modules`
  - `path` to the folder containing modules
- `discord` contains everything associated with discord itself
  - `bot`
    - `token` of the bot
  - `guild` id of the guild the bot is used in
  - `log` channels
    - `warning` channel id
    - `error` channel id
    - `info` channel id
