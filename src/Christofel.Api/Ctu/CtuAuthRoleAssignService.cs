using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Database;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;

namespace Christofel.Api.Ctu
{
    /// <summary>
    /// Service used for storing roles to be assigned in database,
    /// enqueues pending data from the database when needed
    /// </summary>
    public class CtuAuthRoleAssignService
    {
        private readonly IDbContextFactory<ApiCacheContext> _dbContextFactory;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly CtuAuthRoleAssignProcessor _roleAssignProcessor;

        public CtuAuthRoleAssignService(IDbContextFactory<ApiCacheContext> dbContextFactory,
            IDiscordRestGuildAPI guildApi, CtuAuthRoleAssignProcessor roleAssignProcessor)
        {
            _roleAssignProcessor = roleAssignProcessor;
            _guildApi = guildApi;
            _dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// Enqueues roles to be assigned and/or removed from the given user
        /// </summary>
        /// <param name="guildMember">Loaded guild member with roles that are currently</param>
        /// <param name="userId">Id of the user/member the roles should be assigned/removed from</param>
        /// <param name="guildId">Id of the guild where the roles should be assigned</param>
        /// <param name="assignRoles">What roles should be assigned to the member</param>
        /// <param name="removeRoles">What roles should be removed from the member</param>
        public void EnqueueRoles(IGuildMember guildMember, ulong userId, ulong guildId, IReadOnlyList<CtuAuthRole> assignRoles,
            IReadOnlyList<CtuAuthRole> removeRoles)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Save given roles to database to be able to retrieve them in case the bot is stopped
        /// </summary>
        /// <param name="userId">What user should the roles be assigned to</param>
        /// <param name="guildId">What guild are the roles in</param>
        /// <param name="assignRoles">What roles should be assigned</param>
        /// <param name="removeRoles">What roles should be deleted</param>
        /// <returns>Task saving the information to the database</returns>
        public Task SaveRoles(ulong userId, ulong guildId, IReadOnlyList<CtuAuthRole> assignRoles,
            IReadOnlyList<CtuAuthRole> removeRoles)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Remove roles of the given user from the database as they are no longer needed
        /// </summary>
        /// <param name="userId">What user should be removed from the database</param>
        /// <param name="guildId">What guild is the user in</param>
        /// <returns>Task removing the information from the database</returns>
        public Task RemoveRoles(ulong userId, ulong guildId)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Enqueue all roles to the processor
        /// </summary>
        /// <remarks>
        /// Enqueues roles from cache database,
        /// users who didn't get roles assigned in previous run,
        /// will get roles assigned in this one.
        /// </remarks>
        /// <returns>Number of users that were enqueued for role addition</returns>
        public Task<uint> EnqueueRemainingRoles()
        {
            throw new System.NotImplementedException();
        }
    }
}