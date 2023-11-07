using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Portal.API.Enum;

namespace Portal.API.Entities
{
    public class Notification
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public long UserId { get; set; }
        public DateTime? ReadAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public  long PreviousId { get; set; }
        public long NextId { get; set; }

        public NotificationTypeEnum NotificationType { get; set; }
        
    }
}
