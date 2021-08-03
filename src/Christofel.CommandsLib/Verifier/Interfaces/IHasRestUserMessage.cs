using Discord.Rest;

namespace Christofel.CommandsLib.Verificator.Interfaces
{
    public interface IHasRestUserMessage
    {
        public RestUserMessage? Message { get; set; }
    }
}