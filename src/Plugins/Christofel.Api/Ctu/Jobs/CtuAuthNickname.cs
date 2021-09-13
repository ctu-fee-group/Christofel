//
//  CtuAuthNickname.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Discord.Core;

namespace Christofel.Api.Ctu.Jobs
{
    /// <summary>
    /// The data for <see cref="CtuAuthNicknameSetJob"/>.
    /// </summary>
    /// <param name="UserId"></param>
    /// <param name="GuildId"></param>
    /// <param name="Nickname"></param>
    public record CtuAuthNickname
    (
        Snowflake UserId,
        Snowflake GuildId,
        string Nickname
    );
}