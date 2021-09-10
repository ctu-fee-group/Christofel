//
//   SnowflakeParser.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Parsers;
using Remora.Commands.Results;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.CommandsLib.ContextedParsers
{
    /// <summary>
    /// Parses snowflake by unmentioning the value.
    /// </summary>
    public class SnowflakeParser : AbstractTypeParser<Snowflake>
    {
        /// <inheritdoc />
        public override ValueTask<Result<Snowflake>> TryParseAsync
            (string value, CancellationToken ct = default) => new ValueTask<Result<Snowflake>>
        (
            !Snowflake.TryParse(value.Unmention(), out var snowflake)
                ? new ParsingError<Snowflake>(value)
                : snowflake.Value
        );
    }
}