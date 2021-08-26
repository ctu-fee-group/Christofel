namespace Christofel.Api.GraphQL.Authentication
{
    /// <summary>
    /// Input for registerDiscord mutation.
    ///
    /// OauthCode is the code obtained from oauth2.
    /// Redirect uri is the one passed to oauth2.
    /// </summary>
    /// <param name="OauthCode">Code obtained from oauth2</param>
    /// <param name="RedirectUri">Redirect uri passed to oauth2</param>
    public record RegisterDiscordInput(
        string OauthCode,
        string RedirectUri
    );
}