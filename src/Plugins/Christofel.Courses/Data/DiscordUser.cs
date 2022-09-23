//
//   DiscordUser.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.User;
using Remora.Rest.Core;

namespace Christofel.Courses.Data;

/// <inheritdoc />
public record DiscordUser(Snowflake DiscordId)
    : IDiscordUser;