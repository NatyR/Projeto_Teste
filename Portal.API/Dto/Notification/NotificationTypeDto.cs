using Newtonsoft.Json;
using System;

namespace Portal.API.Dto.Notification
{
    public class NotificationTypeDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}