namespace Christofel.Management.Commands.Verifiers
{
    public interface IHasDiscordId
    {
        public ulong? DiscordId { get; set; }
    }
}