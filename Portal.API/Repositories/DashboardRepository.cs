using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Portal.API.Data;
using Portal.API.Entities;
using Portal.API.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly IConfiguration _config;
        private string connectionString = "";
        private readonly IMemoryCache _memoryCache;

        public DashboardRepository(IConfiguration config,
            IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _config = config;
            connectionString = _config.GetConnectionString("BIConnection");
        }

        public async Task<List<DashboardIssueCancellations>> GetIssueCancellations(int idConvenio, int? idGrupo = null)
        {
            var cacheKey = $"DashboardGetIssueCancellations-{idConvenio}-{idGrupo}";
            if (!_memoryCache.TryGetValue(cacheKey, out List<DashboardIssueCancellations> result))
            {

                using var conn = new DbSession(connectionString).Connection;
                string query = @"SELECT 
                                gc.idgrupoconvenio,
                                tl.idloja,
                                tl.idservicosfinanceiros,
                                to_char(bc.data,'mm') as mes,
                                to_char(bc.data,'yyyy') as ano,
                                sum(1) as contas_novas_mes,
                                sum(case when ctpf.status_cliente = 'C' then 1 else 0 end) as contas_canceladas_mes
                                FROM bi_unik.vm_t_loja tl
                                inner join bi_unik.vm_t_grupoconvenio gc on tl.idgrupoconvenio = gc.idgrupoconvenio
                                inner join bi_unik.vm_t_clientetitularpessoafisic ctpf on tl.idloja = ctpf.idloja
                                inner join bi_unik.vm_t_cartaoloja cl on ctpf.idclientetitularpessoafisica = cl.idclientepessoafisica
                                inner join bi_unik.vm_t_pessoafisica pf on ctpf.idclientetitularpessoafisica = pf.idpessoafisica
                                INNER JOIN bi_unik.vm_t_cadastro tc ON ctpf.idclientetitularpessoafisica = tc.idcadastro
                                left join BI_UNIK.vm_t_bloqueiocliente bc on ctpf.idclientetitularpessoafisica = bc.idclientetitularpessoafisica
                                INNER JOIN bi_unik.vm_client c ON tl.idloja = c.clnt_cod_cli
                                INNER JOIN bi_unik.vm_t_tipocontrato tp ON tl.idtipocontrato = tp.idtipocontrato
                                inner join BI_UNIK.vm_t_servicosfinanceiros sf on tl.idservicosfinanceiros = sf.idservicosfinanceiros
                                where c.clnt_situacao in ('A','B') AND bc.data is not null and (tl.idloja = :IdConvenio OR tl.idgrupoconvenio = :IdGrupo)
                                group by
                                gc.idgrupoconvenio,
                                tl.idloja,
                                tl.idservicosfinanceiros,
                                to_char(bc.data,'mm'),
                                to_char(bc.data,'yyyy')
                                order by gc.idgrupoconvenio, tl.idloja,to_char(bc.data,'yyyy') , to_char(bc.data,'mm')";
                result = (await conn.QueryAsync<DashboardIssueCancellations>(query, new
                {
                    IdConvenio = idConvenio,
                    IdGrupo = idGrupo
                })).ToList();
                _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(60));
            }
            return result;

        }
        public async Task<List<DashboardAccountUsage>> GetAccountUsage(int idConvenio, int? idGrupo = null)
        {
            var cacheKey = $"DashboardGetAccountUsage-{idConvenio}-{idGrupo}";
            if (!_memoryCache.TryGetValue(cacheKey, out List<DashboardAccountUsage> accountUsage))
            {
                using var conn = new DbSession(connectionString).Connection;
                string query = @"WITH cte_tb_venda AS(
                                   SELECT  *
                                   FROM BI_UNIK.VM_T_VENDA vtv
                                   WHERE STATUS = 'A' AND DATA >= add_months(trunc(sysdate, 'month'), - 12) 
                            )
                            , retira_dupl_trans AS (
                                   SELECT       DISTINCT ID_TRANSACAO, GRUPO_CONVENVIO, NOME_GRUPO_CONVENIO, CODIGO_CONVENVNIO, valor_compra
                                   FROM BI_UNIK.TB_TRANSACOES_HIST
                                    WHERE (CODIGO_CONVENVNIO = :IdConvenio OR GRUPO_CONVENVIO = :IdGrupo)
                            )
                            SELECT       
                                  GRUPO_CONVENVIO           AS GRUPO_CONVENIO
                                , NOME_GRUPO_CONVENIO       AS NOME_GRUPO_CONVENIO
                                , CODIGO_CONVENVNIO         AS CODIGO_CONVENIO
                                , extract(YEAR from data)   AS ANO
                                , extract(MONTH from data)  AS MES
                                , sum(valor_compra)         AS VALOR
                                , count(1)                  AS QUANTIDADE
                            FROM retira_dupl_trans tth INNER JOIN cte_tb_venda tv ON tth.ID_TRANSACAO = tv.idvenda
                            WHERE (CODIGO_CONVENVNIO = :IdConvenio OR GRUPO_CONVENVIO = :IdGrupo)
                            GROUP BY GRUPO_CONVENVIO, NOME_GRUPO_CONVENIO, CODIGO_CONVENVNIO, extract(YEAR from data), extract(MONTH from data)
                            ORDER BY GRUPO_CONVENVIO, NOME_GRUPO_CONVENIO, CODIGO_CONVENVNIO, extract(YEAR from data), extract(MONTH from data)";
                accountUsage = (await conn.QueryAsync<DashboardAccountUsage>(query, new
                {
                    IdConvenio = idConvenio,
                    IdGrupo = idGrupo,
                })).ToList();
                _memoryCache.Set(cacheKey, accountUsage, TimeSpan.FromMinutes(60));
            }
            return accountUsage;

        }
        public async Task<DashboardAccount> GetAccounts(int idConvenio, int? idGrupo = null)
        {
            var cacheKey = $"DashboardGetAccounts-{idConvenio}-{idGrupo}";

            if (!_memoryCache.TryGetValue(cacheKey, out DashboardAccount totals))
            {

                using var conn = new DbSession(connectionString).Connection;
                string query = @"SELECT ID_GRUPO_CONVENIO       AS IdGroup, 
                                    ID_CONVENIO             AS IdConvenio, 
                                    TOTAL_CONTAS            AS TotalAccounts, 
                                    CONTAS_ATIVAS           AS ActiveAccounts, 
                                    CONTAS_BLOQUEADAS       AS BlockedAccounts, 
                                    CONTAS_CANCELADAS       AS CancelledAccounts, 
                                    TOTAL_CARTAO            AS TotalCards, 
                                    CARTOES_ATIVOS          AS ActiveCards, 
                                    CARTOES_BLOQUEADOS      AS BlockedCards, 
                                    CONTAS_NOVAS_MES        AS CurrentNewAccounts, 
                                    CONTAS_CANCELADAS_MES   AS CurrentCancelledAccounts, 
                                    CONTAS_BLOQUEADAS_MES   AS CurrentBlockedAccounts,
                                    CONTAS_COM_TRANSACAO    AS CurrentTransactionAccounts, 
                                    PORCENT_ATIVACAO        AS ActiviationPercentage
                            FROM BI_UNIK.VW_PORTALRH_CONTAS WHERE (ID_CONVENIO = :IdConvenio OR ID_GRUPO_CONVENIO = :IdGrupo)";
                totals = await conn.QueryFirstOrDefaultAsync<DashboardAccount>(query, new
                {
                    IdConvenio = idConvenio,
                    IdGrupo = idGrupo
                });
                _memoryCache.Set(cacheKey, totals, TimeSpan.FromMinutes(60));
            }
            return totals;
        }
        public async Task<List<DashboardCity>> GetCities(int idConvenio)
        {
            var cacheKey = $"DashboardGetCities-{idConvenio}";

            if (!_memoryCache.TryGetValue(cacheKey, out List<DashboardCity> cities))
            {
                using var conn = new DbSession(connectionString).Connection;
                string query = @"SELECT ID_GRUPO_CONVENIO       AS IdGroup, 
                                    ID_CONVENIO             AS IdConvenio, 
                                    QTDE_COLABORADORES      AS Employees, 
                                    LOCALIDADE              AS City
                                    FROM BI_UNIK.VW_PORTALRH_CIDADE
                            WHERE ID_CONVENIO = :IdConvenio";
                cities = (await conn.QueryAsync<DashboardCity>(query, new
                {
                    IdConvenio = idConvenio
                })).ToList();
                _memoryCache.Set(cacheKey, cities, TimeSpan.FromMinutes(60));
            }
            return cities;
        }
        public async Task<DashboardDemographic> GetDemographic(int idConvenio)
        {
            var cacheKey = $"DashboardGetDemographic-{idConvenio}";
            if (!_memoryCache.TryGetValue(cacheKey, out DashboardDemographic demographic))
            {
                using var conn = new DbSession(connectionString).Connection;
                string query = @"SELECT  ID_GRUPO_CONVENIO       AS IdGroup, 
                                            ID_CONVENIO             AS IdConvenio,     
                                            QTDE_COLABORADORES      AS Employees, 
                                            ""I18-24""                  AS AgeFrom18To24, 
                                            ""I25-34""                  AS AgeFrom25To34, 
                                            ""I35-44""                  AS AgeFrom35To44, 
                                            ""I45-54""                  AS AgeFrom45To54, 
                                            ""I55-64""                  AS AgeFrom55To64, 
                                            ""I65-99""                  AS AgeFrom65To99, 
                                            NAO_INFORMADO           AS AgeUnknown, 
                                            SEXO_MASCULINO          AS SexMaleQuantity,  
                                            PORCENT_MASC            AS SexMalePercentage, 
                                            SEXO_FEMININO           AS SexFemaleQuantity, 
                                            PORCENT_FEM             AS SexFemalePercentage, 
                                            SEXO_OUTROS             AS SexOtherQuantity, 
                                            PORCENT_OUTROS          AS SexOtherPercentage, 
                                            SEXO_NAO_INFORMADO      AS SexUnknownQuantity, 
                                            PORCENT_NAO_INFORMADO   AS SexUnknownPercentage
                            FROM BI_UNIK.VW_PORTALRH_IDADE_SEXO
                            WHERE ID_CONVENIO = :IdConvenio";
                demographic = await conn.QueryFirstOrDefaultAsync<DashboardDemographic>(query, new
                {
                    IdConvenio = idConvenio
                });
                _memoryCache.Set(cacheKey, demographic, TimeSpan.FromMinutes(60));
            }
            return demographic;
        }
        public async Task<List<DashboardMcc>> GetMcc(int idConvenio)
        {
            var cacheKey = $"DashboardGetMcc-{idConvenio}";
            if (!_memoryCache.TryGetValue(cacheKey, out List<DashboardMcc> mcc))
            {
                using var conn = new DbSession(connectionString).Connection;
                string query = @"SELECT GRUPO_CONV      AS IdGroup, 
                                    COD_CONV        AS IdConvenio, 
                                    DESCRICAO       AS Description, 
                                    PORCENTAGEM     AS Percentage
                            FROM BI_UNIK.VW_PORTALRH_MCC
                            WHERE COD_CONV = :IdConvenio";
                mcc = (await conn.QueryAsync<DashboardMcc>(query, new
                {
                    IdConvenio = idConvenio
                })).OrderByDescending(m => m.Percentage).Take(10).ToList();
                _memoryCache.Set(cacheKey, mcc, TimeSpan.FromMinutes(60));
            }
            return mcc;
        }


        public async Task<List<DashboardContasCanceladas>> GetContasCanceladas(int idConvenio, int? idGrupo = null)
        {
            var cacheKey = $"DashboardGetContasCanceladas-{idConvenio}-{idGrupo}";
            if (!_memoryCache.TryGetValue(cacheKey, out List<DashboardContasCanceladas> contasCanceladas))
            {
                using var conn = new DbSession(connectionString).Connection;
                string query = @"select 
                                gc.idgrupoconvenio id_grupo_convenio,
                                tl.idloja id_convenio,
                                to_char(bc.data,'yyyy') ano,
                                to_char(bc.data,'mm') mes,
                                count (distinct ctpf.IDCLIENTETITULARPESSOAFISICA) contas_canceladas
                              FROM bi_unik.vm_t_loja tl
                                inner join bi_unik.vm_t_grupoconvenio gc on tl.idgrupoconvenio = gc.idgrupoconvenio
                                inner join bi_unik.vm_t_clientetitularpessoafisic ctpf on tl.idloja = ctpf.idloja
                                inner join BI_UNIK.vm_t_bloqueiocliente bc on ctpf.idclientetitularpessoafisica = bc.idclientetitularpessoafisica
                              where ctpf.status_cliente = 'C' AND tl.idloja = :IdConvenio
                                group by
                                gc.idgrupoconvenio,
                                tl.idloja,
                                tl.idservicosfinanceiros,
                                to_char(bc.data,'mm'),
                                to_char(bc.data,'yyyy')
                              order by 1,2,3,4,5";
                contasCanceladas = (await conn.QueryAsync<DashboardContasCanceladas>(query, new
                {
                    IdConvenio = idConvenio,
                    IdGrupo = idGrupo
                })).ToList();
                _memoryCache.Set(cacheKey, contasCanceladas, TimeSpan.FromMinutes(60));
            }
            return contasCanceladas;
        }
        public async Task<List<DashboardContasNovas>> GetContasNovas(int idConvenio, int? idGrupo = null)
        {
            var cacheKey = $"DashboardGetContasNovas-{idConvenio}-{idGrupo}";
            if (!_memoryCache.TryGetValue(cacheKey, out List<DashboardContasNovas> contasNovas))
            {
                using var conn = new DbSession(connectionString).Connection;
                string query = @"select 
                                gc.idgrupoconvenio id_grupo_convenio,
                                tl.idloja id_convenio,
                                to_char(tc.datacadastro,'yyyy') ano,
                                to_char(tc.datacadastro,'mm') mes,
                                COUNT(distinct ctpf.IDCLIENTETITULARPESSOAFISICA) contas_novas
                             FROM bi_unik.vm_t_loja tl
                                inner join bi_unik.vm_t_grupoconvenio gc on tl.idgrupoconvenio = gc.idgrupoconvenio
                                inner join bi_unik.vm_t_clientetitularpessoafisic ctpf on tl.idloja = ctpf.idloja
                                INNER JOIN bi_unik.vm_t_cadastro tc ON ctpf.idclientetitularpessoafisica = tc.idcadastro
                                INNER JOIN bi_unik.vm_client c ON tl.idloja = c.clnt_cod_cli
                             where c.clnt_situacao in ('A','B') AND tl.idloja = :IdConvenio
                                group by
                                gc.idgrupoconvenio,
                                tl.idloja,
                                tl.idservicosfinanceiros,
                                to_char(tc.datacadastro,'mm'),
                                to_char(tc.datacadastro,'yyyy')
                             order by 1,2,3,4,5";
                contasNovas = (await conn.QueryAsync<DashboardContasNovas>(query, new
                {
                    IdConvenio = idConvenio,
                    IdGrupo = idGrupo
                })).ToList();
                _memoryCache.Set(cacheKey, contasNovas, TimeSpan.FromMinutes(60));
            }
            return contasNovas;
        }
    }
}

