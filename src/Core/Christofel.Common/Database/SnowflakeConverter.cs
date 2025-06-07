//
//   SnowflakeConverter.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Remora.Discord.API;
using Remora.Rest.Core;

namespace Christofel.Common.Database
{
    /// <summary>
    /// Converts <see cref="Snowflake"/> instances to and from a database provider representation.
    /// </summary>
    public class SnowflakeConverter : ValueConverter<Snowflake, long>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnowflakeConverter"/> class.
        /// </summary>
        public SnowflakeConverter()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnowflakeConverter"/> class.
        /// </summary>
        /// <param name="mappingHints">The mapping hints.</param>
        public SnowflakeConverter
        (
            ConverterMappingHints? mappingHints = null
        )
            : base
            (
                v => (long)v.Value,
                v => new Snowflake((ulong)v, Constants.DiscordEpoch),
                mappingHints
            )
        {
        }
    }
}