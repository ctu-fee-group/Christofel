using Discord;
using Discord.Rest;

namespace Christofel.CommandsLib.Verifier.Interfaces
{
    public interface IHasRestUserMessage
    {
        public IUserMessage? UserMessage { get; set; }
    }
}