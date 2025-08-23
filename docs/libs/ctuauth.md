# Christofel.CtuAuth Library

## Overview

The CtuAuth library provides integration between CTU APIs and Discord role management.
It implements a pipeline-based authentication flow that validates users can authenticate, gathers role data from CTU APIs, and applies Discord role assignments.

## Core Architecture

The authentication process operates through a **three-phase pipeline**:

1. **Pre-auth Conditions** - User validation and duplicate checking
2. **Auth Steps** - Role determination from CTU systems (KOS, Usermap)
3. **Auth Tasks** - Discord integration (role assignment, nickname setting)

The system follows a **fail-fast approach** for data integrity - conditions and steps abort immediately on errors, while tasks are more resilient and continue processing even if some operations fail.
This is important, because the tasks happen only after the request from user terminates. The user will be notified of success after the steps finish and tasks are scheduled, but
before the tasks finish. It must be ensured that once the user sees successful message, the bot will try to finish the tasks even in cases where the bot exits
before the tasks are finished.

During authentication a registration code is used. This code identifies the record in database. This code is created
after Discord identification is obtained from the user (either by clicking on buttons in welcome message on Discord,
or by authenticating through OAuth)

## Key Components

### `CtuAuthProcess`
The main orchestrator that manages the complete authentication flow. Handles OAuth token integration, database persistence, and coordinates all pipeline phases.

### Extensibility Framework
- **`IPreAuthCondition`** - Validation rules executed before processing
- **`IAuthStep`** - Role determination logic from CTU data sources
- **`IAuthTask`** - Discord operations (role assignment, nickname setting)

### External Dependencies
- **KOS API** - CTU student information system for academic data
- **Usermap API** - CTU user directory for roles and titles
- **OAuth System** - CTU authentication tokens
- **Discord API** - Role and nickname management
- **Entity Framework** - Database persistence for user and role mappings

## Data Flow

```
CTU OAuth Token → User Validation → KOS/Usermap API Calls → Role Calculation → Discord Role Assignment
```

The library provides both default implementations and extension points for customizing authentication logic per deployment requirements.

## Default Authentication Components

### Built-in Conditions
- Username validation and matching
- Duplicate account detection
- Member verification

### Built-in Steps
- Academic year-based roles
- Programme-specific roles
- Academic title roles
- Usermap role mapping
- Username-based assignments

### Built-in Tasks
- Discord role assignment
- Nickname management
- Authentication response handling
- Background job processing

## Error Handling Strategy

The system implements **defensive authentication** with graceful degradation:

- **Hard Failures** (Conditions/Steps): Complete process abort, user is notified of the error and must retry
- **Soft Failures** (Tasks): User authenticated, Discord integration may be incomplete
- **API Resilience**: External API failures result in reduced functionality rather than complete failure
- **Mandatory Components**: Critical elements like "Authentication" role cause hard failures if missing

## Usage

The library integrates into applications through dependency injection:

```csharp
services.AddCtuAuthProcess();           // Core authentication engine
services.AddDefaultCtuAuthProcess();    // Default conditions, steps, and tasks
```

Custom authentication logic can be added by implementing the extensibility interfaces and registering them with the service collection.

## Detailed Error Handling

### Authentication Pipeline Error Behavior

The CtuAuth process implements **fail-fast** behavior for conditions and steps, but **resilient** behavior for tasks.

#### Pre-auth Condition Failures

**Behavior**: Immediate termination on first failure
- No database changes are persisted
- Error is returned directly to caller
- User must address underlying issue before retrying

**Common Failures**:
- `DuplicateError` - Account conflicts (requires manual approval or duplicate resolution)
- `InvalidOperationError` - Username mismatch between OAuth and database
- Validation failures - Missing or invalid user data

**Recovery**: User must resolve the condition (duplicate approval, data correction) and retry authentication

#### Auth Step Failures

**Behavior**: Immediate termination on first failure
- Partial database state - CTU username may be saved but role data is not
- Error is returned directly to caller
- All subsequent steps are skipped

**Common Failures**:
- Missing mandatory "Authentication" role mapping → `InvalidOperationError`
- KOS/Usermap API connectivity issues
- Database constraint violations
- External service timeouts

**Recovery**: Address API connectivity or configuration issues, then retry authentication

#### Auth Task Failures

**Behavior**: Continues processing all tasks, collects errors
- All tasks are attempted even if some fail
- Returns `SoftAuthError` wrapper instead of hard failure
    - Each error is logged individually
    - User is considered authenticated despite Discord integration issues

**Common Failures**:
- Discord API rate limiting or permission errors
- Role assignment failures (missing roles, hierarchy conflicts)
- Nickname setting failures
- Job queue system issues

**Recovery**: User can retry authentication to complete Discord integration, or admin intervention for persistent Discord issues

### Error Recovery Strategies

#### API Level Resilience
- **Graceful Degradation**: External API failures reduce functionality rather than cause complete failure
- **Fallback Chains**: KOS API → Usermap API → Skip assignment for non-critical roles
- **Retry Logic**: Background job system handles Discord API rate limiting and temporary failures (role assignment is retried on any errors)

