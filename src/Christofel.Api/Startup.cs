using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Extensions;
using Christofel.Api.Ctu.Steps;
using Christofel.Api.Ctu.Steps.Roles;
using Christofel.Api.Discord;
using Christofel.Api.GraphQL.Authentication;
using Christofel.Api.GraphQL.DataLoaders;
using Christofel.Api.GraphQL.Types;
using Christofel.Api.OAuth;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Extensions;
using Kos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Usermap;
using DiagnosticEventListener = Christofel.Api.GraphQL.Diagnostics.DiagnosticEventListener;

namespace Christofel.Api
{
    public record StartupData(
        IConfiguration Configuration
    );

    public class Startup
    {
        private readonly StartupData _data;

        public Startup(StartupData data)
        {
            _data = data;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = _data.Configuration;
            
            // bot options containing guild id
            services
                .Configure<BotOptions>(configuration.GetSection("Bot"));

            // Ctu handlers for exchanging access token
            services
                .AddScoped<CtuOauthHandler>()
                .AddScoped<DiscordOauthHandler>()
                .Configure<CtuOauthOptions>("Ctu", configuration.GetSection("Oauth:Ctu"))
                .Configure<OauthOptions>("Discord", configuration.GetSection("Oauth:Discord"));
            
            // Apis used for auth
            services
                .AddScoped<DiscordApi>()
                .Configure<DiscordApiOptions>(configuration.GetSection("Apis:Discord"))
                .AddScoped<UsermapApi>()
                .Configure<UsermapApiOptions>(configuration.GetSection("Apis:Usermap"))
                .AddScoped<KosApi>()
                .Configure<KosApiOptions>(configuration.GetSection("Apis:Kos"));

            // processors of queues
            services
                .AddSingleton<CtuAuthRoleAssignProcessor>();

            // add CTU authentication process along with all the steps
            services
                .AddCtuAuthProcess()
                .AddCtuAuthStep<
                    VerifyCtuUsernameStep>() // If ctu username is set and new auth user does not match, error
                .AddCtuAuthStep<VerifyDuplicityStep>() // Handle duplicate
                .AddCtuAuthStep<SpecificRolesStep>() // Add specific roles
                .AddCtuAuthStep<UsermapRolesStep>() // Add usermap roles
                .AddCtuAuthStep<TitlesRoleStep>() // Add roles based on title rules
                .AddCtuAuthStep<ProgrammeRoleStep>() // Obtain programme and its roles
                .AddCtuAuthStep<YearRoleStep>() // Obtain study start year and its roles
                .AddCtuAuthStep<
                    RemoveOldRolesStep>() // Remove all assigned roles by the auth process that weren't added by the previous steps
                .AddCtuAuthStep<AssignRolesStep>() // Assign roles to the user in queue
                .AddCtuAuthStep<FinishVerificationStep>();

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