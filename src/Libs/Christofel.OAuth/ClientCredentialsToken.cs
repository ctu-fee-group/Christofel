//
//   ClientCredentialsToken.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.OAuth;

/// <summary>
/// Store of client credentials.
/// </summary>
public class ClientCredentialsToken
{
    private readonly CtuOauthHandler _ctuOauthHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientCredentialsToken"/> class.
    /// </summary>
    /// <param name="ctuOauthHandler">The ctu oauth handler.</param>
    public ClientCredentialsToken(CtuOauthHandler ctuOauthHandler)
    {
        _ctuOauthHandler = ctuOauthHandler;
    }

    /// <summary>
    /// Gets or sets when the token was issued at.
    /// </summary>
    public DateTime IssuedAt { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets when the token expires.
    /// </summary>
    public DateTime ExpiresAt { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the client access token.
    /// </summary>
    public string? AccessToken { get; private set; }

    /// <summary>
    /// Makes sure the token is valid. If needed, the token is refreshed.
    /// </summary>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <exception cref="InvalidOperationException">Thrown if the token could not be obtained.</exception>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task MakeSureTokenValid(CancellationToken ct = default)
    {
        if (AccessToken is not null && ExpiresAt > DateTime.Now.AddMinutes(2))
        {
            return;
        }

        var response = await _ctuOauthHandler.GrantClientCredentialsAsync(ct);
        if (response.IsError || response.SuccessResponse is null)
        {
            throw new InvalidOperationException("Could not obtain client credentials:" + response.ErrorResponse);
        }

        IssuedAt = DateTime.Now;
        ExpiresAt = DateTime.Now.AddSeconds(response.SuccessResponse.ExpiresIn);
        AccessToken = response.SuccessResponse.AccessToken;
    }
}