## Authentication Steps and Role Assignment Logic

The authentication process executes steps in dependency injection registration order. Each step contributes roles that are aggregated and applied during the task phase.

The object passed through the steps is `CtuAuthProcessData`. It contains identification of the user,
database, Discord object for the guild member. It also holds information about the roles to assign to the user,
as `CtuAuthAssignedRoles` class. This class has a concept of 'soft' removal of roles. When a role is soft removed,
it is meant to be removed from a user, unless explicitly added using `AddRole`.

After conditions pass, the `CtuUsername` is saved in database,
before the steps are called.
This is to ensure that once a registration code is used by given
person, it cannot be transferred to another.
Conditions do not modify the database.

### Pre-auth Conditions

Executed before any role processing begins. **Any failure aborts the entire process.**
The conditions do not just check conditions, they can also modify the internal state
used during authentication. For example the `NoDuplicateCondition` adds
the linked accounts so that they can be then handled by steps or tasks.

#### `CtuUsernameFilledCondition`
- **Purpose**: Ensures CTU username is available from OAuth
- **Failure**: Missing or empty CTU username → Process aborted

#### `MemberMatchesUserCondition`
- **Purpose**: Validates Discord member matches saved database user discord id
- **Failure**: Member/user mismatch → Process aborted

This is just a sanity check. The program should make sure it fetches the correct
discord user based on what is in the database.

#### `NoDuplicateCondition`
- **Purpose**: Prevents account conflicts and manages approved duplicates
- **Logic**:
  - CTU-side duplicates: Allowed with partial de-authentication of old account
  - Discord-side duplicates: Blocked with `DuplicateError`
  - Manual override: `DuplicityApproved` flag bypasses check
- **Failure**: Unapproved duplicate account → Process aborted

Duplicate means there is the same user already authenticated based on a given
identifier.

There are three types of duplicates based on what identifier matches:
- Discord-side - Discord id matches (two different ctu identities are being linked to same discord user)
- CTU-side - CTU username matches (two different Discord users are being linked to one ctu identity)
- Both - both CTU username and Discord id matches

Due to large number of users changing their Discord account, CTU-side duplicate is implicitly allowed,
while deauthenticating the old account (the account is only partially deauthenticated as channel overrides
or roles other than the authentication roles are not removed from them). This wasn't the case before.
Users had to get the duplicity approved, after administrators manually deauthenticated the old account.

Discord-side duplicate is never allowed, as CTU username never changes for one person. Or at least
we do not know of such a case.

Both duplicate is special, this means the user is reauthenticating. Because of that, this new record will
be removed afterwards in the steps, and the original duplicate record will get updated its `AuthenticatedAt`
field.

#### `CtuUsernameMatchesCondition`
- **Purpose**: Ensures that once registration code is used by one ctu identity, it cannot be transferred. Checks OAuth username matches stored database username
- **Failure**: Username mismatch → Process aborted

This mitigates an attack where one user would get a duplicate allowed and then give their link to another
user who would authenticate themselves. This isn't so much relevant now that duplicity is allowed implicitly.

### Authentication Steps

Executed sequentially to gather and assign roles or any database
changes. **Any step failure aborts the entire process.**
Although the steps are executed sequentially, the steps shouldn't
rely on the order of execution. The steps can rely on changes
made by conditions, but they shouldn't assume order of steps being ran.

Most steps have their own database entity that contains mappings
to Discord roles. There is `RoleAssignment` entity that links to Discord
roles. Then the entities, such as `YearRoleAssignment` refer to `RoleAssignment`.
Each entity contains information necessary for given step, ie. Year role assignment
has year - role mapping.

#### `SetUserDataStep`
- **Role Assignment**: None
- **Function**: Updates authentication timestamp, clears registration code, sets CTU username
- **Always Executes**: Core data management, no external dependencies

#### `SpecificRolesStep`
- **Role Assignment**:
  - **"Authentication"** (mandatory for all authenticated users)
  - **"Teacher"** (if user has teacher role in KOS)
  - **Programme type roles**: "BachelorProgramme", "MasterProgramme", "DoctoralProgramme"
- **Data Source**: KOS API for teacher status and active student programmes
- **Critical Failure**: Missing "Authentication" role mapping → `InvalidOperationError` (hard abort)
- **Graceful Handling**: Missing teacher or programme roles logged as warnings

'Specific' stands for there being a specific process behind searching the information that then
matches to a specific key, such as "Authentication", "Teacher"...
Those are the cases where it didn't make sense to create an extra table.

#### `ProgrammeRoleStep`
- **Role Assignment**: Programme-specific roles based on study programme names
- **Logic**:
  - Active students → Standard programme roles
  - Graduated students → Graduation-specific roles
  - Fallback: Uses most recent study programme if no active/graduated roles found
- **Data Source**: KOS API for programme information. Programmes are matched by name of the programme due to frequent changes in codes of programmes.
- **Graceful Handling**: Missing programme mappings logged as warnings, process continues

