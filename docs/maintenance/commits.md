# Committing policy

Conventional commits MUST be used, see reference https://www.conventionalcommits.org/en/v1.0.0/

Generally scope should be added as well depending on what library or plugin the change was made at.
It should be named like the plugin/lib.

So for example when making change to `Christofel.BaseLib`, `feat(base): description of the change`
should be used.

# PR policy

Every feature should be made in a separate branch and PR for it should be created.
This PR should merge to `dev` branch. After version is released, `dev` will be merged to `master`.
When releasing new verison, tag for it should be created.
