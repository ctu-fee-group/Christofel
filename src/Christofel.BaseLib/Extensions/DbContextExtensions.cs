using System;
using System.Linq;
using System.Threading.Tasks;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Christofel.BaseLib.Extensions
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// This method should be called from SaveChanges and (or) SaveChangesAsync
        /// Add support for ITimestampsEntity automatically setting CreatedAt and UpdatedAt
        /// </summary>
        /// <param name="context"></param>
        public static void AddTimestamps(this DbContext context)
        {
            var entries = context.ChangeTracker
                .Entries()
                .Where(e => e.Entity is ITimestampsEntity && (
                    e.State == EntityState.Added
                    || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    ((ITimestampsEntity)entityEntry.Entity).CreatedAt = DateTime.Now;
                }
                else
                {
                    ((ITimestampsEntity)entityEntry.Entity).UpdatedAt = DateTime.Now;
                }
            }
        }
    }
}