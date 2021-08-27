namespace Christofel.Api.Ctu
{
    public interface ICtuTokenProvider
    {
        public string? AccessToken { get; set; }
    }

    public class CtuTokenProvider : ICtuTokenProvider
    {
        public string? AccessToken { get; set; }
    }
}