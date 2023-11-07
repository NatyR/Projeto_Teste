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
    public class IndicadoresController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public IndicadoresController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }


        [HttpGet("getEmissoesCancelamentos/{idGrupo}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<List<EmissoesCancelamentosDto>>> getEmissoesCancelamentos(int idGrupo)
        {
            var result = await _dashboardService.GetIssueCancellations(0, idGrupo);
            var mes1 = DateTime.Today.Month;
            var ano1 = DateTime.Today.Year;
            var mes1Filtrado = result.FirstOrDefault(x => x.MES == mes1 && x.ANO == ano1);

            var mes2 = DateTime.Today.AddMonths(-1).Month;
            var ano2 = DateTime.Today.AddMonths(-1).Year;
            var mes2Filtrado = result.FirstOrDefault(x => x.MES == mes2 && x.ANO == ano2);

            var mes3 = DateTime.Today.AddMonths(-2).Month;
            var ano3 = DateTime.Today.AddMonths(-2).Year;
            var mes3Filtrado = result.FirstOrDefault(x => x.MES == mes3 && x.ANO == ano3);

            var mes4 = DateTime.Today.AddMonths(-3).Month;
            var ano4 = DateTime.Today.AddMonths(-3).Year;
            var mes4Filtrado = result.FirstOrDefault(x => x.MES == mes4 && x.ANO == ano4);

            List<EmissoesCancelamentosDto> retorno = new List<EmissoesCancelamentosDto>();
            try
            {
                retorno.Add(new EmissoesCancelamentosDto() { Mes = DashboardUtil.ConvertMes(mes4), Ano = ano4.ToString(), QtdCancelamento = mes4Filtrado != null ? mes4Filtrado.CONTAS_CANCELADAS_MES : 0, QtdEmissao = mes4Filtrado != null ? mes4Filtrado.CONTAS_NOVAS_MES : 0 });
                retorno.Add(new EmissoesCancelamentosDto() { Mes = DashboardUtil.ConvertMes(mes3), Ano = ano3.ToString(), QtdCancelamento = mes3Filtrado != null ? mes3Filtrado.CONTAS_CANCELADAS_MES : 0, QtdEmissao = mes3Filtrado != null ? mes3Filtrado.CONTAS_NOVAS_MES : 0 });
                retorno.Add(new EmissoesCancelamentosDto() { Mes = DashboardUtil.ConvertMes(mes2), Ano = ano2.ToString(), QtdCancelamento = mes2Filtrado != null ? mes2Filtrado.CONTAS_CANCELADAS_MES : 0, QtdEmissao = mes2Filtrado != null ? mes2Filtrado.CONTAS_NOVAS_MES : 0 });
                retorno.Add(new EmissoesCancelamentosDto() { Mes = DashboardUtil.ConvertMes(mes1), Ano = ano1.ToString(), QtdCancelamento = mes1Filtrado != null ? mes1Filtrado.CONTAS_CANCELADAS_MES : 0, QtdEmissao = mes1Filtrado != null ? mes1Filtrado.CONTAS_NOVAS_MES : 0 });

                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("getVolumetriaContas/{idGrupo}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<VolumetriaContasDto>> getVolumetriaContas(int idGrupo)
        {
            var account = await _dashboardService.GetAccounts(0, idGrupo);

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
        [HttpGet("getNovasContasMesVigente/{idGrupo}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<NovasContasMesVigenteDto>> getNovasContasMesVigente(int idGrupo)
        {
            var account = await _dashboardService.GetAccounts(0, idGrupo);

            NovasContasMesVigenteDto retorno = new NovasContasMesVigenteDto();
            retorno.QtdNovasContas = account != null ? account.CurrentNewAccounts : 0;
            try
            {
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("getBloqueadasMesVigente/{idGrupo}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<NovasContasMesVigenteDto>> getBloqueadasMesVigente(int idGrupo)
        {
            var account = await _dashboardService.GetAccounts(0, idGrupo);

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
        [HttpGet("getCancelamentoContasMesVigente/{idGrupo}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<CancelamentoContasMesVigenteDto>> getCancelamentoContasMesVigente(int idGrupo)
        {
            var account = await _dashboardService.GetAccounts(0, idGrupo);

            CancelamentoContasMesVigenteDto retorno = new CancelamentoContasMesVigenteDto();
            retorno.QtdCancelamentContas = account != null ? account.CurrentCancelledAccounts : 0;
            try
            {
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("getUtilizacaoCartoes/{idGrupo}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<List<UtilizacaoCartoesDto>>> getUtilizacaoCartoes(int idGrupo)
        {
            List<UtilizacaoCartoesDto> retorno = new List<UtilizacaoCartoesDto>();
            var lista = await _dashboardService.GetAccountUsage(0, idGrupo);

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
        [HttpGet("getUtilizacaoCompras/{idGrupo}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<List<UtilizacaoComprasDto>>> getUtilizacaoCompras(int idGrupo)
        {
            List<UtilizacaoComprasDto> retorno = new List<UtilizacaoComprasDto>();
            var lista = await _dashboardService.GetAccountUsage(0, idGrupo);

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