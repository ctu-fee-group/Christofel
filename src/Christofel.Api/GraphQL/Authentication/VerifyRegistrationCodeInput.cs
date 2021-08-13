namespace Christofel.Api.GraphQL.Authentication
{
    /// <summary>
    /// Input of verifyRegistration mutation
    /// </summary>
    /// <param name="RegistrationCode">Code to verify</param>
    public record VerifyRegistrationCodeInput(
        string RegistrationCode
    );
}