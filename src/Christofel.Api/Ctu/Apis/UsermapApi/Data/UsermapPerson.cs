using System.Collections.Generic;
using Newtonsoft.Json;

namespace Christofel.Api.Ctu.Apis.UsermapApi.Data
{
    public record UsermapPerson
    (
        [JsonProperty("username")] string Username,
        [JsonProperty("personalNumber")] string PersonalNumber,
        [JsonProperty("firstName")] string FirstName,
        [JsonProperty("lastName")] string LastName,
        [JsonProperty("fullName")] string FullName,
        [JsonProperty("emails")] List<string> Emails, 
        [JsonProperty("preferredEmail")] string PreferredEmail, 
        [JsonProperty("departments")] List<UsermapDepartment> Departments, 
        [JsonProperty("rooms")] List<string> Rooms, 
        [JsonProperty("phones")] List<string> Phones, 
        [JsonProperty("roles")] List<string> Roles 
    );
}