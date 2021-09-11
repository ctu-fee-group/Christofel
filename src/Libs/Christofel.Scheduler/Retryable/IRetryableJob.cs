//
//  IRetryableJob.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Scheduler.Retryable
{
    /// <summary>
    /// Job that supports repeats.
    /// </summary>
    public interface IRetryableJob
    {
        /// <summary>
        /// Gets the provider for the repeats.
        /// </summary>
        public IRetryProvider ExternalRetryProvider { get; }
    }
}