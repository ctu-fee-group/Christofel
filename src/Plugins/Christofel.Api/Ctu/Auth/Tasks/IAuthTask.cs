using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Tasks
{
    public interface IAuthTask
    {
        public Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default);
    }
}