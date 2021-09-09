using System.Threading.Tasks;

namespace Christofel.DatabaseMigrator.ModelMigrator
{
    public interface IModelMigrator
    {
        public Task MigrateModel();
    }
}