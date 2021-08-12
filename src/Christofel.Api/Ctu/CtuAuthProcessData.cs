using System.Collections.Generic;
using System.Threading;
using Christofel.Api.OAuth;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Discord;
using Discord.Rest;

namespace Christofel.Api.Ctu
{
    public record CtuAuthProcessData(string AccessToken, CtuOauthHandler CtuOauthHandler,
        ChristofelBaseContext DbContext, DbUser DbUser, RestGuildUser GuildUser, CtuAuthAssignedRoles Roles, CancellationToken CancellationToken)
    {
        public bool Finished { get; set; }
    }

    public record CtuAuthRole
    {
        public ulong RoleId { get; init; }
        public RoleType Type { get; init; }
    };

    public class CtuAuthAssignedRoles
    {
        public CtuAuthAssignedRoles()
        {
            AddRoles = new HashSet<CtuAuthRole>();
            RemoveRoles = new HashSet<CtuAuthRole>();
        }
        
        public HashSet<CtuAuthRole> AddRoles { get; }

        public HashSet<CtuAuthRole> RemoveRoles { get; }

        public void AddRole(CtuAuthRole roleId)
        {
            AddRoles.Add(roleId);
            RemoveRoles.Remove(roleId);
        }

        public void RemoveRole(CtuAuthRole roleId)
        {
            AddRoles.Remove(roleId);
            RemoveRoles.Add(roleId);
        }

        public void AddRange(IEnumerable<CtuAuthRole> roleIds)
        {
            foreach (CtuAuthRole roleId in roleIds)
            {
                AddRole(roleId);
            }
        }

        public void RemoveRange(IEnumerable<CtuAuthRole> roleIds)
        {
            foreach (CtuAuthRole roleId in roleIds)
            {
                RemoveRole(roleId);
            }
        }
    }
}