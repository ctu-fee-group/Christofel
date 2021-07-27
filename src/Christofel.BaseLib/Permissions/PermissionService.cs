using System.Collections.Generic;
using System.Linq;

namespace Christofel.BaseLib.Permissions
{
    public sealed class PermissionService : IPermissionService
    {
        private List<IPermission> _permissions;
        
        public PermissionService(IPermissionsResolver resolver)
        {
            Resolver = resolver;
            _permissions = new List<IPermission>();
        }
        
        public IPermissionsResolver Resolver { get; }

        public IEnumerable<IPermission> Permissions => _permissions.AsReadOnly();

        public void RegisterPermission(IPermission permission)
        {
            if (_permissions.All(x => x.Name != permission.Name))
            {
                _permissions.Add(permission);
            }
        }

        public void UnregisterPermission(IPermission permission)
        {
            _permissions.Remove(permission);
        }

        public void UnregisterPermission(string permissionName)
        {
            _permissions.RemoveAll(x => x.Name == permissionName);
        }
    }
}