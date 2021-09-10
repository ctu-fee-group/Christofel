//
//   IChristofelState.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Database;
using Christofel.BaseLib.Discord;
using Christofel.BaseLib.Permissions;
using Christofel.Plugins.Lifetime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Christofel.BaseLib
{
    /// <summary>
    /// Shared state between all plugins.
    /// </summary>
    public interface IChristofelState
    {
        /// <summary>
        /// Gets bot state itself containing Discord client.
        /// </summary>
        public IBot Bot { get; }

        /// <summary>
        /// Gets factory of the base context.
        /// </summary>
        public IDbContextFactory<ChristofelBaseContext> DatabaseFactory { get; }

        /// <summary>
        /// Gets shared configuration linking name/key to value.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets service for registering and resolving permissions.
        /// </summary>
        public IPermissionService Permissions { get; }

        /// <summary>
        /// Gets default configured logger factory.
        /// </summary>
        public ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Gets lifetime of the application that may be used
        /// to stop the application.
        /// </summary>
        public IApplicationLifetime Lifetime { get; }
    }
}