using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Common.Helpers.Interfaces
{
    public interface IHelper
    {
        int GenerateRandomPort();
        string GetLocalIpAddress();
        string GetServiceName();
        string GetAssemblyName();
        string GetAssemblyVersion();
        List<string> GetAll();
    }
}
