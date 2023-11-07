using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Dto.Banner;
using Portal.API.Dto.Report;
using Portal.API.Interfaces.Repositories;
using Portal.API.Interfaces.Services;
using Portal.API.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Portal.API.Entities;
using Portal.API.Common.Dto;
using Amazon.S3.Model;

namespace Portal.API.Service
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IMapper _mapper;
        public ReportService(IReportRepository reportRepository, IMapper mapper)
        {
            _reportRepository = reportRepository;
            _mapper = mapper;
        }

        public async Task<ResponseDto<ReportCartao>> GetCartoes(long convenioId, long grupoId, int limit, int skip, string search, string order)
        => _mapper.Map<ResponseDto<ReportCartao>>(await _reportRepository.GetCartoes(convenioId, grupoId, limit, skip, search, order));
        public async Task<IEnumerable<ReportCartao>> GetCartoesExportCSV(long convenioId, long grupoId, int skip)
         => _mapper.Map<IEnumerable<ReportCartao>>(await _reportRepository.GetCartoesExportCSV(convenioId, grupoId, skip));
        public Task<long> GetCartoesExportCSVTotalPages(long convenioId, long grupoId)
            => _reportRepository.GetCartoesExportCSVTotalPages(convenioId, grupoId);

        public async Task<ResponseDto<ReportContas>> GetContas(long convenioId, long grupoId, int limit, int skip, string search, string order, DateTime ? from, DateTime ? to, string? cardsFilter)
        => _mapper.Map<ResponseDto<ReportContas>>(await _reportRepository.GetContas(convenioId, grupoId, limit, skip, search, order, from, to, cardsFilter));
        public async Task<IEnumerable<ReportContas>> GetContasExportCSV(long convenioId, long grupoId, int skip)
        => _mapper.Map<IEnumerable<ReportContas>>(await _reportRepository.GetContasExportCSV(convenioId, grupoId, skip));
        public Task<long> GetContasExportCSVTotalPages(long convenioId, long grupoId)
        => _reportRepository.GetContasExportCSVTotalPages(convenioId, grupoId);

        public async Task<ResponseDto<PainelGerencialDto>> GetPainelGerencialPaged(PainelGerencialFilterDto filterOptions, int limit, int skip, string order)
        => _mapper.Map<ResponseDto<PainelGerencialDto>>(await _reportRepository.GetPainelGerencialPaged(filterOptions, limit, skip, order));

        public async Task<List<PainelGerencialDto>> GetPainelGerencialAll(PainelGerencialFilterDto filterOptions)
        => _mapper.Map<List<PainelGerencialDto>>(await _reportRepository.GetPainelGerencialAll(filterOptions));

    }
}
