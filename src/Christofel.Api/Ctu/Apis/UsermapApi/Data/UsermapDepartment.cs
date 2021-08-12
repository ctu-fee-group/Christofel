using Newtonsoft.Json;

namespace Christofel.Api.Ctu.Apis.UsermapApi.Data
{
    public record UsermapDepartment(
        [JsonProperty("code")] int Code,
        [JsonProperty("nameCs")] string NameCzech,
        [JsonProperty("nameEn")] string NameEnglish
    );
}