using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.User;

namespace Christofel.Api.OAuth
{
    public interface ICtuTokenApi
    {
        public Task<ICtuUser> CheckTokenAsync(string accessToken, CancellationToken token = default);
    }
}