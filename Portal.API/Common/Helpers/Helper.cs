using Portal.API.Common.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Portal.API.Common.Helpers
{

    public class Helper : IHelper
    {
        /// <summary>
        /// Método responsável por retornar ip address
        /// </summary>
        /// <returns><see cref="string"/> - IP atual</returns>
        public string GetLocalIpAddress()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).First().ToString();
        }

        /// <summary>
        /// Método responsável por retornar uma porta de rede aleatória
        /// </summary>
        /// <returns><see cref="int"/> - Porta</returns>
        public int GenerateRandomPort()
        {
            return new Random(Guid.NewGuid().GetHashCode()).Next(16000, 17000);
        }

        /// <summary>
        /// Método responsável por resgatar serviço atual
        /// </summary>
        /// <returns></returns>
        public string GetServiceName()
        {
            var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
            var serviceName = assemblyName?.Split(".").Last();
            return serviceName;
        }

        /// <summary>
        /// Método responsável por resgatar versão do assembly (ex: 2.2)
        /// </summary>
        /// <returns><see cref="string"/> - Versão do assembly</returns>
        public string GetAssemblyVersion()
        {
            return Assembly.GetEntryAssembly()?.GetName().Version.ToString();
        }

        public List<string> GetAll()
        {
            return Assembly.GetEntryAssembly()?.GetReferencedAssemblies().Select(s => $"{s.Name} - {s.Version}").OrderBy(o => o).ToList();
        }

        /// <summary>
        /// Método responsável por resgatar nome do assembly (ex: netcore)
        /// </summary>
        /// <returns><see cref="string"/> - Nome do assembly</returns>
        public string GetAssemblyName()
        {
            return Assembly.GetEntryAssembly()?.GetName().Name;
        }
    }
}
