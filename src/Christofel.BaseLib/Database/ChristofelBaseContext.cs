using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Christofel.BaseLib.Database
{
    public sealed class ChristofelBaseContext : DbContext
    {
        public ChristofelBaseContext(DbContextOptions<ChristofelBaseContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
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
        public DbSet<ConfigurationEntry> Configuration => Set<ConfigurationEntry>();
        
        public DbSet<RoleAssignment> RoleAssignments => Set<RoleAssignment>();
        
        public DbSet<YearRoleAssignment> YearRoleAssignments => Set<YearRoleAssignment>();
        public DbSet<ProgrammeRoleAssignment> ProgrammeRoleAssignments => Set<ProgrammeRoleAssignment>();
        public DbSet<UsermapRoleAssignment> UsermapRoleAssignments => Set<UsermapRoleAssignment>();
        public DbSet<TitleRoleAssignment> TitleRoleAssignment => Set<TitleRoleAssignment>();
    }
}