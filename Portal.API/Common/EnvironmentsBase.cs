using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Common
{
    public class EnvironmentsBase
    {
        public EnvironmentsBase(IConfiguration _configuration)
        {
            PloomesUserKey = _configuration.GetSection("PortalRHConfig")["Ploomes:UserKey"];
            PloomesUrlApi = _configuration.GetSection("PortalRHConfig")["Ploomes:UrlApi"];
            BulllaSaltToken = _configuration.GetSection("PortalRHConfig")["BulllaConfig:SaltToken"];
            BulllaIvToken = _configuration.GetSection("PortalRHConfig")["BulllaConfig:IvToken"];
            BulllaPassphrase = _configuration.GetSection("PortalRHConfig")["BulllaConfig:Passphrase"];
            BulllaLogin = _configuration.GetSection("PortalRHConfig")["BulllaConfig:Login"];
            BulllaPassword = _configuration.GetSection("PortalRHConfig")["BulllaConfig:Password"];
            BulllaAuthUrlApi = _configuration.GetSection("PortalRHConfig")["BulllaConfig:AuthUrlApi"];
            BulllaUrlApi = _configuration.GetSection("PortalRHConfig")["BulllaConfig:UrlApi"];

            MQ_CONNECTIONSTRING = _configuration.GetSection("PortalRHConfig")["MQ_RABBIT:MQ_CONNECTIONSTRING"];
            MQ_USERNAME = _configuration.GetSection("PortalRHConfig")["MQ_RABBIT:MQ_USERNAME"];
            MQ_PASSWORD = _configuration.GetSection("PortalRHConfig")["MQ_RABBIT:MQ_PASSWORD"];

            AWS_MQ_CONNECTIONSTRING = _configuration.GetSection("PortalRHConfig")["AWS_MQ_RABBIT:MQ_CONNECTIONSTRING"];
            AWS_MQ_USERNAME = _configuration.GetSection("PortalRHConfig")["AWS_MQ_RABBIT:MQ_USERNAME"];
            AWS_MQ_PASSWORD = _configuration.GetSection("PortalRHConfig")["AWS_MQ_RABBIT:MQ_PASSWORD"];


            AWSS3_ACCESS_KEY = _configuration.GetSection("PortalRHConfig")["AWSS3:ACCESS_KEY"];
            AWSS3_SECRET_KEY = _configuration.GetSection("PortalRHConfig")["AWSS3:SECRET_KEY"];
            AWSS3_BUCKET_NAME = _configuration.GetSection("PortalRHConfig")["AWSS3:BUCKET_NAME"];
        }
        public string PloomesUserKey { get; set; }
        public string PloomesUrlApi { get; set; }
        public string BulllaSaltToken { get; set; }
        public string BulllaIvToken { get; set; }
        public string BulllaPassphrase { get; set; }
        public string BulllaLogin { get; set; }
        public string BulllaPassword { get; set; }
        public string BulllaAuthUrlApi { get; set; }
        public string BulllaUrlApi { get; set; }

        public string MQ_CONNECTIONSTRING { get; set; }
        public string MQ_USERNAME { get; set; }
        public string MQ_PASSWORD { get; set; }

        public string AWS_MQ_CONNECTIONSTRING { get; set; }
        public string AWS_MQ_USERNAME { get; set; }
        public string AWS_MQ_PASSWORD { get; set; }

        public string AWSS3_ACCESS_KEY { get; set; }
        public string AWSS3_SECRET_KEY { get; set; }
        public string AWSS3_BUCKET_NAME { get; set; }
    }
}
