using Newtonsoft.Json;

namespace Usermap.Data
{
    public record UsermapDepartment(
        [JsonProperty("code")] int Code,
        [JsonProperty("nameCs")] string NameCzech,
        [JsonProperty("nameEn")] string NameEnglish
    );
}