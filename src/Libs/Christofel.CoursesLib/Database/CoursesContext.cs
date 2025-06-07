//
//   CoursesContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Christofel.CoursesLib.Database;

/// <summary>
/// Context for courses.
/// </summary>
public class CoursesContext : ChristofelContext, IReadableDbContext<CoursesContext>
{
    /// <summary>
    /// The name of the schema that this context's entities lie in.
    /// </summary>
    public const string SchemaName = "Courses";

    /// <summary>
    /// Initializes a new instance of the <see cref="CoursesContext"/> class.
    /// </summary>
    /// <param name="contextOptions">The context options.</param>
    public CoursesContext(DbContextOptions contextOptions)
        : base(SchemaName, contextOptions)
    {
    }

    /// <summary>
    /// Gets courses assignments set.
    /// </summary>
    public DbSet<CourseAssignment> CourseAssignments => Set<CourseAssignment>();

    /// <summary>
    /// Gets courses assignments set.
    /// </summary>
    public DbSet<DepartmentAssignment> DepartmentAssignments => Set<DepartmentAssignment>();

    /// <summary>
    /// Gets courses group assignment set.
    /// </summary>
    public DbSet<CourseGroupAssignment> CourseGroupAssignments => Set<CourseGroupAssignment>();

    /// <summary>
    /// Gets course user link set.
    /// </summary>
    public DbSet<CourseUser> CourseUsers => Set<CourseUser>();

    /// <inheritdoc/>
    IQueryable<TEntity> IReadableDbContext.Set<TEntity>()
        where TEntity : class
        => Set<TEntity>().AsNoTracking();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CourseAssignment>()
            .HasOne(x => x.Department)
            .WithMany(x => x.Courses)
            .HasPrincipalKey(x => x.DepartmentKey)
            .HasForeignKey(x => x.DepartmentKey)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CourseAssignment>()
            .HasOne(x => x.GroupAssignment)
            .WithMany(x => x.Courses)
            .HasPrincipalKey(x => x.ChannelId)
            .HasForeignKey(x => x.ChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CourseUser>()
            .HasOne(x => x.Course)
            .WithMany()
            .HasPrincipalKey(x => x.CourseKey)
            .HasForeignKey(x => x.CourseKey)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseAssignment>()
            .HasIndex(x => x.CourseKey)
            .IsUnique();

        modelBuilder.Entity<DepartmentAssignment>()
            .HasIndex(x => x.DepartmentKey)
            .IsUnique();

        modelBuilder.Entity<CourseGroupAssignment>()
            .HasIndex(x => x.ChannelId)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}