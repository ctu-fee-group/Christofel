namespace Christofel.BaseLib.Database.Models.Enums
{
    /// <summary>
    /// Target type (primarily for permissions), meaning if to get discord user, discord role or everyone.
    /// </summary>
    public enum TargetType
    {
        /// <summary>
        /// Targets specific discord user
        /// </summary>
        User,
        
        /// <summary>
        /// Targets specific discord role
        /// </summary>
        Role,
        
        /// <summary>
        /// Targets everyone
        /// </summary>
        Everyone
    }
}