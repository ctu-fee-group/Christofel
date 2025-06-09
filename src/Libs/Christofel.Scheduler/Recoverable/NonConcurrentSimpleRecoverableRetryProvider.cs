//
//   NonConcurrentSimpleRecoverableRetryProvider.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Scheduler.Abstractions;
using Christofel.Scheduler.Triggers;

namespace Christofel.Scheduler.Recoverable
{
    /*
    /// <summary>
    /// Recoverable retry provider that creates non concurrent simple trigger.
    /// </summary>
    public class NonConcurrentSimpleRecoverableRetryProvider : RecoverableRetryProvider
    {
        private readonly NonConcurrentTrigger.State _ncState;

        /// <summary>
        /// Initializes a new instance of the <see cref="NonConcurrentSimpleRecoverableRetryProvider"/> class.
        /// </summary>
        /// <param name="ncState">The lock state.</param>
        public NonConcurrentSimpleRecoverableRetryProvider(NonConcurrentTrigger.State ncState)
        {
            _ncState = ncState;
        }

        /// <inheritdoc />
        protected override ITrigger CreateRetryTrigger
            (IJobContext context) => new NonConcurrentTrigger(new SimpleTrigger(), _ncState);
    }
    */
}