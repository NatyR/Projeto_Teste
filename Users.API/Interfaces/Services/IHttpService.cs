using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users.API.Entities;

namespace Users.API.Interfaces.Services
{
    public interface IHttpService
    {
        public string GetRequestIP(bool tryUseXForwardHeader = true);
        public string GetUserAgent();
        public T GetHeaderValueAs<T>(string headerName); 
    }
}
