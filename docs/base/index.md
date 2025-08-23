# Base library, application

The base library has features used in the whole application.

## Feature list
  - [base database models](https://dbdocs.io/fandabohacek/christofel)
  - [lifetime](lifetime)
  - [plugins specifications](plugins)
  - [permissions](permissions)
  - [configuration](configuration)
  - application shared state

## Shared state
Shared state is exposed using `IChristofelState`.
Each plugin will obtain instance of state on initialization.

Shared state contains all the features that are listed above.

## Core concepts

### Avoid excessive network requests

Within Christofel, we're trying to make the least amount of calls to Discord api, or any api for that matter,
as possible.

Practical example is that instead of accepting `IUser` in commands, `Snowflake` is used, if obtaining a full
user is not necessary. (though with resolved entities in slash commands this wouldn't be so expensive)
Also, course assignments are kept in the database rather than querying if the user has access to given channel.
This means that even if there is possibility for being out of sync, doing less requests is preferred.

With kos api and usermap api, caching is used. For every individual request to `Christofel.Api`, all the entities are cached.
Pratically, only request for the person, student and its programme are made on each registration, once. This is even though
there are more calls requesting data from the api. All those are for the same entities, so only one backing call to the api is made.

### Least identification data stored in database

The database shouldn't store data that is not necessary. For example, the programme and year of user are kept in Discord roles and the
bot can update them on next authentication of the users, there is no need to also store them in the bot's database.
This is important for cases where there would be a leak of the database, the data could give a lot of information to attackers.
Currently the only link between CTU and Discord stored in the database is the ctu username. This username is used for actions such
as listing courses for current semester. Additionally it's also a measure against harmful behavior of users, where the administrators
could identify a person if deemed absolutely necessary.
