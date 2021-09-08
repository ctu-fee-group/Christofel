using Christofel.Application;
using Christofel.BaseLib.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Christofel.Design
{
    public class ChristofelBaseContextDesignFactory : IDesignTimeDbContextFactory<ChristofelBaseContext>
    {
        public ChristofelBaseContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<ChristofelBaseContext> builder = new DbContextOptionsBuilder<ChristofelBaseContext>();
            IConfiguration configuration = ChristofelApp.CreateConfiguration(args);
            string connectionString = configuration.GetConnectionString("ChristofelBase");
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            
            return new ChristofelBaseContext(builder.Options);
        }
    }
}