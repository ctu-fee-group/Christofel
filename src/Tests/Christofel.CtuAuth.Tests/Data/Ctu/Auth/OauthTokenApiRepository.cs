//
//   OauthTokenApiRepository.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.Database.Models;
using Christofel.OAuth;
using Moq;

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth
{
    /// <summary>
    /// Repository for creating <see cref="ICtuTokenApi"/>.
    /// </summary>
    public class OauthTokenApiRepository
    {
        /// <summary>
        /// Creates mocked <see cref="ICtuTokenApi"/> that will return the given <paramref name="user"/> with <paramref name="username"/>.
        /// </summary>
        /// <param name="user">The user that should be returned.</param>
        /// <param name="username">The username that should be returned.</param>
        /// <returns>A mock of the <see cref="ICtuTokenApi"/>.</returns>
        public static Mock<ICtuTokenApi> GetMockedTokenApi(DbUser user, string username)
        {
            var successfulOauthHandler = new Mock<ICtuTokenApi>();
            successfulOauthHandler
                .Setup(tokenApi => tokenApi.GetUser(It.IsAny<string>()))
                .Returns(new CtuUser(user.UserId, username));

            return successfulOauthHandler;
        }
    }
}
