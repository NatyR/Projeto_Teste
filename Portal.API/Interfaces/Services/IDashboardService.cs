using Portal.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<List<DashboardCity>> GetCities(int idConvenio, int? idGrupo = null);
        Task<DashboardAccount> GetAccounts(int idConvenio, int? idGrupo = null);
        Task<List<DashboardAccountUsage>> GetAccountUsage(int idConvenio, int? idGrupo = null);
        Task<DashboardDemographic> GetDemographic(int idConvenio, int? idGrupo = null);
        Task<List<DashboardMcc>> GetMcc(int idConvenio, int? idGrupo = null);
        Task<List<DashboardIssueCancellations>> GetIssueCancellations(int idConvenio, int? idGrupo = null);

        Task<List<DashboardContasCanceladas>> GetContasCanceladas(int idConvenio, int? idGrupo = null);
        Task<List<DashboardContasNovas>> GetContasNovas(int idConvenio, int? idGrupo = null);


    }
}
