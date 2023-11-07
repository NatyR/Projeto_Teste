using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Integrations.Ploomes.Dto
{
    public class PloomesContactDto
    {
        [JsonProperty("Id")]
        public long Id { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("LegalName")]
        public string LegalName { get; set; }
        [JsonProperty("Register")]
        public string Register { get; set; }
        [JsonProperty("CNPJ")]
        public string CNPJ { get; set; }
        [JsonProperty("CPF")]
        public string CPF { get; set; }
        [JsonProperty("Note")]
        public string Note { get; set; }
        [JsonProperty("StreetAddress")]
        public string StreetAddress { get; set; }
        [JsonProperty("StreetAddressLine2")]
        public string StreetAddressLine2 { get; set; }
        [JsonProperty("Neighborhood")]
        public string Neighborhood { get; set; }
        [JsonProperty("ZipCode")]
        public int? ZipCode { get; set; }
        [JsonProperty("Owner")]
        public PloomesOwnerDto Owner { get; set; }
    }
}
