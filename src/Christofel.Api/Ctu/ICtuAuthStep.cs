using System;
using System.Threading.Tasks;

namespace Christofel.Api.Ctu
{
    public interface ICtuAuthStep
    {
        public Task Handle(CtuAuthProcessData data, Func<CtuAuthProcessData, Task> next);
    }
}