//
//   ListPermissionService.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Christofel.BaseLib.Permissions;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.Application.State
{
    /// <summary>
    /// Permission service using a list for the storage.
    /// </summary>
    /// <remarks>
    /// Thread-safety is achieved using locks.
    /// </remarks>
    public sealed class ListPermissionService : IPermissionService
    {
        private readonly List<IPermission> _permissions;
        private readonly IServiceProvider _provider;
        private readonly object _threadLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="ListPermissionService"/> class.
        /// </summary>
        /// <param name="provider">The provider of the services.</param>
        public ListPermissionService(IServiceProvider provider)
        {
            _provider = provider;
            _permissions = new List<IPermission>();
        }

        /// <inheritdoc />
        public IPermissionsResolver Resolver => _provider.GetRequiredService<IPermissionsResolver>();

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void UnregisterPermission(IPermission permission)
        {
            lock (_threadLock)
            {
                _permissions.Remove(permission);
            }
        }

        /// <inheritdoc />
        public void UnregisterPermission(string permissionName)
        {
            lock (_threadLock)
            {
                _permissions.RemoveAll(x => x.PermissionName == permissionName);
            }
        }
    }
}