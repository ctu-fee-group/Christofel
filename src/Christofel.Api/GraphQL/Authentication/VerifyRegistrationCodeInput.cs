namespace Christofel.Api.GraphQL.Authentication
{
    public record VerifyRegistrationCodeInput(
        string RegistrationCode
    );
}