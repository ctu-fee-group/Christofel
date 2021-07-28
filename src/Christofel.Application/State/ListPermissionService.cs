using System;
using System.Collections.Generic;
using System.Linq;
using Christofel.BaseLib.Permissions;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.Application.State
{
    public sealed class ListPermissionService : IPermissionService
    {
        private List<IPermission> _permissions;
        private IServiceProvider _provider;
        
        public ListPermissionService(IServiceProvider provider)
        {
            _provider = provider;
            _permissions = new List<IPermission>();
        }

        public IPermissionsResolver Resolver => _provider.GetRequiredService<IPermissionsResolver>();

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