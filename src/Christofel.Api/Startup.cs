using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Christofel.Api.GraphQL.Authentication;
using Christofel.Api.GraphQL.DataLoaders;
using Christofel.Api.GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Christofel.Api
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddGraphQLServer()
                .AddMutationType(d => d.Name("Mutation"))
                    .AddTypeExtension<AuthenticationMutations>()
                .AddQueryType(d => d.Name("Query"))
                .AddType<DbUserType>()
                .AddDataLoader<UserByIdDataLoader>()
                .EnableRelaySupport();
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL();
            });
        }
    }
}