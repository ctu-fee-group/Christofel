//
//   SnowflakeType.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Types;
using Remora.Discord.Core;

namespace Christofel.Api.GraphQL.Types
{
    public class SnowflakeType : ScalarType<Snowflake, IntValueNode>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SnowflakeType" /> class.
        /// </summary>
        public SnowflakeType()
            : this
            (
                "UnsignedLong",
                "Snowflake ulong type"
            )
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SnowflakeType" /> class.
        /// </summary>
        public SnowflakeType
        (
            NameString name,
            string? description = null,
            BindingBehavior bind = BindingBehavior.Explicit
        )
            : base(name, bind)
        {
            Description = description;
        }

        public override IValueNode ParseResult(object? resultValue) => ParseValue(resultValue);

        protected override Snowflake ParseLiteral(IntValueNode valueSyntax) => new Snowflake(valueSyntax.ToUInt64());

        protected override IntValueNode ParseValue(Snowflake runtimeValue) => new IntValueNode(runtimeValue.Value);
    }
}