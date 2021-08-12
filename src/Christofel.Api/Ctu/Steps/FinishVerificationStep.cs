using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Christofel.Api.Ctu.Steps
{
    public class FinishVerificationStep : CtuAuthStep
    {
        public FinishVerificationStep(ILogger<FinishVerificationStep> logger)
            : base(logger)
        {
        }

        protected override Task<bool> HandleStep(CtuAuthProcessData data)
        {
            data.Finished = true;
            data.DbUser.AuthenticatedAt = DateTime.Now;
            data.DbUser.RegistrationCode = null;

            return Task.FromResult(true);
        }
    }
}