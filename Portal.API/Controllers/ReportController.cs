using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Common.Dto;
using Portal.API.Dto.Banner;
using Portal.API.Dto.Report;
using Portal.API.Interfaces.Services;
using Portal.API.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Portal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("cartoes")]
        [Authorize]
        [ProducesResponseType(typeof(List<ReportCartaoDto>), 200)]
        public async Task<ActionResult<ResponseDto<ReportCartaoDto>>> GetCartaoes([FromQuery] long convenioId, [FromQuery] long grupoId, [FromQuery] int limit, [FromQuery] int skip, [FromQuery] string search, [FromQuery] string order)
        {
            var response = await _reportService.GetCartoes(convenioId, grupoId, limit, skip, search, order);
            return Ok(response);
        }
            
        [HttpGet("cartoes/exportcsv")]
        [Authorize]
        [ProducesResponseType(typeof(List<ReportCartaoDto>), 200)]
        public async Task<ActionResult<ResponseDto<ReportCartaoDto>>> GetCartoesExportCSV([FromQuery] long convenioId, [FromQuery] long grupoId, [FromQuery] int skip)
        {
            var response = await _reportService.GetCartoesExportCSV(convenioId, grupoId, skip);
            return Ok(response);
        }  
        [HttpGet("cartoes/exportcsv/total")]
        [Authorize]
        [ProducesResponseType(typeof(List<ReportCartaoDto>), 200)]
        public async Task<ActionResult<ResponseDto<ReportCartaoDto>>> GetCartoesExportCSVTotalPages([FromQuery] long convenioId, [FromQuery] long grupoId)
        {
            var response = await _reportService.GetCartoesExportCSVTotalPages(convenioId, grupoId);
            return Ok(response);
        }

        [HttpGet("contas")]
        [Authorize]
        [ProducesResponseType(typeof(List<ReportContasDto>), 200)]
        public async Task<ActionResult<ResponseDto<ReportContasDto>>> GetContas([FromQuery] long convenioId, [FromQuery] long grupoId, [FromQuery] int limit, [FromQuery] int skip, [FromQuery] string search, [FromQuery] string order, [FromQuery] DateTime ?dataFrom, [FromQuery] DateTime ?dataTo, [FromQuery] string? cardsFilter)
        {
          
            
            
            
            var response = await _reportService.GetContas(convenioId, grupoId, limit, skip, search, order, dataFrom, dataTo, null);
            return Ok(response);
        }

        [HttpGet("contas/exportcsv")]
        [Authorize]
        [ProducesResponseType(typeof(List<ReportContasDto>), 200)]
        public async Task<ActionResult<ResponseDto<ReportContasDto>>> GetContasExportCSV([FromQuery] long convenioId, [FromQuery] long grupoId, [FromQuery] int skip)
        {
            var response = await _reportService.GetContasExportCSV(convenioId, grupoId, skip);
            return Ok(response);
        }
        [HttpGet("contas/exportcsv/total")]
        [Authorize]
        [ProducesResponseType(typeof(List<ReportContasDto>), 200)]
        public async Task<ActionResult<ResponseDto<ReportContasDto>>> GetContasExportCSVTotalPages([FromQuery] long convenioId, [FromQuery] long grupoId)
        {
            var response = await _reportService.GetContasExportCSVTotalPages(convenioId, grupoId);
            return Ok(response);
        }

        [HttpPost("painel-gerencial")]
        [Authorize]
        [ProducesResponseType(typeof(List<ReportContasDto>), 200)]
        public async Task<ActionResult<ResponseDto<PainelGerencialDto>>> GetPainelGerencial([FromBody] PainelGerencialFilterDto filterOptions, [FromQuery] int limit, [FromQuery] int skip, [FromQuery] string order)
        {
            var response = await _reportService.GetPainelGerencialPaged(filterOptions, limit, skip, order);
            return Ok(response);
        }
        [HttpPost("painel-gerencial-csv")]
        [Authorize]
        [ProducesResponseType(typeof(List<ReportContasDto>), 200)]
        public async Task<IActionResult> GetPainelGerencialCsv([FromBody] PainelGerencialFilterDto filterOptions)
        {
            var res = await _reportService.GetPainelGerencialAll(filterOptions);
            if (res == null)
            {
                return NotFound();
            }
            var csv = new StringBuilder();
            var enc = new UTF8Encoding(true);
            var header = new string[] { };
            header = new string[] { "Grupo", "Convênio", "Nome do Convênio", "CNPJ do Convênio", "Usuários Cadastrados", "Alterações de Limite", "Alterações de Limite Global", "Solicitações de Cartão", "Solicitações de 2ª via", "Solicitações de Desligamento", "Solicitações de Desbloqueio", "Solicitações Totais" };
            csv.AppendLine(String.Join(';', header));
            foreach (var row in res)
            {
                var line = new string[]
                {
                    row.groupId.ToString(),
                    row.shopId.ToString(),
                    row.shopName,
                    row.shopDocument,
                    row.userCount.ToString(),
                    row.changeLimitCount.ToString(),
                    row.globalLimitCount.ToString(),
                    row.cardRequestCount.ToString(),
                    row.cardReissueCount.ToString(),
                    row.dismissalCount.ToString(),
                    row.unblockCount.ToString(),
                    row.totalRequestsCount.ToString()
                };
                csv.AppendLine(String.Join(';', line));
            }
            byte[] preamble = enc.GetPreamble();
            byte[] byteArray = enc.GetBytes(csv.ToString());
            MemoryStream stream = new MemoryStream(Combine(preamble, byteArray));
            stream.Position = 0;

            return File(stream, "application/octet-stream", "solicitacoes_painel_gerencial.csv");
        }
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }
        

    }
}
