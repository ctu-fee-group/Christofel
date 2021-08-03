using Discord;

namespace Christofel.CommandsLib.Verificator.Interfaces
{
    public interface IHasEmote
    {
        public IEmote? Emote { get; set; }
    }
}