using Discord.Rest;

namespace Christofel.CommandsLib.Verifier.Interfaces
{
    public interface IHasRestUserMessage
    {
        public RestUserMessage? Message { get; set; }
    }
}