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
        public DbSet<ProgrammeRoleAssignment> ProgrammeRoleAssignments => Set<ProgrammeRoleAssignment>();
        public DbSet<UsermapRoleAssignment> UsermapRoleAssignments => Set<UsermapRoleAssignment>();
        public DbSet<TitleRoleAssignment> TitleRoleAssignment => Set<TitleRoleAssignment>();
        
        IQueryable<TEntity> IReadableDbContext.Set<TEntity>() where TEntity : class
        {
            return Set<TEntity>().AsNoTracking();
        }
    }
}