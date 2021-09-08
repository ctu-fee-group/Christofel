using Christofel.BaseLib.Database;
using Christofel.BaseLib.Discord;
using Christofel.BaseLib.Permissions;
using Christofel.BaseLib.Plugins;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Christofel.BaseLib
{
    /// <summary>
    /// Shared state between all modules
    /// </summary>
    public interface IChristofelState
    {
        /// <summary>
        /// The Bot state itself containing Discord client
        /// </summary>
        public IBot Bot { get; }
        
        /// <summary>
        /// Factory of the base context
        /// </summary>
        public IDbContextFactory<ChristofelBaseContext> DatabaseFactory { get; }

        /// <summary>
        /// Shared configuration linking name/key to value 
        /// </summary>
        public IConfiguration Configuration { get; }
        
        /// <summary>
        /// Service for registering and resolving permissions
        /// </summary>
        public IPermissionService Permissions { get; }

        /// <summary>
        /// Logger factory
        /// </summary>
        public ILoggerFactory LoggerFactory { get; }
        
        /// <summary>
        /// Lifetime of the application that may be used
        /// to stop the application
        /// </summary>
        public IApplicationLifetime Lifetime { get; }
    }
}