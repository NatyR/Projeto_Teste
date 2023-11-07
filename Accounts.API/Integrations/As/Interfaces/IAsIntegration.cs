using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System;
using Accounts.API.Common.Services;
using Accounts.API.Integrations.BulllaEmpresa.Interfaces;
using Accounts.API.Common.Helpers.Interfaces;
using Accounts.API.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Accounts.API.Common.Repositories.Interfaces;
using Accounts.API.Common.Enum.Request;
using Accounts.API.Common.Dto.Request;
using Accounts.API.Common.Middlewares.Exceptions;
using System.Linq;
using Accounts.API.Integrations.BulllaEmpresa.Responses;
using Accounts.API.Common.Extensions.Request;
using Accounts.API.Integrations.BulllaEmpresa.Request;
using Accounts.API.Integrations.BulllaEmpresa;
using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
using Accounts.API.Common.Dto.Account;

namespace Accounts.API.Integrations.As
{
    public interface IAsIntegration
    {
        public string DesbloqueioDeSecuranca(string codigoConvenio, int IdUsuario, AccountModel listaContas);
        public dynamic ContasDesligadas(string codigoConvenio, int IdUsuario, string dataFiltro);
        public dynamic Faturas(string codigoConvenio, int IdUsuario, int idConta);
        public dynamic Contas(string codigoConvenio, int IdUsuario, string dataFiltro);
        public dynamic AbatimentoDivida(string codigoConvenio, int IdUsuario, AbatimentoDivida data);
        public dynamic FaturasResidual(string codigoConvenio, int IdUsuario, int idConta);
        public dynamic Cargas(string codigoConvenio, int IdUsuario, int idConta);
        public dynamic Cartoes(string codigoConvenio, int IdUsuario, string dataFiltro);
        public dynamic ReportDismissalDiscount(string userMail, string dataFiltro);
    }
}
