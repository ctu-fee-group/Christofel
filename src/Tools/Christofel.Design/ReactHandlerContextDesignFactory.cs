using Christofel.Api.Ctu.Database;
using Christofel.Application;
using Christofel.ReactHandler.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Christofel.Design
{
    public class ReactHandlerContextDesignFactory : IDesignTimeDbContextFactory<ReactHandlerContext>
    {
        public ReactHandlerContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<ReactHandlerContext> builder = new DbContextOptionsBuilder<ReactHandlerContext>();
            IConfiguration configuration = ChristofelApp.CreateConfiguration(args);
            string connectionString = configuration.GetConnectionString("ReactHandler");
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            
            return new ReactHandlerContext(builder.Options);
        }
    }
}