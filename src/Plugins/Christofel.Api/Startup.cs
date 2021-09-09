//
//   Startup.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Database;
using Christofel.Api.Ctu.Extensions;
using Christofel.Api.Ctu.JobQueue;
using Christofel.Api.Discord;
using Christofel.Api.GraphQL.Authentication;
using Christofel.Api.GraphQL.DataLoaders;
using Christofel.Api.GraphQL.Diagnostics;
using Christofel.Api.GraphQL.Types;
using Christofel.Api.OAuth;
using Christofel.Api.Services;
using Christofel.BaseLib.Configuration;
using Kos;
using Kos.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Usermap;
using Usermap.Extensions;

namespace Christofel.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddHostedService<RestoreAssignRolesService>();

            // cache database
            services
                .AddDbContextFactory<ApiCacheContext>
                (
                    options => options
                        .UseMySql
                        (
                            _configuration.GetConnectionString("ApiCache"),
                            ServerVersion.AutoDetect(_configuration.GetConnectionString("ApiCache"))
                        )
                );

            // bot options containing guild id
            services
                .Configure<BotOptions>(_configuration.GetSection("Bot"));

            // Ctu handlers for exchanging access token
            services
                .AddScoped<CtuOauthHandler>()
                .AddScoped<DiscordOauthHandler>()
                .Configure<CtuOauthOptions>("Ctu", _configuration.GetSection("Oauth:Ctu"))
                .Configure<OauthOptions>("Discord", _configuration.GetSection("Oauth:Discord"));

            // Apis used for auth
            services
                .AddScoped<DiscordApi>()
                .Configure<DiscordApiOptions>(_configuration.GetSection("Apis:Discord"))
                .Configure<UsermapApiOptions>(_configuration.GetSection("Apis:Usermap"))
                .AddScoped<IMemoryCache, MemoryCache>()
                .AddScopedUsermapApi
                (
                    p => p.GetRequiredService<ICtuTokenProvider>().AccessToken ??
                         throw new InvalidOperationException("No access token is provided for ctu services")
                )
                .AddScopedUsermapCaching()
                .AddScopedKosApi
                (
                    p =>
                        p.GetRequiredService<ICtuTokenProvider>().AccessToken ??
                        throw new InvalidOperationException("No access token is provided for ctu services")
                )
                .AddScopedKosCaching()
                .Configure<KosApiOptions>(_configuration.GetSection("Apis:Kos"));

            // processors of queues
            services
                .AddSingleton<IJobQueue<CtuAuthRoleAssign>, CtuAuthRoleAssignProcessor>()
                .AddSingleton<IJobQueue<CtuAuthNicknameSet>, CtuAuthNicknameSetProcessor>()
                .AddSingleton<CtuAuthRoleAssignService>();

            // add CTU authentication process along with all the steps
            services
                .AddCtuAuthProcess()
                .AddDefaultCtuAuthProcess();

            // GraphQL
            services
                .AddGraphQLServer()
                .AddMutationType(d => d.Name("Mutation"))
                .AddTypeExtension<AuthenticationMutations>()
                .AddQueryType(d => d.Name("Query"))
                .AddType<DbUserType>()
                .AddDataLoader<UserByIdDataLoader>()
                .AddDiagnosticEventListener
                (
                    sp =>
                        new DiagnosticEventListener(sp.GetApplicationService<ILogger<DiagnosticEventListener>>())
                )
                .EnableRelaySupport()
                .ModifyRequestOptions(x => x.ExecutionTimeout = TimeSpan.FromMinutes(2));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints
            (
                endpoints =>
                {
                    endpoints.MapGraphQL();
                }
            );
        }
    }
}