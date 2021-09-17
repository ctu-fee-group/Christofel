//
//   DbUserType.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.Database.Models;
using HotChocolate.Types;

namespace Christofel.Api.GraphQL.Types
{
    /// <summary>
    /// GraphQL type configuration representing <see cref="DbUser"/>.
    /// </summary>
    public class DbUserType : ObjectType<DbUser>
    {
        /// <inheritdoc />
        protected override void Configure(IObjectTypeDescriptor<DbUser> descriptor)
        {
            descriptor.Name("User");

            descriptor
                .ImplementsNode()
                .IdField(x => x.UserId)
                .ResolveNode
                (
                    (ctx, id) =>
                        null!
                );

            descriptor.Field(x => x.DiscordId)
                .Type<SnowflakeType>();

            descriptor
                .Ignore(x => x.DuplicitUsersBack)
                .Ignore(x => x.DuplicitUserId)
                .Ignore(x => x.DuplicitUser);
        }
    }
}