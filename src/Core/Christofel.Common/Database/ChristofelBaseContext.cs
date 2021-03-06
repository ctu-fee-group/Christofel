//
//   ChristofelBaseContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Common.Database.Models;
using Christofel.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.API;
using Remora.Rest.Core;

namespace Christofel.Common.Database
{
    /// <summary>
    /// Context for base database holding users, permissions and information about roles.
    /// </summary>
    public sealed class ChristofelBaseContext : ChristofelContext, IReadableDbContext<ChristofelBaseContext>
    {
        /// <summary>
        /// The name of the schema that this context's entities lie in.
        /// </summary>
        public const string SchemaName = "Core";

        /// <summary>
        /// Initializes a new instance of the <see cref="ChristofelBaseContext"/> class.
        /// </summary>
        /// <param name="options">The options for the context.</param>
        public ChristofelBaseContext(DbContextOptions<ChristofelBaseContext> options)
            : base(SchemaName, options)
        {
        }

        /// <summary>
        /// Gets users set.
        /// </summary>
        public DbSet<DbUser> Users => Set<DbUser>();

        /// <summary>
        /// Gets permissions set.
        /// </summary>
        public DbSet<PermissionAssignment> Permissions => Set<PermissionAssignment>();

        /// <summary>
        /// Gets role assignments set.
        /// </summary>
        public DbSet<RoleAssignment> RoleAssignments => Set<RoleAssignment>();

        /// <summary>
        /// Gets year role assignments set.
        /// </summary>
        public DbSet<YearRoleAssignment> YearRoleAssignments => Set<YearRoleAssignment>();

        /// <summary>
        /// Gets specific role assignments set.
        /// </summary>
        public DbSet<SpecificRoleAssignment> SpecificRoleAssignments => Set<SpecificRoleAssignment>();

        /// <summary>
        /// Gets programme role assignments set.
        /// </summary>
        public DbSet<ProgrammeRoleAssignment> ProgrammeRoleAssignments => Set<ProgrammeRoleAssignment>();

        /// <summary>
        /// Gets usermap role assignments set.
        /// </summary>
        public DbSet<UsermapRoleAssignment> UsermapRoleAssignments => Set<UsermapRoleAssignment>();

        /// <summary>
        /// Gets title role assignments set.
        /// </summary>
        public DbSet<TitleRoleAssignment> TitleRoleAssignment => Set<TitleRoleAssignment>();

        /// <inheritdoc/>
        IQueryable<TEntity> IReadableDbContext.Set<TEntity>()
            where TEntity : class => Set<TEntity>().AsNoTracking();

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbUser>()
                .HasOne<DbUser>(x => x.DuplicitUser!)
                .WithMany(x => x.DuplicitUsersBack!)
                .HasForeignKey(x => x.DuplicitUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProgrammeRoleAssignment>()
                .HasOne(x => x.Assignment)
                .WithMany(x => x.ProgrammeRoleAssignments)
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TitleRoleAssignment>()
                .HasOne(x => x.Assignment)
                .WithMany(x => x.TitleRoleAssignments)
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UsermapRoleAssignment>()
                .HasOne(x => x.Assignment)
                .WithMany(x => x.UsermapRoleAssignments)
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<YearRoleAssignment>()
                .HasOne(x => x.Assignment)
                .WithMany(x => x.YearRoleAssignments)
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SpecificRoleAssignment>()
                .HasOne(x => x.Assignment)
                .WithMany(x => x.SpecificRoleAssignments)
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }

        /// <inheritdoc />
        public override Task<int> SaveChangesAsync
        (
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default
        )
        {
            this.AddTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <inheritdoc />
        public override int SaveChanges()
        {
            this.AddTimestamps();
            return base.SaveChanges();
        }
    }
}