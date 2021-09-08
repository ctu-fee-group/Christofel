using Christofel.BaseLib.Database.Models;
using HotChocolate.Types;

namespace Christofel.Api.GraphQL.Types
{
    public class DbUserType : ObjectType<DbUser>
    {
        protected override void Configure(IObjectTypeDescriptor<DbUser> descriptor)
        {
            descriptor.Name("User");
            
            descriptor
                .ImplementsNode()
                .IdField(x => x.UserId)
                .ResolveNode((ctx, id) =>
                    null!);

            descriptor.Field(x => x.DiscordId)
                .Type<SnowflakeType>();

            descriptor
                .Ignore(x => x.DuplicitUsersBack)
                .Ignore(x => x.DuplicitUserId)
                .Ignore(x => x.DuplicitUser);
        }

        /*private class DbUserResolvers
        {
            
        }*/
    }
}