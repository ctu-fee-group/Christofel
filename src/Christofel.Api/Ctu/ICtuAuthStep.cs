using System;
using System.Threading.Tasks;

namespace Christofel.Api.Ctu
{
    /// <summary>
    /// Individual step of the auth process used to
    /// validate, assign roles etc.
    /// </summary>
    public interface ICtuAuthStep
    {
        /// <summary>
        /// Handle the step, call next if the process
        /// should continue
        /// </summary>
        /// <param name="data"></param>
        /// <param name="next">Callback to start next step</param>
        /// <returns></returns>
        public Task Handle(CtuAuthProcessData data, Func<CtuAuthProcessData, Task> next);
    }
}