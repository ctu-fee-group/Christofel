//
//   CtuAuthAssignedRoles.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Christofel.Common.Database.Models.Enums;
using Remora.Rest.Core;

namespace Christofel.Api.Ctu
{
    /// <summary>
    /// Holds information about roles to be added and removed,
    /// should be changed during the auth process.
    /// </summary>
    public class CtuAuthAssignedRoles
    {
        private readonly HashSet<CtuAuthRole> _addRoles;

        private readonly HashSet<CtuAuthRole> _softRemoveRoles;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtuAuthAssignedRoles"/> class.
        /// </summary>
        public CtuAuthAssignedRoles()
        {
            _addRoles = new HashSet<CtuAuthRole>();
            _softRemoveRoles = new HashSet<CtuAuthRole>();
        }

        /// <summary>
        /// Gets read only list of the roles that should be added.
        /// </summary>
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

        /// <summary>
        /// Gets read only list of the roles that should be removed.
        /// </summary>
        /// <remarks>
        /// These roles should be removed only if they are not located in <see cref="AddRoles"/> as well.
        /// </remarks>
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

        /// <summary>
        /// Adds role to the storage.
        /// </summary>
        /// <param name="roleId">The id of the role to add.</param>
        public void AddRole(CtuAuthRole roleId)
        {
            lock (_addRoles)
            {
                _addRoles.Add(roleId);
            }
        }

        /// <summary>
        /// Adds role to the soft remove roles.
        /// </summary>
        /// <param name="roleId">The id of the role to soft remove.</param>
        public void SoftRemoveRole(CtuAuthRole roleId)
        {
            lock (_addRoles)
            {
                _softRemoveRoles.Add(roleId);
            }
        }

        /// <summary>
        /// Adds multiple roles.
        /// </summary>
        /// <param name="roleIds">The id of the roles to add.</param>
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

        /// <summary>
        /// Soft removes multiple roles.
        /// </summary>
        /// <param name="roleIds">The id of the roles to soft remove.</param>
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

    /// <summary>
    /// Role to be assigned or deleted
    /// </summary>
    public record CtuAuthRole
    {
        /// <summary>
        /// Gets the id of the role.
        /// </summary>
        public Snowflake RoleId { get; init; }

        /// <summary>
        /// Gets type of the role.
        /// </summary>
        public RoleType Type { get; init; }

        /// <summary>
        /// Gets description of the assignment.
        /// </summary>
        public string? Description { get; init; }
    }
}