//
//   SoftAuthError.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace Christofel.CtuAuth.Auth
{
    /// <summary>
    /// Acts as an error from auth that shouldn't be treated as hard fail error.
    /// </summary>
    /// <remarks>
    /// This error may occur in task stage of the authentication.
    /// There is no recovery from this stage and the user has to
    /// go through the whole authentication again to try to go around
    /// this error.
    /// </remarks>
    public record SoftAuthError()
        : ResultError("An error has occurred during authentication, but it should be ignored and carried on. The state has already been updated, tasks scheduled. Impossible  to abort!");
}