#### `YearRoleStep`
- **Role Assignment**: Roles based on study start year(s)
- **Logic**:
  - Uses earliest FEE student role to determine programme type
  - Assigns year roles for all studies of the same programme type
  - Filters to CTU FEE faculty only
- **Data Source**: KOS API for student roles and start dates
- **Graceful Handling**: Missing year mappings logged as warnings, process continues

#### `TitlesRoleStep`
- **Role Assignment**: Academic title-based roles (pre/post name titles)
- **Data Sources**:
  - **Primary**: KOS API (separate pre/post title fields)
  - **Fallback**: Usermap API (requires parsing from full name)
- **Logic**: Matches individual titles against database role assignments
- **Graceful Handling**: API failures result in no title roles assigned

#### `UsermapRolesStep`
- **Role Assignment**: Roles based on Usermap role strings
- **Logic**:
  - Direct string matching for exact role names
  - Regex pattern matching for flexible role matching
- **Data Source**: Usermap API for user roles
- **Graceful Handling**: API failure results in no Usermap roles assigned

In Christofel, this is used for programme type assignments (fallback for `SpecificRoleAssignment`),
"CTU impostor" role assignment (when there is no role on FEE)
and "FEE student" role assignment (when the user has OSOBA - person role)

#### `UsernameRolesStep`
- **Role Assignment**: Direct CTU username-to-role mappings
- **Use Case**: Administrative role assignments for specific users, such as PR or academic senate
- **Data Source**: Database `UsernameRoleAssignment` table
- **Always Succeeds**: Database-only operation, no external dependencies

This is mostly for cases where there would need to be a lot of requests made. For example,
in case of PR it would be necessary to do a request to fee website and parse the html,
for every authentication request. Instead of that, the request is done once per week
through cron in `Christofel.Management`. - See the core concepts [index](../base/index.md)

#### `DuplicateAssignStep`
- **Role Assignment**: Handles role transitions for approved duplicate accounts
- **Logic**: Manages roles when accounts are merged or duplicates are resolved

For both duplicate, removes new user record from database and sets `AuthenticatedAt` of the
original account to now.

#### `RemoveOldRolesStep`
- **Role Assignment**: Marks existing Discord roles for soft removal
- **Purpose**: Cleans up roles that should no longer be assigned
- **Logic**: Identifies all assignable roles currently on user, marks for soft removal
- **Always Succeeds**: Prepares role cleanup for task phase

All authentication roles are removed from user unless explicitly added
by another step. => There is no 'memory' of old authentication roles.

### Authentication Tasks

Executed after all steps complete. **Task failures are logged but do not abort the process.**

#### `AssignRolesAuthTask`
- **Function**: Applies calculated role changes to Discord
- **Logic**:
  - Calculates roles to add (new assignments not currently assigned)
  - Calculates roles to remove (marked for removal, excluding new assignments)
  - Skips operation if no changes needed
- **Background Processing**: Uses job queue due to rate limiting, jobs execute sequentially
- **Caching**: Saves role assignments to database for recovery, before enqueueing them
- **Failure Handling**: Errors logged, user can retry authentication

#### `SetNicknameAuthTask`
- **Function**: Updates Discord nickname based on CTU data for first time authentication
- **Data Source**: Real name from KOS/Usermap APIs
- **Failure Handling**: Nickname failures don't affect authentication status

#### `SendNoRolesMessageAuthTask`
- **Function**: Notifies user if no roles were assigned
- **Use Case**: Users who authenticate successfully but don't qualify for any roles - mostly new students
- **Failure Handling**: Message delivery failures logged but ignored

Every user that can authenticate through OAuth gets authenticated role, but having just that role
is suspicious. It usually means the user has been added to the system just today or a few days ago.
The message should tell the user they should authenticate in the future again, to obtain all roles
for access to channels.

#### `RemoveLinkedRolesAuthTask`
- **Function**: Handles role removal for linked/duplicate accounts
- **Use Case**: Clean up when duplicate accounts are merged

This is the step that partially deauthenticates users. All unapproved duplicates are deauthenticated.
Due to the limitations given by the prior conditions and steps, this means the CTU-side duplicates for
cases where duplicate hasn't been explicitly approved.

#### `EditInteractionResponseTask`
- **Function**: Updates Discord interaction with authentication results
- **Use Case**: Provides user feedback on authentication status
- **Failure Handling**: Response update failures don't affect authentication

The `Christofel.Welcome` plugin has a button for authentication. The button will generate
a new message with the link to authenticate. After the authentication, this changes the
message to say authentication has been successful, and removes the link that is now unusable
(due to the `SetUserDataStep` that removes the registration code)

### Role Assignment Priority and Conflicts

- **Additive System**: Multiple steps can assign the same role - no conflicts
- **Soft Removal**: Roles are marked for removal but preserved if reassigned by later steps
- **Final Calculation**: Task phase computes net role changes (adds - removes) before Discord API calls
- **Mandatory Roles**: "Authentication" role is required; missing mapping causes hard failure
- **Optional Roles**: All other role assignments are optional; missing mappings logged as warnings
