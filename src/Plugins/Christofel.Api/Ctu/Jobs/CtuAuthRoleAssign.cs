//
//   CtuAuthRoleAssign.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Remora.Rest.Core;

namespace Christofel.Api.Ctu.Jobs
{
    /// <summary>
    /// The data for <see cref="CtuAuthAssignRoleJob"/>.
    /// </summary>
    /// <param name="UserId">The id of the user to assign roles to.</param>
    /// <param name="GuildId">The guild id of the user.</param>
    /// <param name="AddRoles">The roles to be added.</param>
    /// <param name="RemoveRoles">The roles to be removed.</param>
    public record CtuAuthRoleAssign
    (
        Snowflake UserId,
        Snowflake GuildId,
        IReadOnlyList<Snowflake> AddRoles,
        IReadOnlyList<Snowflake> RemoveRoles
    );
}