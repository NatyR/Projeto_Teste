using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;


namespace Accounts.API.Common.Dto.Account
{
    public class AccountSelectedDto
    {
        [JsonProperty("id")]
        public long? Id { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("cpf")]
        public string Cpf { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }
        [JsonProperty("registrationNumber")]
        public string RegistrationNumber { get; set; }
        [JsonProperty("reason")]
        public string? Reason { get; set; }
        [JsonProperty("reasonDescription")]
        public string? ReasonDescription { get; set; }
        [JsonProperty("cardLimit")]
        public decimal? CardLimit { get; set; }
        [JsonProperty("newCardLimit")]
        public decimal? NewCardLimit { get; set; }
        [JsonProperty("file")]
        public IFormFile File { get; set; }


    }
}
