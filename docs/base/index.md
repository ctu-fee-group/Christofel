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
