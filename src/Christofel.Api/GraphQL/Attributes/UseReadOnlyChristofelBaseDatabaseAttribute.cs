using System.Reflection;
using Christofel.Api.GraphQL.Extensions;
using Christofel.BaseLib.Database;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;

namespace Christofel.Api.GraphQL.Attributes
{
    /// <summary>
    /// Adds readonly ChristofelBaseContext as scoped service
    /// </summary>
    public class UseReadOnlyChristofelBaseDatabaseAttribute : ObjectFieldDescriptorAttribute
    {
        public override void OnConfigure(IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member)
        {
            descriptor.UseReadOnlyDbContext<ChristofelBaseContext>();
        }
    }
}