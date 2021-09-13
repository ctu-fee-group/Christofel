//
//   CtuAuthWarnMessage.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Rest.Core;

namespace Christofel.Api.Ctu.Jobs
{
    /// <summary>
    /// The job for <see cref="Christofel.Api.Ctu.JobQueue.CtuAuthNicknameSetProcessor"/>.
    /// </summary>
    /// <param name="UserId">The user to send the message to.</param>
    /// <param name="Message">The message to send.</param>
    public record CtuAuthWarnMessage(Snowflake UserId, string Message);
}