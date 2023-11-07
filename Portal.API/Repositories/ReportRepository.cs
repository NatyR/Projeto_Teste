using Dapper;
using Microsoft.Extensions.Configuration;
using Portal.API.Data;
using Portal.API.Entities;
using Portal.API.Common.Dto;
using Portal.API.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3.Model;
using System.Collections;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using Portal.API.Dto.Report;
using MongoDB.Driver.Core.Configuration;
using FakeItEasy;
using System.Drawing.Printing;

namespace Portal.API.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly IConfiguration _config;

        private readonly string portalConnectionString;
        public ReportRepository(IConfiguration config)
        {
            _config = config;
            portalConnectionString = _config.GetConnectionString("BIConnection");
        }

        public async Task<ResponseDto<ReportCartao>> GetCartoes(long convenioId, long grupoId, int limit, int skip, string search, string order)
        {
            string filter = "1 = 1";
            string orderBy = "CODIGO_CONVENIO ASC";
            if (!String.IsNullOrEmpty(search))
            {

                string[] formats = { "dd/mm/yyyy" };

                DateTime output;
                if (DateTime.TryParseExact(search, formats, new CultureInfo("pt-BR"), DateTimeStyles.None, out output))
                {
                    filter = $@"
                    (DATA_NASCIMENTO BETWEEN to_date('{search} 00:00:00', 'DD.MM.YYYY HH24:MI:SS') AND to_date('{search} 23:59:59', 'DD.MM.YYYY HH24:MI:SS')) OR
                    (DATA_EMISSAO_CARTAO BETWEEN to_date('{search} 00:00:00', 'DD.MM.YYYY HH24:MI:SS') AND to_date('{search} 23:59:59', 'DD.MM.YYYY HH24:MI:SS'))";
                }
                else
                {
                    search = search.ToLower();

                    filter = $@"(LOWER(NOME_PORTADOR) LIKE '%{search}%' OR 
                LOWER(CPF) LIKE '%{search.Replace(".", "").Replace("-", "")}%' OR
                LOWER(NOME_PORTADOR) LIKE '%{search}%' OR
                LOWER(MATRICULA) LIKE '%{search}%' OR
                LOWER(ID_CONTA) LIKE '%{search}%' OR
                LOWER(STATUS_CONTA) LIKE '%{search}%' OR
                LOWER(STATUS_CARTAO) LIKE '%{search}%' OR
                LOWER(ULT_DIGITOS_CARTAO) LIKE '%{search}%' OR
                LOWER(LIMITE_MENSAL) LIKE '%{search}%')";
                }
            }

            if (!String.IsNullOrEmpty(order))
            {
                var part = order.Split(' ');
                switch (part[0].ToLower())
                {
                    case "codigoconvenio": orderBy = "CODIGO_CONVENIO " + part[1]; break;
                    case "codgrupoconvenio": orderBy = "COD_GRUPO_CONVENIO " + part[1]; break;
                    case "cpf": orderBy = "CPF " + part[1]; break;
                    case "datanascimentoportador": orderBy = "DATA_NASCIMENTO " + part[1]; break;
                    case "nomeportador": orderBy = "NOME_PORTADOR " + part[1]; break;
                    case "matricula": orderBy = "MATRICULA " + part[1]; break;
                    case "idconta": orderBy = "ID_CONTA " + part[1]; break;
                    case "statusconta": orderBy = "STATUS_CONTA " + part[1]; break;
                    case "statuscartao": orderBy = "STATUS_CARTAO " + part[1]; break;
                    case "numerocartao": orderBy = "ULT_DIGITOS_CARTAO " + part[1]; break;
                    case "dataemissaocartao": orderBy = "DATA_EMISSAO_CARTAO " + part[1]; break;
                    case "limitemensal": orderBy = "LIMITE_MENSAL " + part[1]; break;

                }
            }

            string basequery = $@" 
                SELECT 
                              CODIGO_CONVENIO as CodigoConvenio
                            , COD_GRUPO_CONVENIO as CodGrupoConvenio
                            , CPF as Cpf
                            , DATA_NASCIMENTO AS DataNascimentoPortador
                            , NOME_PORTADOR AS NomePortador
                            , MATRICULA AS Matricula
                            , ID_CONTA AS IdConta
                            , STATUS_CONTA AS StatusConta
                            , STATUS_CARTAO AS StatusCartao
                            , ULT_DIGITOS_CARTAO AS NumeroCartao
                            , DATA_EMISSAO_CARTAO AS DataEmissaoCartao
                            , LIMITE_MENSAL AS LimiteMensal";

            string whereQuery = $@" FROM BI_UNIK.TB_CARTOES 

                WHERE CODIGO_CONVENIO = {convenioId} AND COD_GRUPO_CONVENIO = {grupoId} AND {filter} ORDER BY {orderBy}";

            string countquery = $@"SELECT count(*) AS COUNT {whereQuery}";
            string query = $"SELECT b.* FROM(SELECT a.*, rownum r__ FROM ( {basequery + whereQuery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";


            using var conn = new DbSession(_config.GetConnectionString("BIConnection")).Connection;
            var count = await conn.QueryFirstOrDefaultAsync<long>(countquery);

            var records = await conn.QueryAsync<ReportCartao>(query, new
            {
                pageNumber = (skip / limit) + 1,
                pageSize = limit
            });

            return new ResponseDto<ReportCartao>
            {
                CurrentPage = (skip / limit) + 1,
                Data = records.ToList(),
                PerPage = limit,
                Total = count
            };


        }

        public async Task<long> GetCartoesExportCSVTotalPages(long convenioId, long grupoId)
        {
            int limit = 10000;

            string countquery = $@"SELECT count(*) AS COUNT FROM BI_UNIK.TB_CARTOES  WHERE CODIGO_CONVENIO = {convenioId} AND COD_GRUPO_CONVENIO = {grupoId}  ";

            using var conn = new DbSession(_config.GetConnectionString("BIConnection")).Connection;
            var count = await conn.QueryFirstOrDefaultAsync<long>(countquery);

            long totalPages = count / limit + 1;

            return totalPages > 1 ? totalPages : 1;
        }

        public async Task<List<ReportCartao>> GetCartoesExportCSV(long convenioId, long grupoId, int skip)
        {

            int limit = 10000;

            string paginatequery = $@"
            SELECT *
                from (
                                SELECT      rownum as rn,
                                              CODIGO_CONVENIO as CodigoConvenio
                                            , COD_GRUPO_CONVENIO as CodGrupoConvenio
                                            , CPF as Cpf
                                            , DATA_NASCIMENTO AS DataNascimentoPortador
                                            , NOME_PORTADOR AS NomePortador
                                            , MATRICULA AS Matricula
                                            , ID_CONTA AS IdConta
                                            , STATUS_CONTA AS StatusConta
                                            , STATUS_CARTAO AS StatusCartao
                                            , ULT_DIGITOS_CARTAO AS NumeroCartao
                                            , DATA_EMISSAO_CARTAO AS DataEmissaoCartao
                                            , LIMITE_MENSAL AS LimiteMensal
                                FROM BI_UNIK.TB_CARTOES 

                                WHERE CODIGO_CONVENIO = {convenioId} AND COD_GRUPO_CONVENIO = {grupoId}
                      )          
                WHERE rn between: pageNumber + :skip and :pageNumber + :pageSize - :skip";

            using var conn = new DbSession(_config.GetConnectionString("BIConnection")).Connection;

            IEnumerable<ReportCartao> records = await conn.QueryAsync<ReportCartao>(paginatequery, new
            {
                pageNumber = skip * limit + 1,
                skip,
                pageSize = limit
            });

            return records.ToList();
        }

        public async Task<ResponseDto<ReportContas>> GetContas(long convenioId, long grupoId, int limit, int skip, string search, string order, DateTime? from, DateTime? to, string? cardsFilter)
        {
            string filter = "1 = 1";
            string orderBy = "CODIGO_CONVENIO ASC";
            string between = "1 = 1";

            if (string.IsNullOrEmpty(cardsFilter))
            {
                cardsFilter = "last";
            }

            if (from != null && to != null)
            {
                between = $@"(DATA_CAD_CONTA BETWEEN to_date('{from?.ToString("dd/MM/yyyy")} 00:00:00', 'DD.MM.YYYY HH24:MI:SS') AND to_date('{to?.ToString("dd/MM/yyyy")} 23:59:59', 'DD.MM.YYYY HH24:MI:SS'))";
            }


            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                filter = $@"(LOWER(NOME_PORTADOR) LIKE '%{search}%' OR 
                LOWER(CPF) LIKE '%{search.Replace(".", "").Replace("-", "")}%' OR
                LOWER(MATRICULA) LIKE '%{search}%' OR
                LOWER(ID_CONTA) LIKE '%{search}%' OR
                LOWER(STATUS_CONTA) LIKE '%{search}%' OR
                LOWER(COD_GRUPO_CONVENIO) LIKE '%{search}%' OR
                LOWER(CODIGO_CONVENIO) LIKE '%{search}%' OR
                LOWER(LIMITE_MENSAL) LIKE '%{search}%')";
            }

            if (!String.IsNullOrEmpty(order))
            {
                var part = order.Split(' ');
                switch (part[0].ToLower())
                {
                    case "codigoconvenio": orderBy = "A.CODIGO_CONVENIO " + part[1]; break;
                    case "codgrupoconvenio": orderBy = "A.COD_GRUPO_CONVENIO " + part[1]; break;
                    case "cpf": orderBy = "A.CPF " + part[1]; break;
                    case "nomeportador": orderBy = "A.NOME_PORTADOR " + part[1]; break;
                    case "matricula": orderBy = "A.MATRICULA " + part[1]; break;
                    case "idconta": orderBy = "A.ID_CONTA " + part[1]; break;
                    case "statusconta": orderBy = "A.STATUS_CONTA " + part[1]; break;
                    case "limitemensal": orderBy = "A.LIMITE_MENSAL " + part[1]; break;
                    case "codfilial": orderBy = "C.CODIGO " + part[1]; break;
                    case "filial": orderBy = "C.DESCRICAO " + part[1]; break;
                    case "centrocusto": orderBy = "D.DESCRICAO " + part[1]; break;
                }
            }
            string cardsJoin = " and ((A.ID_CARTAO = (select max(x.ID_CARTAO) from BI_UNIK.TB_CARTOES x where x.ID_CONTA = B.IDCLIENTETITULARPESSOAFISICA AND x.STATUS_CARTAO = 'DESBLOQUEADO')) OR " +
                    "(A.ID_CARTAO = (select max(x.ID_CARTAO) from BI_UNIK.TB_CARTOES x where x.ID_CONTA = B.IDCLIENTETITULARPESSOAFISICA AND x.STATUS_CARTAO = 'BLOQUEADO') AND NOT EXISTS (select 1 from BI_UNIK.TB_CARTOES xx where xx.ID_CONTA = B.IDCLIENTETITULARPESSOAFISICA AND xx.STATUS_CARTAO = 'DESBLOQUEADO')) OR" +
                    "(A.ID_CARTAO = (select max(x.ID_CARTAO) from BI_UNIK.TB_CARTOES x where x.ID_CONTA = B.IDCLIENTETITULARPESSOAFISICA AND x.STATUS_CARTAO = 'CANCELADO') AND NOT EXISTS (select 1 from BI_UNIK.TB_CARTOES xx where xx.ID_CONTA = B.IDCLIENTETITULARPESSOAFISICA AND xx.STATUS_CARTAO in ('BLOQUEADO','DESBLOQUEADO')))) ";
            if (cardsFilter == "all")
            {
                cardsJoin = "";
            }
            string basequery = $@" 
                           SELECT 
                             A.CODIGO_CONVENIO as codConvenio
                            ,A.COD_GRUPO_CONVENIO as codGrupoConvenio
                            ,A.STATUS_CONVENIO as statusConvenio
                            ,A.ID_CONTA as idConta
                            ,A.CPF as Cpf
                            ,A.NOME_PORTADOR as nomePortador
                            ,A.MATRICULA as matricula
                            ,A.STATUS_CONTA as statusConta
                            ,A.DATA_CAD_CONTA as dtCadConta
                            ,A.DATA_CANCELAMENTO_CONTA as dtCancelConta
                            ,A.LIMITE_MENSAL as limiteMensal
                            ,C.CODIGO AS codFilial
                            ,C.DESCRICAO as filial
                            ,D.DESCRICAO as centroCusto";

            string whereQuery = $@" FROM BI_UNIK.TB_CARTOES A
                            JOIN bi_unik.vm_t_clientetitularpessoafisic B ON A.ID_CONTA = B.IDCLIENTETITULARPESSOAFISICA {cardsJoin}
                            LEFT JOIN BI_UNIK.vm_t_filialfuncionario C ON B.IDFILIALFUNCIONARIO = C.IDFILIALFUNCIONARIO
                            LEFT JOIN BI_UNIK.VM_t_centrocusto D ON D.IDFILIALFUNCIONARIO = C.IDFILIALFUNCIONARIO
                            WHERE CODIGO_CONVENIO = {convenioId} AND COD_GRUPO_CONVENIO = {grupoId} AND {filter} AND {between} ORDER BY {orderBy}";

            string countquery = $@"SELECT count(*) AS COUNT {whereQuery}";
            string query = $"SELECT b.* FROM(SELECT a.*, rownum r__ FROM ( {basequery + whereQuery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";

            using var conn = new DbSession(_config.GetConnectionString("BIConnection")).Connection;
            var count = await conn.QueryFirstOrDefaultAsync<long>(countquery);

            var records = await conn.QueryAsync<ReportContas>(query, new
            {
                pageNumber = (skip / limit) + 1,
                pageSize = limit
            });

            return new ResponseDto<ReportContas>
            {
                CurrentPage = (skip / limit) + 1,
                Data = records.ToList(),
                PerPage = limit,
                Total = count
            };
        }

        public Task<long> GetContasExportCSVTotalPages(long convenioId, long grupoId)
        => GetCartoesExportCSVTotalPages(convenioId, grupoId);

        public async Task<List<ReportContas>> GetContasExportCSV(long convenioId, long grupoId, int skip)
        {
            int limit = 10000;

            string basequery = $@" 
                           SELECT 
                             A.CODIGO_CONVENIO as codConvenio
                            ,A.COD_GRUPO_CONVENIO as codGrupoConvenio
                            ,A.STATUS_CONVENIO as statusConvenio
                            ,A.ID_CONTA as idConta
                            ,A.CPF as Cpf
                            ,A.NOME_PORTADOR as nomePortador
                            ,A.MATRICULA as matricula
                            ,A.STATUS_CONTA as statusConta
                            ,A.LIMITE_MENSAL as limiteMensal
                            ,C.CODIGO AS codFilial
                            ,C.DESCRICAO as filial
                            ,D.DESCRICAO as centroCusto
                            FROM BI_UNIK.TB_CARTOES A
                            LEFT JOIN bi_unik.vm_t_clientetitularpessoafisic B ON A.ID_CONTA = B.IDCLIENTETITULARPESSOAFISICA
                            LEFT JOIN BI_UNIK.vm_t_filialfuncionario C ON B.IDFILIALFUNCIONARIO = C.IDFILIALFUNCIONARIO
                            LEFT JOIN BI_UNIK.VM_t_centrocusto D ON D.IDFILIALFUNCIONARIO = C.IDFILIALFUNCIONARIO
                            WHERE CODIGO_CONVENIO = {convenioId} AND COD_GRUPO_CONVENIO = {grupoId}";


            string query = $"SELECT b.* FROM(SELECT a.*, rownum r__ FROM ( {basequery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";


            using var conn = new DbSession(_config.GetConnectionString("BIConnection")).Connection;

            IEnumerable<ReportContas> records = await conn.QueryAsync<ReportContas>(query, new
            {
                pageNumber = (skip / limit) + 1,
                pageSize = limit
            });

            return records.ToList();
        }

        public async Task<ResponseDto<PainelGerencial>> GetPainelGerencialPaged(PainelGerencialFilterDto filterOptions, int limit, int skip, string order)
        {
            using (var conn = new DbSession(_config.GetConnectionString("PortalConnection")).Connection)
            {

                string filterSolicitacao = "1 = 1";
                string filterUsuario = "1 = 1";
                string orderBy = "CONVENIO_ID DESC";
                if (!String.IsNullOrEmpty(filterOptions.SearchTerm))
                {
                    filterOptions.SearchTerm = filterOptions.SearchTerm.ToLower();
                    //remove non numeric characters
                    var document = new string(filterOptions.SearchTerm.Where(c => char.IsDigit(c)).ToArray());
                    if (document.Length > 0)
                    {
                        filterSolicitacao += $" AND (LOWER(tu.NOME) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(tu.CPF) LIKE '%{document}%' OR REGEXP_REPLACE(tu.CPF, '[^0-9]+', '') LIKE '%{document}%') ";
                        filterUsuario += $" AND (LOWER(usu.NOME) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(usu.CPF) LIKE '%{document}%' OR REGEXP_REPLACE(usu.CPF, '[^0-9]+', '') LIKE '%{document}%') ";

                    }
                    else
                    {
                        filterSolicitacao += $" AND (LOWER(tu.NOME) LIKE '%{filterOptions.SearchTerm}%') ";
                        filterUsuario += $" AND (LOWER(usu.NOME) LIKE '%{filterOptions.SearchTerm}%') ";
                    }
                }
                if (filterOptions.GroupId != null && filterOptions.GroupId.Length > 0)
                {
                    filterSolicitacao += " AND ts.GRUPO_ID = ANY :Groups ";
                    filterUsuario += " AND tu.GRUPO_ID = ANY :Groups ";
                }
                if (filterOptions.ShopId != null && filterOptions.ShopId.Length > 0)
                {
                    filterSolicitacao += " AND ts.CONVENIO_ID = ANY :Shops ";
                    filterUsuario += " AND tuc.CONVENIO_ID = ANY :Shops ";
                }

                if (filterOptions.SolicitationStartDate != null)
                {
                    filterSolicitacao += $" AND ts.DATA_SOLICITACAO >= :SolicitationStartDate ";
                    filterUsuario += $" AND tu.DATA_CADASTRO >= :SolicitationStartDate ";
                }
                if (filterOptions.SolicitationEndDate != null)
                {
                    filterOptions.SolicitationEndDate = filterOptions.SolicitationEndDate.Value.AddDays(1);
                    filterSolicitacao += $" AND ts.DATA_SOLICITACAO <= :SolicitationEndDate ";
                    filterUsuario += $" AND tu.DATA_CADASTRO <= :SolicitationEndDate ";
                }
                if (!String.IsNullOrEmpty(order))
                {
                    var part = order.Split(' ');
                    switch (part[0].ToLower())
                    {
                        case "shopname":
                            orderBy = "NOME_CONVENIO " + part[1];
                            break;
                        case "shopdocument":
                            orderBy = "CNPJ_CONVENIO " + part[1];
                            break;
                        case "usercount":
                            orderBy = "5 " + part[1];
                            break;
                        case "changelimitcount":
                            orderBy = "6 " + part[1];
                            break;
                        case "globallimitcount":
                            orderBy = "7 " + part[1];
                            break;
                        case "cardrequestcount":
                            orderBy = "8 " + part[1];
                            break;
                        case "cardreissuecount":
                            orderBy = "9 " + part[1];
                            break;
                        case "dismissalcount":
                            orderBy = "10 " + part[1];
                            break;
                        case "unblockcount":
                            orderBy = "11 " + part[1];
                            break;
                        case "accountupdatecount":
                            orderBy = "12 " + part[1];
                            break;
                        case "totalrequestscount":
                            orderBy = "13 " + part[1];
                            break;
                    }
                }
                string countquery = $@"SELECT count(*) AS COUNT 
                                        FROM (
                                            SELECT GRUPO_ID,CONVENIO_ID
                                            FROM 
                                            (

                                            SELECT ts.GRUPO_ID , ts.CONVENIO_ID,  max(ts.NOME_CONVENIO) AS NOME_CONVENIO , max(ts.CNPJ_CONVENIO) AS CNPJ_CONVENIO ,ts.TIPO_SOLICITACAO_ID , count(1) AS QTD
                                            FROM PORTALRH.T_SOLICITACAO ts 
                                            JOIN T_USUARIO tu ON tu.ID = ts.USUARIO_ID 
                                            WHERE {filterSolicitacao}
                                            GROUP BY ts.GRUPO_ID ,ts.CONVENIO_ID ,ts.TIPO_SOLICITACAO_ID

                                            UNION ALL 

                                            SELECT tu.GRUPO_ID, tuc.CONVENIO_ID ,NULL , NULL , -1, count(1)
                                            FROM T_USUARIO_CONVENIO tuc 
                                            INNER JOIN T_USUARIO tu ON tu.ID = tuc.USUARIO_ID 
                                            JOIN T_USUARIO usu ON usu.ID = tu.USUARIO_CADASTRO_ID 
                                            WHERE {filterUsuario}
                                            GROUP BY tu.GRUPO_ID, tuc.CONVENIO_ID

                                            )

                                            GROUP BY GRUPO_ID, CONVENIO_ID
                                        )";
                string basequery = $@"SELECT GRUPO_ID AS groupId,CONVENIO_ID as shopId, max(NOME_CONVENIO) AS shopName, max(CNPJ_CONVENIO) AS shopDocument, 
                                        sum(CASE WHEN TIPO_SOLICITACAO_ID = -1 THEN QTD ELSE 0 END) AS userCount,
                                        sum(CASE WHEN TIPO_SOLICITACAO_ID = 1 THEN QTD ELSE 0 END) AS changeLimitCount,
                                        sum(CASE WHEN TIPO_SOLICITACAO_ID = 10 THEN QTD ELSE 0 END) AS globalLimitCount,
                                        sum(CASE WHEN TIPO_SOLICITACAO_ID = 6 THEN QTD ELSE 0 END) AS cardRequestCount,
                                        sum(CASE WHEN TIPO_SOLICITACAO_ID = 7 THEN QTD ELSE 0 END) AS cardReissueCount,
                                        sum(CASE WHEN TIPO_SOLICITACAO_ID = 8 THEN QTD ELSE 0 END) AS dismissalCount,
                                        sum(CASE WHEN TIPO_SOLICITACAO_ID = 9 THEN QTD ELSE 0 END) AS unblockCount,
                                        sum(CASE WHEN TIPO_SOLICITACAO_ID = 11 THEN QTD ELSE 0 END) AS accountUpdateCount,
                                        sum(QTD) AS totalRequestsCount
                                        FROM 
                                        (

                                            SELECT ts.GRUPO_ID , ts.CONVENIO_ID,  max(ts.NOME_CONVENIO) AS NOME_CONVENIO , max(ts.CNPJ_CONVENIO) AS CNPJ_CONVENIO ,ts.TIPO_SOLICITACAO_ID , count(1) AS QTD
                                            FROM PORTALRH.T_SOLICITACAO ts 
                                            JOIN T_USUARIO tu ON tu.ID = ts.USUARIO_ID 
                                            WHERE {filterSolicitacao}
                                            GROUP BY ts.GRUPO_ID ,ts.CONVENIO_ID ,ts.TIPO_SOLICITACAO_ID

                                            UNION ALL 

                                            SELECT tu.GRUPO_ID, tuc.CONVENIO_ID ,NULL , NULL , -1, count(1)
                                            FROM T_USUARIO_CONVENIO tuc 
                                            INNER JOIN T_USUARIO tu ON tu.ID = tuc.USUARIO_ID 
                                            JOIN T_USUARIO usu ON usu.ID = tu.USUARIO_CADASTRO_ID 
                                            WHERE {filterUsuario}
                                            GROUP BY tu.GRUPO_ID, tuc.CONVENIO_ID

                                            )

                                        GROUP BY GRUPO_ID, CONVENIO_ID
                                         ORDER BY {orderBy}";
                string query = $"SELECT b.* FROM(SELECT a.*, rownum r__ FROM ( {basequery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";
                var count = await conn.QueryFirstOrDefaultAsync<long>(countquery, new
                {
                    Groups = filterOptions.GroupId,
                    Shops = filterOptions.ShopId,
                    SolicitationStartDate = filterOptions.SolicitationStartDate,
                    SolicitationEndDate = filterOptions.SolicitationEndDate
                });
                var data = await conn.QueryAsync<PainelGerencial>(query, new
                {
                    Groups = filterOptions.GroupId,
                    Shops = filterOptions.ShopId,
                    SolicitationStartDate = filterOptions.SolicitationStartDate,
                    SolicitationEndDate = filterOptions.SolicitationEndDate,
                    pageNumber = (skip / limit) + 1,
                    pageSize = limit
                });
                return new ResponseDto<PainelGerencial>
                {
                    CurrentPage = (skip / limit) + 1,
                    Data = data.ToList(),
                    PerPage = limit,
                    Total = count
                };
            }
        }
        public async Task<List<PainelGerencial>> GetPainelGerencialAll(PainelGerencialFilterDto filterOptions)
        {
            using (var conn = new DbSession(_config.GetConnectionString("PortalConnection")).Connection)
            {

                string filterSolicitacao = "1 = 1";
                string filterUsuario = "1 = 1";
                if (!String.IsNullOrEmpty(filterOptions.SearchTerm))
                {
                    filterOptions.SearchTerm = filterOptions.SearchTerm.ToLower();
                    //remove non numeric characters
                    var document = new string(filterOptions.SearchTerm.Where(c => char.IsDigit(c)).ToArray());
                    if (document.Length > 0)
                    {
                        filterSolicitacao += $" AND (LOWER(tu.NOME) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(tu.CPF) LIKE '%{document}%' OR REGEXP_REPLACE(tu.CPF, '[^0-9]+', '') LIKE '%{document}%') ";
                        filterUsuario += $" AND (LOWER(usu.NOME) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(usu.CPF) LIKE '%{document}%' OR REGEXP_REPLACE(usu.CPF, '[^0-9]+', '') LIKE '%{document}%') ";

                    }
                    else
                    {
                        filterSolicitacao += $" AND (LOWER(tu.NOME) LIKE '%{filterOptions.SearchTerm}%') ";
                        filterUsuario += $" AND (LOWER(usu.NOME) LIKE '%{filterOptions.SearchTerm}%') ";
                    }
                }
                if (filterOptions.GroupId != null && filterOptions.GroupId.Length > 0)
                {
                    filterSolicitacao += " AND ts.GRUPO_ID = ANY :Groups ";
                    filterUsuario += " AND tu.GRUPO_ID = ANY :Groups ";
                }
                if (filterOptions.ShopId != null && filterOptions.ShopId.Length > 0)
                {
                    filterSolicitacao += " AND ts.CONVENIO_ID = ANY :Shops ";
                    filterUsuario += " AND tuc.CONVENIO_ID = ANY :Shops ";
                }

                if (filterOptions.SolicitationStartDate != null)
                {
                    filterSolicitacao += $" AND ts.DATA_SOLICITACAO >= :SolicitationStartDate ";
                    filterUsuario += $" AND tu.DATA_CADASTRO >= :SolicitationStartDate ";
                }
                if (filterOptions.SolicitationEndDate != null)
                {
                    filterOptions.SolicitationEndDate = filterOptions.SolicitationEndDate.Value.AddDays(1);
                    filterSolicitacao += $" AND ts.DATA_SOLICITACAO <= :SolicitationEndDate ";
                    filterUsuario += $" AND tu.DATA_CADASTRO <= :SolicitationEndDate ";
                }


                string basequery = $@"SELECT GRUPO_ID AS groupId,CONVENIO_ID as shopId, max(NOME_CONVENIO) AS shopName, max(CNPJ_CONVENIO) AS shopDocument, 
                                        sum(CASE WHEN TIPO_SOLICITACAO_ID = -1 THEN QTD ELSE 0 END) AS userCount,
                                        sum(CASE WHEN TIPO_SOLICITACAO_ID = 1 THEN QTD ELSE 0 END) AS changeLimitCount,
                                        sum(CASE WHEN TIPO_SOLICITACAO_ID = 10 THEN QTD ELSE 0 END) AS globalLimitCount,
                                        sum(CASE WHEN TIPO_SOLICITACAO_ID = 6 THEN QTD ELSE 0 END) AS cardRequestCount,
                                        sum(CASE WHEN TIPO_SOLICITACAO_ID = 7 THEN QTD ELSE 0 END) AS cardReissueCount,
                                        sum(CASE WHEN TIPO_SOLICITACAO_ID = 8 THEN QTD ELSE 0 END) AS dismissalCount,
                                        sum(CASE WHEN TIPO_SOLICITACAO_ID = 9 THEN QTD ELSE 0 END) AS unblockCount,
                                        sum(CASE WHEN TIPO_SOLICITACAO_ID = 11 THEN QTD ELSE 0 END) AS accountUpdateCount,
                                        sum(QTD) AS totalRequestsCount
                                        FROM 
                                        (

                                            SELECT ts.GRUPO_ID , ts.CONVENIO_ID,  max(ts.NOME_CONVENIO) AS NOME_CONVENIO , max(ts.CNPJ_CONVENIO) AS CNPJ_CONVENIO ,ts.TIPO_SOLICITACAO_ID , count(1) AS QTD
                                            FROM PORTALRH.T_SOLICITACAO ts 
                                            JOIN T_USUARIO tu ON tu.ID = ts.USUARIO_ID 
                                            WHERE {filterSolicitacao}
                                            GROUP BY ts.GRUPO_ID ,ts.CONVENIO_ID ,ts.TIPO_SOLICITACAO_ID

                                            UNION ALL 

                                            SELECT tu.GRUPO_ID, tuc.CONVENIO_ID ,NULL , NULL , -1, count(1)
                                            FROM T_USUARIO_CONVENIO tuc 
                                            INNER JOIN T_USUARIO tu ON tu.ID = tuc.USUARIO_ID 
                                            JOIN T_USUARIO usu ON usu.ID = tu.USUARIO_CADASTRO_ID 
                                            WHERE {filterUsuario}
                                            GROUP BY tu.GRUPO_ID, tuc.CONVENIO_ID

                                            )

                                        GROUP BY GRUPO_ID, CONVENIO_ID";

                return (await conn.QueryAsync<PainelGerencial>(basequery, new
                {
                    Groups = filterOptions.GroupId,
                    Shops = filterOptions.ShopId,
                    SolicitationStartDate = filterOptions.SolicitationStartDate,
                    SolicitationEndDate = filterOptions.SolicitationEndDate
                })).ToList();

            }
        }
    }
}
