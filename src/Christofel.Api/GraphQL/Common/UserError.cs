namespace Christofel.Api.GraphQL.Common
{
    /// <summary>
    /// Validation error
    /// </summary>
    /// <param name="Message">What has gone wrong</param>
    public record UserError(
        string Message
    );
}