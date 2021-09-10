//
//   ILinkUser.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Discord.Core;

namespace Christofel.BaseLib.User
{
    /// <summary>
    /// User with both discord id and ctu username.
    /// </summary>
    public interface ILinkUser : ICtuUser, IDiscordUser
    {
    }
}