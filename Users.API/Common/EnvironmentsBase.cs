using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Users.API.Common
{
    public class EnvironmentsBase
    {
        public EnvironmentsBase(IConfiguration _configuration)
        {
            MQ_CONNECTIONSTRING = _configuration.GetSection("PortalRHConfig")["MQ_RABBIT:MQ_CONNECTIONSTRING"];
            MQ_USERNAME = _configuration.GetSection("PortalRHConfig")["MQ_RABBIT:MQ_USERNAME"];
            MQ_PASSWORD = _configuration.GetSection("PortalRHConfig")["MQ_RABBIT:MQ_PASSWORD"];

            AWS_MQ_CONNECTIONSTRING = _configuration.GetSection("PortalRHConfig")["AWS_MQ_RABBIT:MQ_CONNECTIONSTRING"];
            AWS_MQ_USERNAME = _configuration.GetSection("PortalRHConfig")["AWS_MQ_RABBIT:MQ_USERNAME"];
            AWS_MQ_PASSWORD = _configuration.GetSection("PortalRHConfig")["AWS_MQ_RABBIT:MQ_PASSWORD"];

        }

        public string MQ_CONNECTIONSTRING { get; set; }
        public string MQ_USERNAME { get; set; }
        public string MQ_PASSWORD { get; set; }
        public string AWS_MQ_CONNECTIONSTRING { get; set; }
        public string AWS_MQ_USERNAME { get; set; }
        public string AWS_MQ_PASSWORD { get; set; }
    }
}
