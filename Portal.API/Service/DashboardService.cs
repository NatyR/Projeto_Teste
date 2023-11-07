using Portal.API.Entities;
using Portal.API.Interfaces.Repositories;
using Portal.API.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Service
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<DashboardAccount> GetAccounts(int idConvenio, int? idGrupo = -1)
        {
            return await _dashboardRepository.GetAccounts(idConvenio, idGrupo);
        }

        public async Task<List<DashboardAccountUsage>> GetAccountUsage(int idConvenio, int? idGrupo = -1)
        {
            return await _dashboardRepository.GetAccountUsage(idConvenio, idGrupo);
        }

        public async Task<List<DashboardCity>> GetCities(int idConvenio, int? idGrupo = -1)
        {
            return await _dashboardRepository.GetCities(idConvenio);
        }

        public async Task<DashboardDemographic> GetDemographic(int idConvenio, int? idGrupo = -1)
        {
            return await _dashboardRepository.GetDemographic(idConvenio);
        }

        public async Task<List<DashboardMcc>> GetMcc(int idConvenio, int? idGrupo = -1)
        {
            return await _dashboardRepository.GetMcc(idConvenio);
        }

        public async Task<List<DashboardIssueCancellations>> GetIssueCancellations(int idConvenio, int? idGrupo = -1)
        {
            return await _dashboardRepository.GetIssueCancellations(idConvenio, idGrupo);
        }


        public async Task<List<DashboardContasCanceladas>> GetContasCanceladas(int idConvenio, int? idGrupo = -1)
        {
            return await _dashboardRepository.GetContasCanceladas(idConvenio, idGrupo);
        }
        public async Task<List<DashboardContasNovas>> GetContasNovas(int idConvenio, int? idGrupo = -1)
        {
            return await _dashboardRepository.GetContasNovas(idConvenio, idGrupo);
        }
    }
}
