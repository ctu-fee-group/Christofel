//
//   Payload.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Christofel.Api.GraphQL.Common
{
    /// <summary>
    /// Base payload for mutations.
    /// </summary>
    public abstract class Payload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Payload"/> class.
        /// </summary>
        /// <param name="errors">The collection with user errors.</param>
        public Payload(ICollection<UserError> errors)
        {
            Errors = errors;
        }

        /// <summary>
        /// User errors in case that there were any.
        /// </summary>
        public ICollection<UserError> Errors { get; }
    }
}