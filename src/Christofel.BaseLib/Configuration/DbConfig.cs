using System.Linq;
using System.Threading.Tasks;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Christofel.BaseLib.Configuration
{
    public sealed class DbConfig : ConvertersConfig, IWRConfig
    {
        private IDbContextFactory<ChristofelBaseContext> _dbContextFactory;
        
        public DbConfig(IDbContextFactory<ChristofelBaseContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
        
        public async Task SetAsync<T>(string name, T value)
        {
            await using ChristofelBaseContext dbContext = _dbContextFactory.CreateDbContext();
            
            ConfigurationEntry? entry = await GetNullableEntry(dbContext, name);

            if (entry == null)
            {
                entry = new ConfigurationEntry
                {
                    Name = name,
                    Value = value?.ToString() ?? "null",
                };
                
                await dbContext.Set<ConfigurationEntry>().AddAsync(entry);
            }
            else
            {
                entry.Value = name;
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task<T> GetAsync<T>(string name)
        {
            await using ChristofelBaseContext dbContext = _dbContextFactory.CreateDbContext();
            ConfigurationEntry? entry = await GetNullableEntry(dbContext, name);

            if (entry == null)
            {
                throw new ConfigValueNotFoundException(name);
            }

            return Convert<T>(entry.Value);
        }

        private async Task<ConfigurationEntry?> GetNullableEntry(ChristofelBaseContext dbContext, string name)
        {
            return (ConfigurationEntry?)(await dbContext.Set<ConfigurationEntry>()
                .AsQueryable().
                Where(x => x.Name == name)
                .FirstOrDefaultAsync());
        } 
    }
}