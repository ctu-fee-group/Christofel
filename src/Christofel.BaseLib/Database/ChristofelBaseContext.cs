using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Christofel.BaseLib.Database
{
    /// <summary>
    /// Context for base database holding users, permissions and information about roles
    /// </summary>
    public sealed class ChristofelBaseContext : DbContext, IReadableDbContext
    {
        public ChristofelBaseContext(DbContextOptions<ChristofelBaseContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbUser>()
                .HasOne<DbUser>(x => x.DuplicitUser!)
                .WithMany(x => x.DuplicitUsersBack!)
                .HasForeignKey(x => x.DuplicitUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProgrammeRoleAssignment>()
                .HasOne<RoleAssignment>(x => x.Assignment)
                .WithMany(x => x.ProgrammeRoleAssignments)
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<TitleRoleAssignment>()
                .HasOne<RoleAssignment>(x => x.Assignment)
                .WithMany(x => x.TitleRoleAssignments)
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<UsermapRoleAssignment>()
                .HasOne<RoleAssignment>(x => x.Assignment)
                .WithMany(x => x.UsermapRoleAssignments)
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<YearRoleAssignment>()
                .HasOne<RoleAssignment>(x => x.Assignment)
                .WithMany(x => x.YearRoleAssignments)
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<SpecificRoleAssignment>()
                .HasOne<RoleAssignment>(x => x.Assignment)
                .WithMany(x => x.SpecificRoleAssignments)
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            this.AddTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override int SaveChanges()
        {
            this.AddTimestamps();
            return base.SaveChanges();
        }

        public DbSet<DbUser> Users => Set<DbUser>();
        public DbSet<PermissionAssignment> Permissions => Set<PermissionAssignment>();
        public DbSet<RoleAssignment> RoleAssignments => Set<RoleAssignment>();
        
        public DbSet<YearRoleAssignment> YearRoleAssignments => Set<YearRoleAssignment>();
        public DbSet<SpecificRoleAssignment> SpecificRoleAssignments => Set<SpecificRoleAssignment>();
        public DbSet<ProgrammeRoleAssignment> ProgrammeRoleAssignments => Set<ProgrammeRoleAssignment>();
        public DbSet<UsermapRoleAssignment> UsermapRoleAssignments => Set<UsermapRoleAssignment>();
        public DbSet<TitleRoleAssignment> TitleRoleAssignment => Set<TitleRoleAssignment>();
        
        IQueryable<TEntity> IReadableDbContext.Set<TEntity>() where TEntity : class
        {
            return Set<TEntity>().AsNoTracking();
        }
    }
}