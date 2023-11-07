using System.Collections.Generic;

namespace Accounts.API.Common.Dto.Rabbit
{
    public class SendEmailMessageDto
    {
        public string from { get; set; }
        public string to { get; set; }
        public string subject { get; set; }
        public string html { get; set; }
        public string text { get; set; }

        public List<MessageAttachmentDto> attachments{ get; set; }
        
        public SendEmailMessageDto(){
            attachments = new List<MessageAttachmentDto>();
        }
    }
}
