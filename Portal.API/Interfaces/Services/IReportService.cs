using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Common.Dto;
using Portal.API.Dto.Report;
using Portal.API.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Services
{
    public interface IReportService
    {
        Task<ResponseDto<ReportCartao>> GetCartoes(long convenioId, long grupoId, int limit, int skip, string search, string order);
        Task<long> GetCartoesExportCSVTotalPages(long convenioId, long grupoId);
        Task<IEnumerable<ReportCartao>> GetCartoesExportCSV(long convenioId, long grupoId, int skip);

        Task<ResponseDto<ReportContas>> GetContas(long convenioId, long grupoId, int limit, int skip, string search, string order, DateTime ? from, DateTime ? to, string? cardsFilter);
        Task<long> GetContasExportCSVTotalPages(long convenioId, long grupoId);
        Task<IEnumerable<ReportContas>> GetContasExportCSV(long convenioId, long grupoId, int skip);
        Task<ResponseDto<PainelGerencialDto>> GetPainelGerencialPaged(PainelGerencialFilterDto filterOptions, int limit, int skip, string order);
        Task<List<PainelGerencialDto>> GetPainelGerencialAll(PainelGerencialFilterDto filterOptions);
    }
}
