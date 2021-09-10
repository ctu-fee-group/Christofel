//
//   ChristofelBaseContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Extensions;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.Core;

namespace Christofel.BaseLib.Database
{
    /// <summary>
    /// Context for base database holding users, permissions and information about roles
    /// </summary>
    public sealed class ChristofelBaseContext : DbContext, IReadableDbContext<ChristofelBaseContext>
    {
        public ChristofelBaseContext(DbContextOptions<ChristofelBaseContext> options)
            : base(options)
        {
        }

        public DbSet<DbUser> Users => Set<DbUser>();
        public DbSet<PermissionAssignment> Permissions => Set<PermissionAssignment>();
        public DbSet<RoleAssignment> RoleAssignments => Set<RoleAssignment>();

        public DbSet<YearRoleAssignment> YearRoleAssignments => Set<YearRoleAssignment>();
        public DbSet<SpecificRoleAssignment> SpecificRoleAssignments => Set<SpecificRoleAssignment>();
        public DbSet<ProgrammeRoleAssignment> ProgrammeRoleAssignments => Set<ProgrammeRoleAssignment>();
        public DbSet<UsermapRoleAssignment> UsermapRoleAssignments => Set<UsermapRoleAssignment>();
        public DbSet<TitleRoleAssignment> TitleRoleAssignment => Set<TitleRoleAssignment>();

        IQueryable<TEntity> IReadableDbContext.Set<TEntity>()
            where TEntity : class => Set<TEntity>().AsNoTracking();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbUser>()
                .Property(x => x.DiscordId)
                .HasConversion(v => (long) v.Value, v => new Snowflake((ulong) v));

            modelBuilder.Entity<RoleAssignment>()
                .Property(x => x.RoleId)
                .HasConversion(v => (long) v.Value, v => new Snowflake((ulong) v));

            modelBuilder.Entity<PermissionAssignment>()
                .OwnsOne
                (
                    x => x.Target,
                    b => b
                        .Property(x => x.DiscordId)
                        .HasConversion(v => (long) v.Value, v => new Snowflake((ulong) v))
                );

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
        }

        public override Task<int> SaveChangesAsync
        (
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationtoken = default
        )
        {
            this.AddTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override int SaveChanges()
        {
            this.AddTimestamps();
            return base.SaveChanges();
        }
    }
}