using Newtonsoft.Json;
using System;

namespace Portal.API.Dto.Notification
{
    public class NotificationUpdateDto
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("userId")]
        public long UserId { get; set; }
        [JsonProperty("readAt")]
        public DateTime? ReadAt { get; set; }
        [JsonProperty("createdAt")]
        public DateTime? CreatedAt { get; set; }
    }
}
