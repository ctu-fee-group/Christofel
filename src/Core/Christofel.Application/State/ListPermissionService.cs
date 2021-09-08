using System;
using System.Collections.Generic;
using System.Linq;
using Christofel.BaseLib.Permissions;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.Application.State
{
    /// <summary>
    /// Thread-safe is achieved using locks.
    /// This service should not be modified to by other threads,
    /// but just to be sure
    /// </summary>
    public sealed class ListPermissionService : IPermissionService
    {
        private List<IPermission> _permissions;
        private IServiceProvider _provider;
        private object _threadLock = new object();
        
        public ListPermissionService(IServiceProvider provider)
        {
            _provider = provider;
            _permissions = new List<IPermission>();
        }

        public IPermissionsResolver Resolver => _provider.GetRequiredService<IPermissionsResolver>();

        public IEnumerable<IPermission> Permissions
        {
            get
            {
                lock (_threadLock)
                {
                    return new List<IPermission>(_permissions);
                }
            }
        }

        public void RegisterPermission(IPermission permission)
        {
            lock (_threadLock)
            {
                if (_permissions.All(x => x.PermissionName != permission.PermissionName))
                {
                    _permissions.Add(permission);
                }
            }
        }

        public void UnregisterPermission(IPermission permission)
        {
            lock (_threadLock)
            {
                _permissions.Remove(permission);
            }
        }

        public void UnregisterPermission(string permissionName)
        {
            lock (_threadLock)
            {
                _permissions.RemoveAll(x => x.PermissionName == permissionName);
            }
        }
    }
}