using Discord;

namespace Christofel.CommandsLib.Verifier.Interfaces
{
    public interface IHasMessageChannel
    {
        public IMessageChannel? Channel { get; set; }
    }
}