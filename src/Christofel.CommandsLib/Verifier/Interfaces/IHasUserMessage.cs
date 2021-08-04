using Discord;
using Discord.Rest;

namespace Christofel.CommandsLib.Verifier.Interfaces
{
    public interface IHasUserMessage
    {
        public IUserMessage? UserMessage { get; set; }
    }
}