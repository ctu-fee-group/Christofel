using System;
using System.Threading.Tasks;

namespace Christofel.CommandsLib.Verifier.Verifiers
{
    public static class BasicVerifierExtensions
    {
        /// <summary>
        /// Verifies whether the specified value is inside the boundaries (including the boundaries)
        /// T must implement IHasMessageChannel
        /// </summary>
        /// <param name="verifier"></param>
        /// <param name="channel"></param>
        /// <param name="parameterName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static CommandVerifier<T> VerifyMinMax<T, TValue>(this CommandVerifier<T> verifier, TValue val, TValue min, TValue max,
            string parameterName)
            where T : new()
            where TValue : IComparable
        {
            verifier.QueueWork(() => verifier.VerifyMinMaxAsync(val, min, max, parameterName));
            return verifier;
        }

        private static Task VerifyMinMaxAsync<T, TValue>(this CommandVerifier<T> verifier, TValue val, TValue min, TValue max,
            string parameterName)
            where T : new()
            where TValue : IComparable
        {
            if (val.CompareTo(min) < 0 || val.CompareTo(max) > 0)
            {
                verifier.SetFailed(parameterName, $@"Value {val} is not in range {min} - {max}.");
            }
            
            return Task.CompletedTask;
        }
    }
}