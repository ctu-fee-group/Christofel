using Christofel.Api.GraphQL.DataLoaders;
using Christofel.BaseLib.Database.Models;
using HotChocolate.Resolvers;
using HotChocolate.Types;

namespace Christofel.Api.GraphQL.Types
{
    public class DbUserType : ObjectType<DbUser>
    {
        protected override void Configure(IObjectTypeDescriptor<DbUser> descriptor)
        {
            descriptor
                .ImplementsNode()
                .IdField(x => x.UserId)
                .ResolveNode((ctx, id) =>
                    null!);

            descriptor.Field(x => x.DiscordId)
                .Type<UnsignedLongType>();

            descriptor
                .Ignore(x => x.DuplicitUsersBack)
                .Ignore(x => x.DuplicitUser);
        }

        /*private class DbUserResolvers
        {
            
        }*/
    }
}