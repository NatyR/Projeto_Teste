using Newtonsoft.Json;
using Portal.API.Enum;

namespace Portal.API.Dto.Notification
{
    public class NotificationAddDto
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("destination")]        
        public string Destination { get; set; }
        [JsonProperty("groupId")]
        public long? GroupId { get; set; }
        [JsonProperty("shopId")]
        public long? ShopId { get; set; }
        [JsonProperty("userId")]
        public long? UserId { get; set; }
        public NotificationTypeEnum? NotificationType { get; set; }
    }
}
