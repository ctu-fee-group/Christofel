//
//   DbContextExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Christofel.BaseLib.Database.Models.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Christofel.BaseLib.Extensions
{
    /// <summary>
    /// Class containing extensions for waiting for <see cref="DbContext"/> to change into some state.
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// This method should be called from SaveChanges and (or) SaveChangesAsync.
        /// Add support for ITimestampsEntity automatically setting CreatedAt and UpdatedAt.
        /// </summary>
        /// <param name="context">The context that should be changed.</param>
        public static void AddTimestamps(this DbContext context)
        {
            var entries = context.ChangeTracker
                .Entries()
                .Where
                (
                    e => e.Entity is ITimestampsEntity &&
                         (
                             e.State == EntityState.Added || e.State == EntityState.Modified)
                );

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