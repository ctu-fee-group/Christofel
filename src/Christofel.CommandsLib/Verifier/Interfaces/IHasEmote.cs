using Discord;

namespace Christofel.CommandsLib.Verifier.Interfaces
{
    public interface IHasEmote
    {
        public IEmote? Emote { get; set; }
    }
}