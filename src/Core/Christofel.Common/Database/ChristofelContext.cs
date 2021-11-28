//
//   ChristofelContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;

namespace Christofel.Common.Database
{
    /// <summary>
    /// The base database context.
    /// </summary>
    public class ChristofelContext : DbContext
    {
        /// <summary>
        /// Gets the schema of the database.
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChristofelContext"/> class.
        /// </summary>
        /// <param name="schema">The schema managed by the context.</param>
        /// <param name="contextOptions">The context options.</param>
        public ChristofelContext(string schema, DbContextOptions contextOptions)
            : base(contextOptions)
        {
            this.Schema = schema;
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema(Schema);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.GetSchema() == Schema)
                {
                    continue;
                }

                entityType.SetIsTableExcludedFromMigrations(true);
            }
        }

        /// <inheritdoc />
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            configurationBuilder.Properties<Snowflake>().HaveConversion(typeof(SnowflakeConverter));
        }
    }
}