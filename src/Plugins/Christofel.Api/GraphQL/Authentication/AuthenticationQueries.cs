//
//   AuthenticationQueries.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.GraphQL.Attributes;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;

namespace Christofel.Api.GraphQL.Authentication;

/// <summary>
/// Queries for user registration.
/// </summary>
[ExtendObjectType("Query")]
public class AuthenticationQueries
{
    /// <summary>
    /// Verify specified registration code to know what stage
    /// of registration should be used (registerDiscord or registerCtu).
    /// </summary>
    /// <param name="input">Input of the mutation.</param>
    /// <param name="dbContext">The database context.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>Payload for the user.</returns>
    [UseReadOnlyChristofelBaseDatabase]
    public async Task<VerifyRegistrationCodePayload> VerifyRegistrationCodeAsync
    (
        VerifyRegistrationCodeInput input,
        [ScopedService] IReadableDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        var user = await dbContext.Set<DbUser>()
            .Where(x => x.AuthenticatedAt == null)
            .FirstOrDefaultAsync(x => x.RegistrationCode == input.RegistrationCode, cancellationToken);

        RegistrationCodeVerification verificationStage;
        if (user == null || string.IsNullOrEmpty(input.RegistrationCode))
        {
            verificationStage = RegistrationCodeVerification.NotValid;
        }
        else if (user.AuthenticatedAt != null)
        {
            verificationStage = RegistrationCodeVerification.Done;
        }
        else if (user.CtuUsername != null)
        {
            verificationStage = RegistrationCodeVerification.CtuAuthorized;
        }
        else
        {
            verificationStage = RegistrationCodeVerification.DiscordAuthorized;
        }

        return new VerifyRegistrationCodePayload(verificationStage);
    }
}