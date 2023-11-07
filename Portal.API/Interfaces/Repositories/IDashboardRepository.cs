using Portal.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Repositories
{
    public interface IDashboardRepository
    {
        Task<List<DashboardAccountUsage>> GetAccountUsage(int idConvenio, int? idGrupo);
        Task<List<DashboardCity>> GetCities(int idConvenio);
        Task<DashboardAccount> GetAccounts(int idConvenio, int? idGrupo);
        Task<DashboardDemographic> GetDemographic(int idConvenio);
        Task<List<DashboardMcc>> GetMcc(int idConvenio);
        Task<List<DashboardIssueCancellations>> GetIssueCancellations(int idConvenio, int? idGrupo);

        Task<List<DashboardContasCanceladas>> GetContasCanceladas(int idConvenio, int? idGrupo);
        Task<List<DashboardContasNovas>> GetContasNovas(int idConvenio, int? idGrupo);



    }
}
