using System;
using System.Collections.Generic;
using System.Threading;
using Christofel.Api.OAuth;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Christofel.BaseLib.User;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.API.Abstractions.Objects;

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
        ICtuUser LoadedUser { get; }
        
        /// <summary>
        /// Id of the guild where the user is located
        /// </summary>
        ulong GuildId { get; }
        
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
        
        /// <summary>
        /// Data persistent throughout steps for passing valuable information
        /// </summary>
        Dictionary<string, object> StepData { get; }
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
            ICtuUser LoadedUser,
            ulong GuildId,
            ChristofelBaseContext DbContext,
            DbUser DbUser,
            IGuildMember GuildUser,
            CtuAuthAssignedRoles Roles,
            Dictionary<string, object> StepData)
        : IAuthData;

    /// <summary>
    /// Role to be assigned or deleted
    /// </summary>
    public record CtuAuthRole
    {
        public ulong RoleId { get; init; }
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
            AddRoles = new HashSet<CtuAuthRole>();
            SoftRemoveRoles = new HashSet<CtuAuthRole>();
        }
        
        public HashSet<CtuAuthRole> AddRoles { get; }

        public HashSet<CtuAuthRole> SoftRemoveRoles { get; }

        public void AddRole(CtuAuthRole roleId)
        {
            lock (AddRoles)
            {
                AddRoles.Add(roleId);
                SoftRemoveRoles.Remove(roleId);
            }
        }

        public void SoftRemoveRole(CtuAuthRole roleId)
        {
            lock (AddRoles)
            {
                SoftRemoveRoles.Add(roleId);
            }
        }

        public void AddRange(IEnumerable<CtuAuthRole> roleIds)
        {
            lock (AddRoles)
            {
                foreach (CtuAuthRole roleId in roleIds)
                {
                    AddRole(roleId);
                }
            }
        }

        public void SoftRemoveRange(IEnumerable<CtuAuthRole> roleIds)
        {
            lock (AddRoles)
            {
                foreach (CtuAuthRole roleId in roleIds)
                {
                    SoftRemoveRole(roleId);
                }
            }
        }
    }
}