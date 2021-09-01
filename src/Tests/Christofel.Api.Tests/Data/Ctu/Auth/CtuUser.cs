using Christofel.BaseLib.User;

namespace Christofel.Api.Tests.Data.Ctu.Auth
{
    public record CtuUser
    (
        int UserId,
        string CtuUsername
    ) : ICtuUser;
}