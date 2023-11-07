using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Common
{
    public class EnvironmentsBase
    {
        public EnvironmentsBase(IConfiguration _configuration)
        {
            BulllaEmpresaChannel = _configuration.GetSection("PortalRHConfig")["BulllaEmpresa:Channel"];
            BulllaEmpresaUsername = _configuration.GetSection("PortalRHConfig")["BulllaEmpresa:Username"];
            BulllaEmpresaPassword = _configuration.GetSection("PortalRHConfig")["BulllaEmpresa:Password"];
            BulllaEmpresaUrlApi = _configuration.GetSection("PortalRHConfig")["BulllaEmpresa:UrlApi"];
            BaseUrl = _configuration.GetSection("PortalRHConfig")["BaseUrl"];

            
            ASChannel = _configuration.GetSection("PortalRHConfig")["ASConfig:Channel"];
            ASUsername = _configuration.GetSection("PortalRHConfig")["ASConfig:Login"];
            ASPassword = _configuration.GetSection("PortalRHConfig")["ASConfig:Password"];
            ASUrlApi = _configuration.GetSection("PortalRHConfig")["ASConfig:UrlApi"];
            BulllaEmpresaCallbackUrl = _configuration.GetSection("PortalRHConfig")["BulllaEmpresa:CallbackUrl"];


            MQ_CONNECTIONSTRING = _configuration.GetSection("PortalRHConfig")["MQ_RABBIT:MQ_CONNECTIONSTRING"];
            MQ_USERNAME = _configuration.GetSection("PortalRHConfig")["MQ_RABBIT:MQ_USERNAME"];
            MQ_PASSWORD = _configuration.GetSection("PortalRHConfig")["MQ_RABBIT:MQ_PASSWORD"];

            AWS_MQ_CONNECTIONSTRING = _configuration.GetSection("PortalRHConfig")["AWS_MQ_RABBIT:MQ_CONNECTIONSTRING"];
            AWS_MQ_USERNAME = _configuration.GetSection("PortalRHConfig")["AWS_MQ_RABBIT:MQ_USERNAME"];
            AWS_MQ_PASSWORD = _configuration.GetSection("PortalRHConfig")["AWS_MQ_RABBIT:MQ_PASSWORD"];

            AWSS3_ACCESS_KEY = _configuration.GetSection("PortalRHConfig")["AWSS3:ACCESS_KEY"];
            AWSS3_SECRET_KEY = _configuration.GetSection("PortalRHConfig")["AWSS3:SECRET_KEY"];
            AWSS3_BUCKET_NAME = _configuration.GetSection("PortalRHConfig")["AWSS3:BUCKET_NAME"];

            AWSS3_BUCKET_NAME = _configuration.GetSection("PortalRHConfig")["AWSS3:BUCKET_NAME"];

            JOB_SEND_EMAIL_RESCISION = _configuration.GetSection("PortalRHConfig")["JobNotificationEmail"];

        }

        public string BulllaEmpresaChannel { get; set; }
        public string BulllaEmpresaUsername { get; set; }
        public string BulllaEmpresaPassword { get; set; }
        public string BulllaEmpresaUrlApi { get; set; }
        public string BulllaEmpresaCallbackUrl { get; set; }
        public string BaseUrl { get; set; }


        public string ASChannel { get; set; }
        public string ASUsername { get; set; }
        public string ASPassword { get; set; }
        public string ASUrlApi { get; set; }


        public string MQ_CONNECTIONSTRING { get; set; }
        public string MQ_USERNAME { get; set; }
        public string MQ_PASSWORD { get; set; }

        public string AWS_MQ_CONNECTIONSTRING { get; set; }
        public string AWS_MQ_USERNAME { get; set; }
        public string AWS_MQ_PASSWORD { get; set; }
        
        public string AWSS3_ACCESS_KEY { get; set; }
        public string AWSS3_SECRET_KEY { get; set; }
        public string AWSS3_BUCKET_NAME { get; set; }

        public string JOB_SEND_EMAIL_RESCISION { get; set; }


    }
}
