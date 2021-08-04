using Discord;

namespace Christofel.CommandsLib.Verifier.Interfaces
{
    public interface IHasEmbed
    {
        public Embed? Embed { get; set; }
    }
}