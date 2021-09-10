//
//   Payload.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Christofel.Api.GraphQL.Common
{
    /// <summary>
    /// Base payload of mutations
    /// </summary>
    public abstract class Payload
    {
        public Payload(ICollection<UserError> errors)
        {
            Errors = errors;
        }

        /// <summary>
        /// Validation errors in case that there are any
        /// </summary>
        public ICollection<UserError> Errors { get; }
    }
}