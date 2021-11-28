//
//   SnowflakeType.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Types;
using Remora.Discord.API;
using Remora.Rest.Core;

namespace Christofel.Api.GraphQL.Types
{
    /// <summary>
    /// GraphQL type configuration representing <see cref="Snowflake"/>.
    /// </summary>
    public class SnowflakeType : ScalarType<Snowflake, IntValueNode>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnowflakeType" /> class.
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
        /// Initializes a new instance of the <see cref="SnowflakeType"/> class.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="description">The description of the field.</param>
        /// <param name="bind">The binding behavior.</param>
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

        /// <inheritdoc/>
        public override IValueNode ParseResult(object? resultValue) => ParseValue(resultValue);

        /// <inheritdoc/>
        protected override Snowflake ParseLiteral(IntValueNode valueSyntax) => new Snowflake(valueSyntax.ToUInt64(), Constants.DiscordEpoch);

        /// <inheritdoc/>
        protected override IntValueNode ParseValue(Snowflake runtimeValue) => new IntValueNode(runtimeValue.Value);
    }
}