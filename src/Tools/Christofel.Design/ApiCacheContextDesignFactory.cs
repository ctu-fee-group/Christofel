using Christofel.Api.Ctu.Database;
using Christofel.Application;
using Christofel.BaseLib.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Christofel.Design
{
    public class ApiCacheContextDesignFactory: IDesignTimeDbContextFactory<ApiCacheContext>
    {
        public ApiCacheContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<ApiCacheContext> builder = new DbContextOptionsBuilder<ApiCacheContext>();
            IConfiguration configuration = ChristofelApp.CreateConfiguration(args);
            string connectionString = configuration.GetConnectionString("ApiCache");
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            
            return new ApiCacheContext(builder.Options);
        }
    }
}