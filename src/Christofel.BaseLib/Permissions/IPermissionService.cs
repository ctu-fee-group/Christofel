using System.Collections.Generic;

namespace Christofel.BaseLib.Permissions
{
    /// <summary>
    /// Service for registering permissions including permissions resolver
    /// </summary>
    public interface IPermissionService
    {
        public IPermissionsResolver Resolver { get; }

        public IEnumerable<IPermission> Permissions { get; }

        public void RegisterPermission(IPermission permission);
        public void UnregisterPermission(IPermission permission);
        public void UnregisterPermission(string permissionName);
    }
}