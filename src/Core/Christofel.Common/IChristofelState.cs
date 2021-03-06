//
//   IChristofelState.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Christofel.Common.Database;
using Christofel.Common.Discord;
using Christofel.Common.Permissions;
using Christofel.Plugins.Lifetime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.Caching.Abstractions.Services;

namespace Christofel.Common
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

        /// <summary>
        /// Gets the cache provider.
        /// </summary>
        public ICacheProvider CacheProvider { get; }

        /// <summary>
        /// Gets options for json that are generated by Remora.
        /// </summary>
        public IOptionsMonitor<JsonSerializerOptions> DiscordJsonOptions { get; }
    }
}