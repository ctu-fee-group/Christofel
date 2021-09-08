using Remora.Discord.Core;

namespace Christofel.BaseLib.User
{
    public interface IDiscordUser : IUser
    {
        public Snowflake DiscordId { get; }
    }
    
    /// <summary>
    /// Ctu user should be used everywhere where only Ctu information are needed
    /// </summary>
    public interface ICtuUser : IUser
    {
        public string CtuUsername { get; }
    }

    /// <summary>
    /// User with user id only - to link to database
    /// </summary>
    public interface IUser
    {
        /// <summary>
        /// Database primary key
        /// </summary>
        public int UserId { get; }
    }

    /// <summary>
    /// Ctu and Discord link user
    /// </summary>
    public interface ILinkUser : ICtuUser, IDiscordUser
    {
    }
}