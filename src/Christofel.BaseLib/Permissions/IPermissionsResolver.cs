using System.Collections.Generic;
using System.Threading.Tasks;
using Christofel.BaseLib.Database.Models;

namespace Christofel.BaseLib.Permissions
{
    /// <summary>
    /// Get discord targets off of permissions or whether specified target has permissions
    /// </summary>
    public interface IPermissionsResolver
    {
        /// <summary>
        /// Get what targets have permission with the given name
        /// </summary>
        /// <param name="permissionName">Name of the permission</param>
        /// <returns>Collection containing all DiscordTargets</returns>
        public Task<IEnumerable<DiscordTarget>> GetPermissionTargetsAsync(string permissionName);
        
        /// <summary>
        /// Get what targets have permission with the given name
        /// </summary>
        /// <param name="permission">Name of the permission</param>
        /// <returns>Collection containing all DiscordTargets</returns>
        public Task<IEnumerable<DiscordTarget>> GetPermissionTargetsAsync(IPermission permission);

        /// <summary>
        /// Check for permission on specified target
        /// </summary>
        /// <param name="permissionName"></param>
        /// <param name="target"></param>
        /// <returns>Whether the target has the permission</returns>
        public Task<bool> HasPermissionAsync(string permissionName, DiscordTarget target);
        
        /// <summary>
        /// Check for permission on specified target
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="target"></param>
        /// <returns>Whether the target has the permission</returns>
        public Task<bool> HasPermissionAsync(IPermission permission, DiscordTarget target);
        
        public Task<bool> AnyHasPermissionAsync(string permissionName, IEnumerable<DiscordTarget> target);
        public Task<bool> AnyHasPermissionAsync(IPermission permission, IEnumerable<DiscordTarget> target);
    }
}