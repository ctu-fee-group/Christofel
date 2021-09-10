//
//   UseReadOnlyChristofelBaseDatabaseAttribute.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Christofel.Api.GraphQL.Extensions;
using Christofel.BaseLib.Database;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;

namespace Christofel.Api.GraphQL.Attributes
{
    /// <summary>
    /// Adds readonly ChristofelBaseContext as scoped service.
    /// </summary>
    public class UseReadOnlyChristofelBaseDatabaseAttribute : ObjectFieldDescriptorAttribute
    {
        /// <inheritdoc />
        public override void OnConfigure
            (IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member)
        {
            descriptor.UseReadOnlyDbContext<ChristofelBaseContext>();
        }
    }
}