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

## Concepts

Within Christofel, we're trying to make the least amount of calls to Discord api, or any api for that matter,
as possible.

Practical example is that instead of accepting `IUser` in commands, `Snowflake` is used, if obtaining a full
user is not necessary. (though with resolved entities in slash commands this wouldn't be so expensive)
Also, course assignments are kept in the database rather than querying if the user has access to given channel.
This means that even if there is possibility for being out of sync, doing less requests is preferred.

With kos api and usermap api, caching is used. For every individual request to `Christofel.Api`, all the entities are cached.
Pratically, only request for the person, student and its programme are made on each registration, once. This is even though
there are more calls requesting data from the api. All those are for the same entities, so only one backing call to the api is made.
