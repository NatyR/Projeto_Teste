using Newtonsoft.Json;
using Portal.API.Enum;
using System;

namespace Portal.API.Dto.Notification
{
    public class NotificationDto
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
        public DateTime CreatedAt { get; set; }
        [JsonProperty("nextId")]
        public long NextId { get; set; }
        [JsonProperty("previousId")]
        public long PreviousId { get; set; }
        public NotificationTypeEnum NotificationType { get; set; }


    }

}
