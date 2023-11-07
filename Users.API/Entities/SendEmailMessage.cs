using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Users.API.Entities
{
    public class SendEmailMessage
    {
        public string from { get; set; }
        public string to { get; set; }
        public string subject { get; set; }
        public string html { get; set; }
        public string text { get; set; }
    }
}
