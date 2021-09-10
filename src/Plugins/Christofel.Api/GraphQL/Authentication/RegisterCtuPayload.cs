//
//   RegisterCtuPayload.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Christofel.Api.GraphQL.Common;
using Christofel.BaseLib.Database.Models;

namespace Christofel.Api.GraphQL.Authentication
{
    public class RegisterCtuPayload : Payload
    {
        public RegisterCtuPayload(DbUser user)
            : base(new List<UserError>())
        {
            User = user;
        }

        public RegisterCtuPayload(UserError error)
            : base(new List<UserError>(new[] { error }))
        {
        }

        /// <summary>
        /// User filled with information that were obtained so far
        /// </summary>
        public DbUser? User { get; }
    }
}