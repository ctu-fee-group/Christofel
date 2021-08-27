using System;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Database;
using Christofel.Api.Ctu.Auth.Conditions;
using Christofel.Api.Ctu.Auth.Steps;
using Christofel.Api.Ctu.Auth.Tasks;
using Christofel.Api.Ctu.Extensions;
using Christofel.Api.Discord;
using Christofel.Api.GraphQL.Authentication;
using Christofel.Api.GraphQL.DataLoaders;
using Christofel.Api.GraphQL.Types;
using Christofel.Api.OAuth;
using Christofel.BaseLib.Configuration;
using Kos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Usermap;
using DiagnosticEventListener = Christofel.Api.GraphQL.Diagnostics.DiagnosticEventListener;

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
            // cache database
            services
                .AddPooledDbContextFactory<ApiCacheContext>(options => options
                    .UseMySql(
                        _configuration.GetConnectionString("ApiCache"),
                        ServerVersion.AutoDetect(_configuration.GetConnectionString("ApiCache")
                        )));

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
                .AddScoped<UsermapApi>()
                .Configure<UsermapApiOptions>(_configuration.GetSection("Apis:Usermap"))
                .AddScoped<KosApi>()
                .Configure<KosApiOptions>(_configuration.GetSection("Apis:Kos"));

            // processors of queues
            services
                .AddSingleton<CtuAuthRoleAssignProcessor>()
                .AddSingleton<CtuAuthRoleAssignService>();

            // scoped authorized apis
            services
                .AddScoped<ICtuTokenProvider, CtuTokenProvider>()
                .AddScoped<AuthorizedUsermapApi>(p =>
                {
                    var tokenProvider = p.GetRequiredService<ICtuTokenProvider>();
                    if (tokenProvider.AccessToken is null)
                    {
                        throw new InvalidOperationException("No access token is provided for ctu services");
                    }

                    return p.GetRequiredService<UsermapApi>().GetAuthorizedApi(tokenProvider.AccessToken);
                })
                .AddScoped<AuthorizedKosApi>(p =>
                {
                    var tokenProvider = p.GetRequiredService<ICtuTokenProvider>();
                    if (tokenProvider.AccessToken is null)
                    {
                        throw new InvalidOperationException("No access token is provided for ctu services");
                    }

                    return p.GetRequiredService<KosApi>().GetAuthorizedApi(tokenProvider.AccessToken);
                });
            
            // add CTU authentication process along with all the steps
            services
                .AddCtuAuthProcess()
                .AddAuthCondition<CtuUsernameFilledCondition>()
                .AddAuthCondition<MemberMatchesUserCondition>()
                .AddAuthCondition<NoDuplicateCondition>()
                .AddAuthCondition<CtuUsernameMatchesCondition>()
                .AddAuthStep<ProgrammeRoleStep>()
                .AddAuthStep<SetUserDataStep>()
                .AddAuthStep<SpecificRolesStep>()
                .AddAuthStep<TitlesRoleStep>()
                .AddAuthStep<UsermapRolesStep>()
                .AddAuthStep<YearRoleStep>()
                .AddAuthStep<DuplicateAssignStep>()
                .AddAuthStep<RemoveOldRolesStep>()
                .AddAuthTask<AssignRolesAuthTask>();

            // GraphQL
            services
                .AddGraphQLServer()
                .AddMutationType(d => d.Name("Mutation"))
                .AddTypeExtension<AuthenticationMutations>()
                .AddQueryType(d => d.Name("Query"))
                .AddType<DbUserType>()
                .AddDataLoader<UserByIdDataLoader>()
                .AddDiagnosticEventListener<DiagnosticEventListener>(sp =>
                    new DiagnosticEventListener(sp.GetApplicationService<ILogger<DiagnosticEventListener>>()))
                .EnableRelaySupport();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapGraphQL(); });
        }
    }
}
