using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.Api.Ctu
{
    public class CtuAuthStepProvider
    {
        private List<Type> _steps;

        public CtuAuthStepProvider()
        {
            _steps = new List<Type>();
        }

        public IServiceProvider? Provider { get; set; }

        public void AddStep(Type type)
        {
            if (!type.IsAssignableTo(typeof(ICtuAuthStep)))
            {
                throw new InvalidOperationException("Trying to add non-step type to auth steps");
            }
            
            _steps.Add(type);
        }

        public virtual IEnumerable<ICtuAuthStep> GetSteps()
        {
            if (Provider is null)
            {
                throw new InvalidOperationException("Cannot obtain auth steps if service provider is null");
            }

            foreach (Type stepType in _steps)
            {
                yield return (ICtuAuthStep)Provider.GetRequiredService(stepType);
            }
        }
    }
}