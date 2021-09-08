using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Parsers;
using Remora.Commands.Results;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.CommandsLib.ContextedParsers
{
    public class SnowflakeParser: AbstractTypeParser<Snowflake>
    {
        /// <inheritdoc />
        public override ValueTask<Result<Snowflake>> TryParseAsync(string value, CancellationToken ct = default)
        {
            return new
            (
                !Snowflake.TryParse(value.Unmention(), out var snowflake)
                    ? new ParsingError<Snowflake>(value)
                    : snowflake.Value
            );
        }
    }
}