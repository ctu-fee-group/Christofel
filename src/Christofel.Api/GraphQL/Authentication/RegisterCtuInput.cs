namespace Christofel.Api.GraphQL.Authentication
{
    public record RegisterCtuInput(
        string OauthCode,
        string RegistrationCode
    );
}