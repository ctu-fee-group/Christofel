namespace Christofel.BaseLib.User
{
    public interface IDiscordUser
    {
        public ulong DiscordId { get; set; }
    }
    
    public interface ICtuUser
    {
        public string CtuUsername { get; set; }
    }

    /// <summary>
    /// Ctu and Discord link user
    /// </summary>
    public interface ILinkUser : ICtuUser, IDiscordUser
    {
        
    }
}