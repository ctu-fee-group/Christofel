using System.Collections.Generic;
using System.Threading.Tasks;
using Christofel.DatabaseMigrator.ModelMigrator;

namespace Christofel.DatabaseMigrator
{
    public class Migrator
    {
        private readonly IEnumerable<IModelMigrator> _modelMigrators;
        
        public Migrator(IEnumerable<IModelMigrator> modelMigrators)
        {
            _modelMigrators = modelMigrators;
        }

        public async Task Migrate()
        {
            foreach (var modelMigrator in _modelMigrators)
            {
                await modelMigrator.MigrateModel();
            }
        }
    }
}