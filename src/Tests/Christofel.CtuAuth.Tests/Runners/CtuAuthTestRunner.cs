//
//   CtuAuthTestRunner.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Common.Database;
using Christofel.CtuAuth.Auth.Tasks;
using Christofel.CtuAuth.Auth.Tasks.Options;
using Christofel.CtuAuth.Extensions;
using Christofel.CtuAuth.Tests.Data;
using Christofel.CtuAuth.Tests.Data.Ctu.Auth;
using Christofel.CtuAuth.Tests.Data.Ctu.Auth.KosSource;
using Christofel.Helpers.ReadOnlyDatabase;
using Kos.Abstractions;
using Kos.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Remora.Results;
using Usermap.Controllers;

namespace Christofel.CtuAuth.Tests.Runners;

/// <summary>
/// This runs common tests for testing CtuAuth steps.
/// </summary>
public class CtuAuthTestRunner
{
    /// <summary>
    /// Build kos sources, services, common ctu auth flow, and test
    /// the steps.
    /// </summary>
    /// <param name="username">The username to authenticate.</param>
    /// <param name="config">Configurer for the ctu source builder, adding users.</param>
    /// <param name="assignedRoles">The roles that should be assigned to the user, based on what was built from previous argument.</param>
    /// <returns>The asynchronous task representing the operation of this function.</returns>
    public static async Task BuildAndTestSteps
    (
        string username,
        Action<ICtuSourceBuilder> config,
        RoleAssignmentInfo assignedRoles
    )
    {
        var builder = new CtuSourceBuilder();
        config(builder);
        var kosApiSource = builder.BuildKosApiSource();
        var usermapApiSource = builder.BuildUsermapApiSource();

        var dbOptions = SqliteInMemory.CreateOptions<ChristofelBaseContext>();
        dbOptions.PreventDispose();

        var dbContext = new ChristofelBaseContext(dbOptions);
        dbContext.Database.EnsureCreated();

        var roleAssignmentRepository = new RoleAssignmentRepository();
        await roleAssignmentRepository.FillDatabase(dbContext);

        // TODO: add roles to the guild member! Then soft remove roles can be verified
        //       this should probably be another argument to this function
        var serviceCollection = new ServiceCollection()
            .AddLogging(b => b.ClearProviders().AddConsole())

            // CtuAuth
            .AddCtuAuthProcess()
            .AddDefaultCtuAuthProcess()

            // Configs
            .Configure<AuthOptions>(o => o.FacultyCode = "13000")

            // Database
            .AddTransient(p => p.GetRequiredService<IDbContextFactory<ChristofelBaseContext>>().CreateDbContext())
            .AddSingleton<IDbContextFactory<ChristofelBaseContext>, ChristofelBaseContextFactory>
                (p => new ChristofelBaseContextFactory(dbOptions))
            .AddSingleton<ReadonlyDbContextFactory<ChristofelBaseContext>>()

            // Apis
            .AddSingleton<IKosAtomApi>(_ => new TestKosAtomApi(kosApiSource))
            .AddSingleton<IUsermapPeopleApi>(_ => new TestUsermapPeopleApi(usermapApiSource))
            .AddScoped<IKosPeopleApi, KosPeopleApi>()
            .AddScoped<IKosProgrammesApi, KosProgrammesApi>()
            .AddScoped<IKosStudentsApi, KosStudentsApi>()
            .AddScoped<IKosTeachersApi, KosTeachersApi>()
            .AddScoped<IKosDivisionsApi, KosDivisionsApi>();

        var toRemove = new List<ServiceDescriptor>();
        foreach (var service in serviceCollection)
        {
            if (service.ServiceType.IsAssignableFrom(typeof(IAuthTask)))
            {
                toRemove.Add(service);
            }
        }

        var task = new Mock<TaskRepository.MockTask>();
        CtuAuthAssignedRoles? capturedRoles = null;
        task.Setup(task => task.ExecuteAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()))
            .Callback((IAuthData data, CancellationToken ct) => capturedRoles = data.Roles)
            .ReturnsAsync(Result.FromSuccess());

        foreach (var remove in toRemove)
        {
            serviceCollection.Remove(remove);
        }

        serviceCollection
            .AddTransient<IAuthTask>(_ => task.Object);

        var services = serviceCollection.BuildServiceProvider();

        var ctuAuth = services.GetRequiredService<CtuAuthProcess>();
        var user = await dbContext.SetupUserToAuthenticateAsync(username, 1);
        var guildMember = GuildMemberRepository.CreateDummyGuildMember(user);

        var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, username);
        var result = await ctuAuth.FinishAuthAsync
        (
            "token",
            successfulOauthHandler.Object,
            dbContext,
            1,
            user,
            guildMember
        );

        Console.WriteLine(result.Error);
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedRoles);
        var expectedRoles = roleAssignmentRepository.ComputeRolesFor(assignedRoles).ToList().Select(x => x.ToString());
        var actualRoles = capturedRoles.AddRoles.Select(x => x.RoleId).Select(x => x.ToString());

        Assert.Equivalent(expectedRoles, actualRoles);

        // Assert.Equivalent(new int[] { 0 }, new int[] { 20, 21 });

        dbOptions.ManualDispose();
    }
}
