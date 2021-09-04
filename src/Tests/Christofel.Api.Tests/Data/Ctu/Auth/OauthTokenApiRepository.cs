using System.Threading;
using Christofel.Api.OAuth;
using Christofel.BaseLib.Database.Models;
using Moq;

namespace Christofel.Api.Tests.Data.Ctu.Auth
{
    public class OauthTokenApiRepository
    {
        public static Mock<ICtuTokenApi> GetMockedTokenApi(DbUser user, string username)
        {
            var successfulOauthHandler = new Mock<ICtuTokenApi>();
            successfulOauthHandler
                .Setup(tokenApi => tokenApi.CheckTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()).Result)
                .Returns(new CtuUser(user.UserId, username));

            return successfulOauthHandler;
        }
    }
}