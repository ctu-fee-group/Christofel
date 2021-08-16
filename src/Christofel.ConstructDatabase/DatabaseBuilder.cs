using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Christofel.BaseLib.Lifetime;
using Christofel.BaseLib.Plugins;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Christofel.ConstructDatabase
{
    public class DatabaseBuilder : IStartable
    {
        private readonly IDbContextFactory<ChristofelBaseContext> _dbContextFactory;
        private readonly BotOptions _options;
        private readonly DiscordRestClient _client;
        private readonly ICurrentPluginLifetime _lifetime;

        public DatabaseBuilder(
            IDbContextFactory<ChristofelBaseContext> dbContextFactory,
            IOptions<BotOptions> botOptions,
            DiscordRestClient client,
            ICurrentPluginLifetime lifetime
        )
        {
            _lifetime = lifetime;
            _options = botOptions.Value;
            _client = client;
            _dbContextFactory = dbContextFactory;
        }

        public Task StartAsync(CancellationToken token = new CancellationToken())
        {
            Task.Run(async () =>
            {
                using (ChristofelBaseContext context = _dbContextFactory.CreateDbContext())
                {
                    RestGuild guild = await _client.GetGuildAsync(_options.GuildId);
                    IReadOnlyCollection<IRole> roles = guild.Roles;

                    AddYearRoles(context, roles);
                    AddProgrammeRoles(context, roles);
                    AddSpecificRoles(context, roles);
                    AddUsermapRoles(context, roles);
                    AddTitleRoles(context, roles);

                    await context.SaveChangesAsync();
                }

                _lifetime.RequestStop();
            });

            return Task.CompletedTask;
        }

        private void AddTitleRoles(ChristofelBaseContext context, IReadOnlyCollection<IRole> roles)
        {
            var assignments = new Dictionary<string, TitleRoleAssignment>();
            assignments.Add("Magistr", new TitleRoleAssignment()
            {
                Title = "Mgr.",
                Pre = true,
                Post = false,
                Priority = 20
            });
            assignments.Add("Bakalar", new TitleRoleAssignment()
            {
                Title = "Bc.",
                Pre = true,
                Post = false,
                Priority = 10
            });

            foreach (var assignment in assignments)
            {
                var roleAssignment = new RoleAssignment()
                {
                    RoleId = roles.First(x => x.Name == assignment.Key).Id
                };

                assignment.Value.Assignment = roleAssignment;

                context.Add(roleAssignment);
                context.Add(assignment.Value);
            }
        }

        private void AddUsermapRoles(ChristofelBaseContext context, IReadOnlyCollection<IRole> roles)
        {
            var felAssignment = new RoleAssignment()
            {
                RoleId = roles.First(x => x.Name == "FEL student").Id,
                RoleType = RoleType.Faculty
            };
            var nonfelAssignment = new RoleAssignment()
            {
                RoleId = roles.First(x => x.Name == "NONFEL student").Id,
                RoleType = RoleType.Faculty
            };

            var bakalantAssignment = new RoleAssignment()
            {
                RoleId = roles.First(x => x.Name == "Bakalant").Id,
                RoleType = RoleType.CurrentStudies
            };
            var diplomantAssignment = new RoleAssignment()
            {
                RoleId = roles.First(x => x.Name == "Diplomant").Id,
                RoleType = RoleType.CurrentStudies
            };

            var bakalantUsermapAssignment = new UsermapRoleAssignment()
            {
                Assignment = bakalantAssignment,
                RegexMatch = false,
                UsermapRole = "B-00000-SUMA-STUDENT-BAKALAR"
            };

            var diplomantUsermapAssignment = new UsermapRoleAssignment()
            {
                Assignment = diplomantAssignment,
                RegexMatch = false,
                UsermapRole = "B-00000-SUMA-STUDENT-MAGISTR"
            };

            var felUsermapAssignment = new UsermapRoleAssignment()
            {
                Assignment = felAssignment,
                RegexMatch = false,
                UsermapRole = "B-13000-OSOBA-CVUT"
            };

            var nonFelUsermapAssignment = new UsermapRoleAssignment()
            {
                Assignment = nonfelAssignment,
                RegexMatch = true,
                UsermapRole = "^B-([2-9][0-9]|1[0-24-9]|0[1-9])000-OSOBA-CVUT$"
            };

            context.Add(nonfelAssignment);
            context.Add(felAssignment);

            context.Add(bakalantAssignment);
            context.Add(diplomantAssignment);

            context.Add(felUsermapAssignment);
            context.Add(nonFelUsermapAssignment);

            context.Add(bakalantUsermapAssignment);
            context.Add(diplomantUsermapAssignment);
        }

        private void AddYearRoles(ChristofelBaseContext context, IReadOnlyCollection<IRole> roles)
        {
            string prefix = "Year ";

            foreach (IRole role in roles.Where(x => x.Name.StartsWith(prefix)))
            {
                string yearString = role.Name.Substring(prefix.Length);
                int year = int.Parse(yearString);

                var roleAssignment = new RoleAssignment()
                {
                    RoleId = role.Id,
                    RoleType = RoleType.Year
                };

                var yearAssignment = new YearRoleAssignment()
                {
                    Year = year,
                    Assignment = roleAssignment
                };

                context.Add(roleAssignment);
                context.Add(yearAssignment);
            }
        }

        private void AddProgrammeRoles(ChristofelBaseContext context, IReadOnlyCollection<IRole> roles)
        {
            var programmes = new Dictionary<string, string>();
            programmes.Add("OES", "Otevřené elektronické systémy");
            programmes.Add("KyR", "Kybernetika a robotika");

            foreach (var programme in programmes)
            {
                var role = roles.FirstOrDefault(x => x.Name == programme.Key);
                if (role == null)
                {
                    continue;
                }

                var roleAssignment = new RoleAssignment()
                {
                    RoleId = role.Id,
                    RoleType = RoleType.Programme
                };

                var programmeAssignment = new ProgrammeRoleAssignment()
                {
                    Programme = programme.Value,
                    Assignment = roleAssignment
                };

                context.Add(roleAssignment);
                context.Add(programmeAssignment);
            }
        }

        private void AddSpecificRoles(ChristofelBaseContext context, IReadOnlyCollection<IRole> roles)
        {
            var authenticated = new RoleAssignment()
            {
                RoleId = roles.First(x => x.Name == "Authenticated").Id,
                RoleType = RoleType.General
            };

            var authenticatedSpecific = new SpecificRoleAssignment()
            {
                Name = "Authentication",
                Assignment = authenticated,
            };

            var teacher = new RoleAssignment()
            {
                RoleId = roles.First(x => x.Name == "Teacher").Id,
                RoleType = RoleType.General
            };

            var teacherSpecific = new SpecificRoleAssignment()
            {
                Name = "Teacher",
                Assignment = authenticated,
            };

            context.Add(teacher);
            context.Add(teacherSpecific);

            context.Add(authenticated);
            context.Add(authenticatedSpecific);
        }
    }
}