using System.Threading;
using Christofel.Management.Database.Models;

namespace Christofel.Management.Slowmode
{
    public record RegisteredTemporalSlowmode(TemporalSlowmode TemporalSlowmodeEntity, CancellationTokenSource CancellationTokenSource);
}