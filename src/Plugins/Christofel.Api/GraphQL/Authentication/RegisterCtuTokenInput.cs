namespace Christofel.Api.GraphQL.Authentication
{
    /// <summary>
    /// Input for registerCtuToken mutation.
    ///
    /// Access token to kos api and usermap api
    /// Registration code is obtained from the first step of the registration (registerDiscord).
    /// </summary>
    /// <param name="AccessToken">Access token to kos api and usermap api</param>
    /// <param name="RegistrationCode">Code obtained from the first step of the registration (registerDiscord)</param>
    public record RegisterCtuTokenInput(
        string AccessToken,
        string RegistrationCode
    );
}