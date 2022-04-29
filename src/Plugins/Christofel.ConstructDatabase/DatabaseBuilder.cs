//
//  DatabaseBuilder.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Christofel.Common.Database.Models.Enums;
using Christofel.Plugins.Lifetime;
using Christofel.Plugins.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;

namespace Christofel.ConstructDatabase
{
    /// <summary>
    /// The databse builder.
    /// </summary>
    public class DatabaseBuilder : IStartable
    {
        private readonly IDbContextFactory<ChristofelBaseContext> _dbContextFactory;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly BotOptions _options;
        private readonly ICurrentPluginLifetime _lifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseBuilder"/> class.
        /// </summary>
        /// <param name="dbContextFactory">The context factory.</param>
        /// <param name="botOptions">The bot options.</param>
        /// <param name="guildApi">The guild api.</param>
        /// <param name="lifetime">The lifetime.</param>
        public DatabaseBuilder(
            IDbContextFactory<ChristofelBaseContext> dbContextFactory,
            IOptions<BotOptions> botOptions,
            IDiscordRestGuildAPI guildApi,
            ICurrentPluginLifetime lifetime
        )
        {
            _lifetime = lifetime;
            _options = botOptions.Value;
            _dbContextFactory = dbContextFactory;
            _guildApi = guildApi;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken token = default)
        {
            Task.Run(async () =>
            {
                try
                {
                    await using (var context = _dbContextFactory.CreateDbContext())
                    {
                        var rolesResult = await _guildApi.GetGuildRolesAsync
                            (DiscordSnowflake.New(_options.GuildId), token);
                        if (!rolesResult.IsDefined(out var roles))
                        {
                            Console.WriteLine("Could not load guild roles.");
                            return;
                        }

                        AddYearRoles(context, roles);
                        AddProgrammeRoles(context, roles);
                        AddSpecificRoles(context, roles);
                        AddUsermapRoles(context, roles);
                        AddTitleRoles(context, roles);

                        await context.SaveChangesAsync(token);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                _lifetime.RequestStop();
            });

            return Task.CompletedTask;
        }

        private void AddTitleRoles(ChristofelBaseContext context, IReadOnlyList<IRole> roles)
        {
            var assignments = new Dictionary<string, TitleRoleAssignment>();
            assignments.Add("Magistr", new TitleRoleAssignment()
            {
                Title = "Mgr.",
                Pre = true,
                Post = false,
                Priority = 20
            });
            assignments.Add("Bakalář", new TitleRoleAssignment()
            {
                Title = "Bc.",
                Pre = true,
                Post = false,
                Priority = 10
            });
            assignments.Add("Inženýr", new TitleRoleAssignment()
            {
                Title = "Ing.",
                Pre = true,
                Post = false,
                Priority = 20
            });

            foreach (var assignment in assignments)
            {
                var roleAssignment = new RoleAssignment()
                {
                    RoleId = roles.First(x => x.Name == assignment.Key).ID
                };

                assignment.Value.Assignment = roleAssignment;

                context.Add(roleAssignment);
                context.Add(assignment.Value);
            }
        }

        private void AddUsermapRoles(ChristofelBaseContext context, IReadOnlyList<IRole> roles)
        {
            var felAssignment = new RoleAssignment()
            {
                RoleId = roles.First(x => x.Name == "FELák").ID,
                RoleType = RoleType.Faculty
            };
            var nonfelAssignment = new RoleAssignment()
            {
                RoleId = roles.First(x => x.Name == "ČVUT impostor").ID,
                RoleType = RoleType.Faculty
            };

            var bakalantAssignment = new RoleAssignment()
            {
                RoleId = roles.First(x => x.Name == "Bakalant").ID,
                RoleType = RoleType.CurrentStudies
            };
            var diplomantAssignment = new RoleAssignment()
            {
                RoleId = roles.First(x => x.Name == "Diplomant").ID,
                RoleType = RoleType.CurrentStudies
            };
            var doktorandAssignment = new RoleAssignment()
            {
                RoleId = roles.First(x => x.Name == "Doktorand").ID,
                RoleType = RoleType.CurrentStudies
            };

            var bakalantUsermapAssignment = new UsermapRoleAssignment()
            {
                Assignment = bakalantAssignment,
                RegexMatch = false,
                UsermapRole = "B-00000-SUMA-STUDENT-BAKALAR"
            };

            var doktorandUsermapAssignment = new UsermapRoleAssignment()
            {
                Assignment = doktorandAssignment,
                RegexMatch = false,
                UsermapRole = "B-00000-SUMA-STUDENT-DOKTORAND"
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
            context.Add(doktorandUsermapAssignment);
        }

        private void AddYearRoles(ChristofelBaseContext context, IReadOnlyList<IRole> roles)
        {
            string prefix = "Ročník ";

            foreach (IRole role in roles.Where(x => x.Name.StartsWith(prefix)))
            {
                string yearString = role.Name.Substring(prefix.Length);
                int year = int.Parse(yearString);

                var roleAssignment = new RoleAssignment()
                {
                    RoleId = role.ID,
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

        private void AddProgrammeRoles(ChristofelBaseContext context, IReadOnlyList<IRole> roles)
        {
            var programmes = new Dictionary<string, string>();
            programmes.Add("OES", "Otevřené elektronické systémy");
            programmes.Add("KYR", "Kybernetika a robotika");
            programmes.Add("OI", "Otevřená informatika");
            programmes.Add("EK", "Elektronika a komunikace");
            programmes.Add("EEK", "Elektrotechnika, elektronika a komunikační technika");
            programmes.Add("SIT", "Softwarové inženýrství a technologie");
            programmes.Add("EEM", "Elektrotechnika, energetika a management");
            programmes.Add("LEB", "Lékařská elektronika a bioinformatika");

            foreach (var programme in programmes)
            {
                var role = roles.FirstOrDefault(x => x.Name == programme.Key);
                if (role == null)
                {
                    continue;
                }

                var roleAssignment = new RoleAssignment()
                {
                    RoleId = role.ID,
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

        private void AddSpecificRoles(ChristofelBaseContext context, IReadOnlyList<IRole> roles)
        {
            var authenticated = new RoleAssignment()
            {
                RoleId = roles.First(x => x.Name == "Ověřený").ID,
                RoleType = RoleType.General
            };

            var authenticatedSpecific = new SpecificRoleAssignment()
            {
                Name = "Authentication",
                Assignment = authenticated,
            };

            var teacher = new RoleAssignment()
            {
                RoleId = roles.First(x => x.Name == "Vyučující").ID,
                RoleType = RoleType.General
            };

            var teacherSpecific = new SpecificRoleAssignment()
            {
                Name = "Teacher",
                Assignment = teacher,
            };

            context.Add(teacher);
            context.Add(teacherSpecific);

            context.Add(authenticated);
            context.Add(authenticatedSpecific);
        }
    }
}