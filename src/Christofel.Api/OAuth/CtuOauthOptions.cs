namespace Christofel.Api.OAuth
{
    /// <summary>
    /// Options for ctu oauth handler
    /// </summary>
    public class CtuOauthOptions : OauthOptions
    {
        /// <summary>
        /// Endpoint to obtain ctu username at with valid token
        /// </summary>
        public string? CheckTokenEndpoint { get; set; }
    }
}