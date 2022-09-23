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
    /// <param name="schema">The schema managed by the context.</param>
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

    /// <inheritdoc/>
    IQueryable<TEntity> IReadableDbContext.Set<TEntity>()
        where TEntity : class => Set<TEntity>().AsNoTracking();
}