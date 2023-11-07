using Portal.API.Dto.Report;
using Portal.API.Entities;
using Portal.API.Common.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Portal.API.Interfaces.Repositories
{
    public interface IReportRepository
    {
        Task<ResponseDto<ReportCartao>> GetCartoes(long convenioId, long grupoId, int limit, int skip, string search, string order);
        Task<long> GetCartoesExportCSVTotalPages(long convenioId, long grupoId);
        Task<List<ReportCartao>> GetCartoesExportCSV(long convenioId, long grupoId,int skip);

        Task<ResponseDto<ReportContas>> GetContas(long convenioId, long grupoId, int limit, int skip, string search, string order, DateTime ? from, DateTime ? to, string? cardsFilter);
        Task<long> GetContasExportCSVTotalPages(long convenioId, long grupoId);
        Task<List<ReportContas>> GetContasExportCSV(long convenioId, long grupoId, int skip);
        Task<ResponseDto<PainelGerencial>> GetPainelGerencialPaged(PainelGerencialFilterDto filterOptions, int limit, int skip, string order);
        Task<List<PainelGerencial>> GetPainelGerencialAll(PainelGerencialFilterDto filterOptions);


    }
}
