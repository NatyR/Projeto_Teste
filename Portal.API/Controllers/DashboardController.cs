using Microsoft.AspNetCore.Mvc;
using Portal.API.Entities;
using Portal.API.Interfaces.Repositories;
using Portal.API.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Portal.API.Dto.Dashboard;

namespace Portal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        [HttpGet("cities/{idConvenio}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<List<DashboardCity>>> GetCidades(int idConvenio)
        {
            try
            {
                return Ok(await _dashboardService.GetCities(idConvenio));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("accounts/{idConvenio}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<DashboardAccount>> GetAccounts(int idConvenio)
        {
            try
            {
                var ppp = await _dashboardService.GetAccounts(idConvenio);
                return Ok(ppp);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpGet("account-usage/{idConvenio}")]
        //[ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<DashboardAccountUsage>> GetAccountUsage(int idConvenio)
        {
            try
            {
                var ppp = await _dashboardService.GetAccountUsage(idConvenio);
                var ppp2 = await _dashboardService.GetAccounts(idConvenio);
                return Ok(ppp2);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpGet("demographic/{idConvenio}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult> GetDemographic(int idConvenio)
        {
            try
            {
                return Ok(await _dashboardService.GetDemographic(idConvenio));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpGet("mcc/{idConvenio}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<List<DashboardMcc>>> GetMcc(int idConvenio)
        {
            try
            {
                return Ok(await _dashboardService.GetMcc(idConvenio));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }



        [HttpGet("getEmissoesCancelamentos/{idConvenio}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<List<EmissoesCancelamentosDto>>> getEmissoesCancelamentos(int idConvenio)
        {
            var resultCanceladas = await _dashboardService.GetContasCanceladas(idConvenio);
            var resultNovas = await _dashboardService.GetContasNovas(idConvenio);

            var mes1 = DateTime.Today.Month;
            var ano1 = DateTime.Today.Year;
            var mes1FiltradoCanceladas = resultCanceladas.FirstOrDefault(x => x.MES == mes1 && x.ANO == ano1);
            var mes1FiltradoNovas      = resultNovas.FirstOrDefault(x => x.MES == mes1 && x.ANO == ano1);

            var mes2 = DateTime.Today.AddMonths(-1).Month;
            var ano2 = DateTime.Today.AddMonths(-1).Year;
            var mes2FiltradoCanceladas = resultCanceladas.FirstOrDefault(x => x.MES == mes2 && x.ANO == ano2);
            var mes2FiltradoNovas      = resultNovas.FirstOrDefault(x => x.MES == mes2 && x.ANO == ano2);

            var mes3 = DateTime.Today.AddMonths(-2).Month;
            var ano3 = DateTime.Today.AddMonths(-2).Year;
            var mes3FiltradoCanceladas = resultCanceladas.FirstOrDefault(x => x.MES == mes3 && x.ANO == ano3);
            var mes3FiltradoNovas      = resultNovas.FirstOrDefault(x => x.MES == mes3 && x.ANO == ano3);

            var mes4 = DateTime.Today.AddMonths(-3).Month;
            var ano4 = DateTime.Today.AddMonths(-3).Year;
            var mes4FiltradoCanceladas = resultCanceladas.FirstOrDefault(x => x.MES == mes4 && x.ANO == ano4);
            var mes4FiltradoNovas      = resultNovas.FirstOrDefault(x => x.MES == mes4 && x.ANO == ano4);

            List<EmissoesCancelamentosDto> retorno = new List<EmissoesCancelamentosDto>();
            try
            {
                retorno.Add(new EmissoesCancelamentosDto() { Mes = DashboardUtil.ConvertMes(mes4), Ano = ano4.ToString(), QtdCancelamento = mes4FiltradoCanceladas != null ? mes4FiltradoCanceladas.CONTAS_CANCELADAS : 0, QtdEmissao = mes4FiltradoNovas != null ? mes4FiltradoNovas.CONTAS_NOVAS : 0 });
                retorno.Add(new EmissoesCancelamentosDto() { Mes = DashboardUtil.ConvertMes(mes3), Ano = ano3.ToString(), QtdCancelamento = mes3FiltradoCanceladas != null ? mes3FiltradoCanceladas.CONTAS_CANCELADAS : 0, QtdEmissao = mes3FiltradoNovas != null ? mes3FiltradoNovas.CONTAS_NOVAS : 0 });
                retorno.Add(new EmissoesCancelamentosDto() { Mes = DashboardUtil.ConvertMes(mes2), Ano = ano2.ToString(), QtdCancelamento = mes2FiltradoCanceladas != null ? mes2FiltradoCanceladas.CONTAS_CANCELADAS : 0, QtdEmissao = mes2FiltradoNovas != null ? mes2FiltradoNovas.CONTAS_NOVAS : 0 });
                retorno.Add(new EmissoesCancelamentosDto() { Mes = DashboardUtil.ConvertMes(mes1), Ano = ano1.ToString(), QtdCancelamento = mes1FiltradoCanceladas != null ? mes1FiltradoCanceladas.CONTAS_CANCELADAS : 0, QtdEmissao = mes1FiltradoNovas != null ? mes1FiltradoNovas.CONTAS_NOVAS : 0 });

                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("getVolumetriaContas/{idConvenio}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<VolumetriaContasDto>> getVolumetriaContas(int idConvenio)
        {
            var account = await _dashboardService.GetAccounts(idConvenio);

            VolumetriaContasDto retorno = new VolumetriaContasDto();
            retorno.Ativas = account != null ? account.ActiveAccounts : 0;
            retorno.Bloqueadas = account != null ? account.BlockedAccounts : 0;
            retorno.Canceladas = account != null ? account.CancelledAccounts : 0;
            retorno.Total = account != null ? account.TotalAccounts : 0;
            try
            {
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("getNovasContasMesVigente/{idConvenio}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<NovasContasMesVigenteDto>> getNovasContasMesVigente(int idConvenio)
        {
            var resultNovas = await _dashboardService.GetContasNovas(idConvenio);
            var mes1 = DateTime.Today.Month;
            var ano1 = DateTime.Today.Year;
            var mes1FiltradoNovas = resultNovas.FirstOrDefault(x => x.MES == mes1 && x.ANO == ano1);

            NovasContasMesVigenteDto retorno = new NovasContasMesVigenteDto();
            retorno.QtdNovasContas = mes1FiltradoNovas != null ? mes1FiltradoNovas.CONTAS_NOVAS : 0;
            try
            {
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("getBloqueadasMesVigente/{idConvenio}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<NovasContasMesVigenteDto>> getBloqueadasMesVigente(int idConvenio)
        {
            var account = await _dashboardService.GetAccounts(idConvenio);

            NovasContasMesVigenteDto retorno = new NovasContasMesVigenteDto();
            retorno.QtdNovasContas = account != null ? account.CurrentBlockedAccounts : 0;
            try
            {
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("getCancelamentoContasMesVigente/{idConvenio}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<CancelamentoContasMesVigenteDto>> getCancelamentoContasMesVigente(int idConvenio)
        {
            var resultCanceladas = await _dashboardService.GetContasCanceladas(idConvenio);
            var mes1 = DateTime.Today.Month;
            var ano1 = DateTime.Today.Year;
            var mes1FiltradoNovas = resultCanceladas.FirstOrDefault(x => x.MES == mes1 && x.ANO == ano1);

            CancelamentoContasMesVigenteDto retorno = new CancelamentoContasMesVigenteDto();
            retorno.QtdCancelamentContas = mes1FiltradoNovas != null ? mes1FiltradoNovas.CONTAS_CANCELADAS : 0;
            try
            {
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("getUtilizacaoCartoes/{idConvenio}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<List<UtilizacaoCartoesDto>>> getUtilizacaoCartoes(int idConvenio)
        {
            List<UtilizacaoCartoesDto> retorno = new List<UtilizacaoCartoesDto>();
            var lista = await _dashboardService.GetAccountUsage(idConvenio);

            var mes1 = DateTime.Today.Month;
            var ano1 = DateTime.Today.Year;
            var mes1Filtrado = lista.FirstOrDefault(x => x.MES == mes1 && x.ANO == ano1);

            var mes2 = DateTime.Today.AddMonths(-1).Month;
            var ano2 = DateTime.Today.AddMonths(-1).Year;
            var mes2Filtrado = lista.FirstOrDefault(x => x.MES == mes2 && x.ANO == ano2);

            var mes3 = DateTime.Today.AddMonths(-2).Month;
            var ano3 = DateTime.Today.AddMonths(-2).Year;
            var mes3Filtrado = lista.FirstOrDefault(x => x.MES == mes3 && x.ANO == ano3);

            var mes4 = DateTime.Today.AddMonths(-3).Month;
            var ano4 = DateTime.Today.AddMonths(-3).Year;
            var mes4Filtrado = lista.FirstOrDefault(x => x.MES == mes4 && x.ANO == ano4);

            retorno.Add(new UtilizacaoCartoesDto() { Mes = DashboardUtil.ConvertMes(mes4), Ano = ano4.ToString(), QtsUtilizacoes = mes4Filtrado != null ? mes4Filtrado.QUANTIDADE : 0 });
            retorno.Add(new UtilizacaoCartoesDto() { Mes = DashboardUtil.ConvertMes(mes3), Ano = ano3.ToString(), QtsUtilizacoes = mes3Filtrado != null ? mes3Filtrado.QUANTIDADE : 0 });
            retorno.Add(new UtilizacaoCartoesDto() { Mes = DashboardUtil.ConvertMes(mes2), Ano = ano2.ToString(), QtsUtilizacoes = mes2Filtrado != null ? mes2Filtrado.QUANTIDADE : 0 });
            retorno.Add(new UtilizacaoCartoesDto() { Mes = DashboardUtil.ConvertMes(mes1), Ano = ano1.ToString(), QtsUtilizacoes = mes1Filtrado != null ? mes1Filtrado.QUANTIDADE : 0 });


            try
            {
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("getUtilizacaoCompras/{idConvenio}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<List<UtilizacaoComprasDto>>> getUtilizacaoCompras(int idConvenio)
        {
            List<UtilizacaoComprasDto> retorno = new List<UtilizacaoComprasDto>();
            var lista = await _dashboardService.GetAccountUsage(idConvenio);

            var mes1 = DateTime.Today.Month;
            var ano1 = DateTime.Today.Year;
            var mes1Filtrado = lista.FirstOrDefault(x => x.MES == mes1 && x.ANO == ano1);

            var mes2 = DateTime.Today.AddMonths(-1).Month;
            var ano2 = DateTime.Today.AddMonths(-1).Year;
            var mes2Filtrado = lista.FirstOrDefault(x => x.MES == mes2 && x.ANO == ano2);

            var mes3 = DateTime.Today.AddMonths(-2).Month;
            var ano3 = DateTime.Today.AddMonths(-2).Year;
            var mes3Filtrado = lista.FirstOrDefault(x => x.MES == mes3 && x.ANO == ano3);

            var mes4 = DateTime.Today.AddMonths(-3).Month;
            var ano4 = DateTime.Today.AddMonths(-3).Year;
            var mes4Filtrado = lista.FirstOrDefault(x => x.MES == mes4 && x.ANO == ano4);

            retorno.Add(new UtilizacaoComprasDto() { Mes = DashboardUtil.ConvertMes(mes4), Ano = ano4.ToString(), ValorUtilizacoesMoeda = "R$ " + (mes4Filtrado != null ? mes4Filtrado.VALOR : 0).ToString("N2"), ValorUtilizacoes = (mes4Filtrado != null ? mes4Filtrado.VALOR : 0) });
            retorno.Add(new UtilizacaoComprasDto() { Mes = DashboardUtil.ConvertMes(mes3), Ano = ano3.ToString(), ValorUtilizacoesMoeda = "R$ " + (mes3Filtrado != null ? mes3Filtrado.VALOR : 0).ToString("N2"), ValorUtilizacoes = (mes3Filtrado != null ? mes3Filtrado.VALOR : 0) });
            retorno.Add(new UtilizacaoComprasDto() { Mes = DashboardUtil.ConvertMes(mes2), Ano = ano2.ToString(), ValorUtilizacoesMoeda = "R$ " + (mes2Filtrado != null ? mes2Filtrado.VALOR : 0).ToString("N2"), ValorUtilizacoes = (mes2Filtrado != null ? mes2Filtrado.VALOR : 0) });
            retorno.Add(new UtilizacaoComprasDto() { Mes = DashboardUtil.ConvertMes(mes1), Ano = ano1.ToString(), ValorUtilizacoesMoeda = "R$ " + (mes1Filtrado != null ? mes1Filtrado.VALOR : 0).ToString("N2"), ValorUtilizacoes = (mes1Filtrado != null ? mes1Filtrado.VALOR : 0) });

            try
            {
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}