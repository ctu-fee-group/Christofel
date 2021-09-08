using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Christofel.Api.OAuth;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Christofel.BaseLib.User;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Core;

namespace Christofel.Api.Ctu
{

    /// <summary>
    /// Data used in ctu authentication process
    /// </summary>
    public interface IAuthData
    {
        /// <summary>
        /// Access token that can be used to provide access to ctu services
        /// </summary>
        [Obsolete("Use injection of authorized apis instead")]
        string AccessToken { get; }
        
        /// <summary>
        /// Loaded user from oauth check token
        /// </summary>
        ILinkUser LoadedUser { get; }
        
        /// <summary>
        /// Id of the guild where the user is located
        /// </summary>
        Snowflake GuildId { get; }
        
        ChristofelBaseContext DbContext { get; }
        
        /// <summary>
        /// User stored in the database, can be edited during the process
        /// </summary>
        DbUser DbUser { get; }
        
        /// <summary>
        /// What guild user is in question of the auth process
        /// </summary>
        IGuildMember GuildUser { get; }
        
        /// <summary>
        /// What roles should be assigned and removed at the end of the process
        /// </summary>
        CtuAuthAssignedRoles Roles { get; }
    }

    /// <summary>
    /// Data used along the ctu auth process in each step
    /// </summary>
    /// <param name="AccessToken"></param>
    /// <param name="DbContext"></param>
    /// <param name="DbUser"></param>
    /// <param name="GuildUser"></param>
    /// <param name="Roles"></param>
    public record CtuAuthProcessData(
            string AccessToken,
            ILinkUser LoadedUser,
            Snowflake GuildId,
            ChristofelBaseContext DbContext,
            DbUser DbUser,
            IGuildMember GuildUser,
            CtuAuthAssignedRoles Roles)
        : IAuthData;

    /// <summary>
    /// Role to be assigned or deleted
    /// </summary>
    public record CtuAuthRole
    {
        public Snowflake RoleId { get; init; }
        public RoleType Type { get; init; }
        
        public string? Description { get; init; }
    };

    /// <summary>
    /// Holds information about roles to be added and removed,
    /// should be changed during the auth process
    /// </summary>
    public class CtuAuthAssignedRoles
    {
        public CtuAuthAssignedRoles()
        {
            _addRoles = new HashSet<CtuAuthRole>();
            _softRemoveRoles = new HashSet<CtuAuthRole>();
        }

        private readonly HashSet<CtuAuthRole> _addRoles;

        private readonly HashSet<CtuAuthRole> _softRemoveRoles;

        public IReadOnlyList<CtuAuthRole> AddRoles
        {
            get
            {
                lock (_addRoles)
                {
                    return new List<CtuAuthRole>(_addRoles);
                }
            }
        }
        
        public IReadOnlyList<CtuAuthRole> SoftRemoveRoles
        {
            get
            {
                lock (_addRoles)
                {
                    return new List<CtuAuthRole>(_softRemoveRoles);
                }
            }
        }

        public void AddRole(CtuAuthRole roleId)
        {
            lock (_addRoles)
            {
                _addRoles.Add(roleId);
                _softRemoveRoles.Remove(roleId);
            }
        }

        public void SoftRemoveRole(CtuAuthRole roleId)
        {
            lock (_addRoles)
            {
                _softRemoveRoles.Add(roleId);
            }
        }

        public void AddRange(IEnumerable<CtuAuthRole> roleIds)
        {
            lock (_addRoles)
            {
                foreach (CtuAuthRole roleId in roleIds)
                {
                    AddRole(roleId);
                }
            }
        }

        public void SoftRemoveRange(IEnumerable<CtuAuthRole> roleIds)
        {
            lock (_addRoles)
            {
                foreach (CtuAuthRole roleId in roleIds)
                {
                    SoftRemoveRole(roleId);
                }
            }
        }
    }
}