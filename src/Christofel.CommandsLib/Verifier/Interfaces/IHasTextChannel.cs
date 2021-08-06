using Discord;

namespace Christofel.CommandsLib.Verifier.Interfaces
{
    public interface IHasTextChannel
    {
        ITextChannel? TextChannel { get; set; }
    }
}