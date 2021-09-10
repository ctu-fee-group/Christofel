//
//   ChristofelState.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Discord;
using Christofel.BaseLib.Permissions;
using Christofel.Plugins.Lifetime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Christofel.Application.State
{
    /// <inheritdoc />
    public class ChristofelState : IChristofelState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChristofelState"/> class.
        /// </summary>
        /// <param name="bot">The discord bot.</param>
        /// <param name="factory">The christofel base context factory.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="permissions">The permissions service.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="lifetime">The lifetime of the application.</param>
        public ChristofelState
        (
            IBot bot,
            IDbContextFactory<ChristofelBaseContext> factory,
            IConfiguration config,
            IPermissionService permissions,
            ILoggerFactory loggerFactory,
            IApplicationLifetime lifetime
        )
        {
            Bot = bot;
            DatabaseFactory = factory;
            Configuration = config;
            Permissions = permissions;
            LoggerFactory = loggerFactory;
            Lifetime = lifetime;
        }

        /// <inheritdoc />
        public IBot Bot { get; }

        /// <inheritdoc />
        public IDbContextFactory<ChristofelBaseContext> DatabaseFactory { get; }

        /// <inheritdoc />
        public IConfiguration Configuration { get; }

        /// <inheritdoc />
        public IPermissionService Permissions { get; }

        /// <inheritdoc />
        public ILoggerFactory LoggerFactory { get; }

        /// <inheritdoc />
        public IApplicationLifetime Lifetime { get; }
    }
}