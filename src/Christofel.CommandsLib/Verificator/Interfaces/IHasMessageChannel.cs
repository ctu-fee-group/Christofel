using Discord;

namespace Christofel.CommandsLib.Verificator.Interfaces
{
    public interface IHasMessageChannel
    {
        public IMessageChannel? Channel { get; set; }
    }
}