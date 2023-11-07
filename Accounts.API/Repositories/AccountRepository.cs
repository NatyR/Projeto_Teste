using Accounts.API.Entities;
using Accounts.API.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Dapper;
using Microsoft.Extensions.Configuration;
using Accounts.API.Data;
using System.Data;
using Dapper.Oracle;
using Accounts.API.Common.Dto;
using Accounts.API.Common.Dto.Account;
using System.IO;
using Amazon.S3.Model;

namespace Accounts.API.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IConfiguration _config;

        private readonly string ConnectionString;
        public AccountRepository(IConfiguration config)
        {
            _config = config;
            ConnectionString = _config.GetConnectionString("ASConnection");
        }
        public async Task<ResponseDto<BlockedAccount>> GetBlockedPaged(int limit, int skip, string order, BlockedAccountFilterDto filterOptions)
        {
            using (var conn = new DbSession(ConnectionString).Connection)
            {
                string filter = "1 = 1";
                string orderBy = "tb.DATA ASC";
                if (!String.IsNullOrEmpty(order))
                {
                    var part = order.Split(' ');
                    switch (part[0].ToLower())
                    {
                        case "groupid":
                            orderBy = "tg.IDGRUPOCONVENIO " + part[1];
                            break;
                        case "groupname":
                            orderBy = "tg.DESCRICAO " + part[1];
                            break;
                        case "shopid":
                            orderBy = "tl.IDLOJA " + part[1];
                            break;
                        case "shopname":
                            orderBy = "tl.DESCRICAO " + part[1];
                            break;
                        case "cpf":
                            orderBy = "PF.CPF " + part[1];
                            break;
                        case "branchid":
                            orderBy = "tf.IDFILIALFUNCIONARIO " + part[1];
                            break;
                        case "branchcode":
                            orderBy = "tf.CODIGO " + part[1];
                            break;
                        case "branchname":
                            orderBy = "tf.DESCRICAO " + part[1];
                            break;
                        case "costcenterid":
                            orderBy = "cc.IDCENTROCUSTO " + part[1];
                            break;
                        case "costcentercode":
                            orderBy = "cc.CODIGOINTERNO " + part[1];
                            break;
                        case "costcentername":
                            orderBy = "cc.DESCRICAO " + part[1];
                            break;
                        case "registrationnumber":
                            orderBy = "CT.CAMPOAUX1 " + part[1];
                            break;
                        case "createdat":
                            orderBy = "CL.DATAEMISSAO " + part[1];
                            break;
                        case "blockedat":
                            orderBy = "tb.DATA " + part[1];
                            break;
                        case "cardlimit":
                            orderBy = "LI.LIMITEAPLICADO " + part[1];
                            break;
                        case "cardnumber":
                            orderBy = "CL.NUMEROGERADO " + part[1];
                            break;

                    }
                }
                if (filterOptions.BlockTypeId != null && filterOptions.BlockTypeId.Length > 0)
                {
                    filter += $" AND tb.IDTIPOBLOQUEIOCLIENTE = ANY :BlockTypes ";
                }
                if (filterOptions.GroupId != null && filterOptions.GroupId.Length > 0)
                {
                    filter += $" AND tg.IDGRUPOCONVENIO = ANY :Groups ";
                }
                if (filterOptions.ShopId != null && filterOptions.ShopId.Length > 0)
                {
                    filter += $" AND tl.IDLOJA = ANY :Shops ";
                }
                if (filterOptions.StartDate != null)
                {
                    filter += $" AND tb.DATA >= :StartDate ";
                }
                if (filterOptions.EndDate != null)
                {
                    filterOptions.EndDate = filterOptions.EndDate.Value.AddDays(1);
                    filter += $" AND tb.DATA <= :EndDate ";
                }

                string countquery = $@"SELECT count(*) AS COUNT 
                                       FROM NEWUNIK.T_PESSOAFISICA PF
                                        INNER JOIN NEWUNIK.T_CLIENTETITULARPESSOAFISICA CT ON PF.IDPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA
                                        INNER JOIN NEWUNIK.T_CARTAOLOJA CL ON CL.IDCLIENTEPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA
                                        INNER JOIN NEWUNIK.T_BLOQUEIOCLIENTE tb ON tb.IDCLIENTETITULARPESSOAFISICA = PF.IDPESSOAFISICA 
                                        INNER JOIN NEWUNIK.T_LOJA tl ON tl.IDLOJA = CT.IDLOJA 
                                        INNER JOIN NEWUNIK.T_GRUPOCONVENIO tg ON tg.IDGRUPOCONVENIO  = tl.IDGRUPOCONVENIO 
                                        JOIN NEWUNIK.t_limitecredito LI ON LI.IDCLIENTETITULARPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA AND LI.IDLIMITECREDITO = (SELECT max(tl2.IDLIMITECREDITO) FROM NEWUNIK.T_LIMITECREDITO tl2 WHERE tl2.IDCLIENTETITULARPESSOAFISICA = LI.IDCLIENTETITULARPESSOAFISICA)
                                        LEFT JOIN NEWUNIK.T_FILIALFUNCIONARIO tf ON tf.IDFILIALFUNCIONARIO  = CT.IDFILIALFUNCIONARIO 
                                        LEFT JOIN NEWUNIK.T_CENTROCUSTO cc ON cc.IDCENTROCUSTO = CT.IDCENTROCUSTO 
                                        WHERE {filter}";
                string basequery = $@"SELECT tg.IDGRUPOCONVENIO AS GroupId,
                                tg.DESCRICAO AS GroupName,
                                tl.IDLOJA AS ShopId,
                                tl.DESCRICAOEXIBIVELCLIENTE AS ShopName,
                                tf.IDFILIALFUNCIONARIO AS BranchId,
                                tf.CODIGO AS BranchCode,
                                tf.DESCRICAO AS BranchName,
                                cc.IDCENTROCUSTO AS CostCenterId,
                                cc.CODIGOINTERNO AS CostCenterCode,
                                cc.DESCRICAO AS CostCenterName,
                                PF.CPF AS Cpf,
                                CT.CAMPOAUX1             AS RegistrationNumber,
                                CL.DATAEMISSAO AS CreatedAt,
                                tb.DATA AS BlockedAt,
                                LI.LIMITEAPLICADO/100        AS CardLimit,
                                CL.NUMEROGERADO          AS CardNumber

                                FROM NEWUNIK.T_PESSOAFISICA PF
                                INNER JOIN NEWUNIK.T_CLIENTETITULARPESSOAFISICA CT ON PF.IDPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA
                                INNER JOIN NEWUNIK.T_CARTAOLOJA CL ON CL.IDCLIENTEPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA
                                INNER JOIN NEWUNIK.T_BLOQUEIOCLIENTE tb ON tb.IDCLIENTETITULARPESSOAFISICA = PF.IDPESSOAFISICA 
                                INNER JOIN NEWUNIK.T_LOJA tl ON tl.IDLOJA = CT.IDLOJA 
                                INNER JOIN NEWUNIK.T_GRUPOCONVENIO tg ON tg.IDGRUPOCONVENIO  = tl.IDGRUPOCONVENIO 
                                JOIN NEWUNIK.t_limitecredito LI ON LI.IDCLIENTETITULARPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA AND LI.IDLIMITECREDITO = (SELECT max(tl2.IDLIMITECREDITO) FROM NEWUNIK.T_LIMITECREDITO tl2 WHERE tl2.IDCLIENTETITULARPESSOAFISICA = LI.IDCLIENTETITULARPESSOAFISICA)
                                LEFT JOIN NEWUNIK.T_FILIALFUNCIONARIO tf ON tf.IDFILIALFUNCIONARIO  = CT.IDFILIALFUNCIONARIO 
                                LEFT JOIN NEWUNIK.T_CENTROCUSTO cc ON cc.IDCENTROCUSTO = CT.IDCENTROCUSTO 
                                WHERE {filter} ORDER BY {orderBy}";
                string query = $"SELECT b.* FROM(SELECT a.*, rownum r__ FROM ( {basequery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";
                var count = await conn.QueryFirstOrDefaultAsync<long>(countquery, new
                {
                    BlockTypes = filterOptions.BlockTypeId,
                    Groups = filterOptions.GroupId,
                    Shops = filterOptions.ShopId,
                    StartDate = filterOptions.StartDate,
                    EndDate = filterOptions.EndDate
                });
                var accounts = await conn.QueryAsync<BlockedAccount>(query, new
                {
                    BlockTypes = filterOptions.BlockTypeId,
                    Groups = filterOptions.GroupId,
                    Shops = filterOptions.ShopId,
                    StartDate = filterOptions.StartDate,
                    EndDate = filterOptions.EndDate,
                    pageNumber = (skip / limit) + 1,
                    pageSize = limit
                });
                return new ResponseDto<BlockedAccount>
                {
                    CurrentPage = (skip / limit) + 1,
                    Data = accounts.ToList(),
                    PerPage = limit,
                    Total = count
                };
            }
        }
        public async Task<List<BlockedAccount>> GetBlocked(BlockedAccountFilterDto filterOptions)
        {
            using (var conn = new DbSession(ConnectionString).Connection)
            {
                string filter = "1 = 1";

                if (filterOptions.BlockTypeId != null && filterOptions.BlockTypeId.Length > 0)
                {
                    filter += " AND tb.IDTIPOBLOQUEIOCLIENTE = ANY :BlockTypes ";
                }
                if (filterOptions.GroupId != null && filterOptions.GroupId.Length > 0)
                {
                    filter += " AND tg.IDGRUPOCONVENIO = ANY :Groups ";
                }
                if (filterOptions.ShopId != null && filterOptions.ShopId.Length > 0)
                {
                    filter += " AND tl.IDLOJA = ANY :Shops ";
                }
                if (filterOptions.StartDate != null)
                {
                    filter += $" AND tb.DATA >= :StartDate ";
                }
                if (filterOptions.EndDate != null)
                {
                    filterOptions.EndDate = filterOptions.EndDate.Value.AddDays(1);
                    filter += $" AND tb.DATA <= :EndDate ";
                }


                string basequery = $@"SELECT tg.IDGRUPOCONVENIO AS GroupId,
                                tg.DESCRICAO AS GroupName,
                                tl.IDLOJA AS ShopId,
                                tl.DESCRICAOEXIBIVELCLIENTE AS ShopName,
                                tf.IDFILIALFUNCIONARIO AS BranchId,
                                tf.CODIGO AS BranchCode,
                                tf.DESCRICAO AS BranchName,
                                cc.IDCENTROCUSTO AS CostCenterId,
                                cc.CODIGOINTERNO AS CostCenterCode,
                                cc.DESCRICAO AS CostCenterName,
                                PF.CPF AS Cpf,
                                CT.CAMPOAUX1             AS RegistrationNumber,
                                CL.DATAEMISSAO AS CreatedAt,
                                tb.DATA AS BlockedAt,
                                LI.LIMITEAPLICADO/100        AS CardLimit,
                                CL.NUMEROGERADO          AS CardNumber

                                FROM NEWUNIK.T_PESSOAFISICA PF
                                INNER JOIN NEWUNIK.T_CLIENTETITULARPESSOAFISICA CT ON PF.IDPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA
                                INNER JOIN NEWUNIK.T_CARTAOLOJA CL ON CL.IDCLIENTEPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA
                                INNER JOIN NEWUNIK.T_BLOQUEIOCLIENTE tb ON tb.IDCLIENTETITULARPESSOAFISICA = PF.IDPESSOAFISICA 
                                INNER JOIN NEWUNIK.T_LOJA tl ON tl.IDLOJA = CT.IDLOJA 
                                INNER JOIN NEWUNIK.T_GRUPOCONVENIO tg ON tg.IDGRUPOCONVENIO  = tl.IDGRUPOCONVENIO 
                                JOIN NEWUNIK.t_limitecredito LI ON LI.IDCLIENTETITULARPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA AND LI.IDLIMITECREDITO = (SELECT max(tl2.IDLIMITECREDITO) FROM NEWUNIK.T_LIMITECREDITO tl2 WHERE tl2.IDCLIENTETITULARPESSOAFISICA = LI.IDCLIENTETITULARPESSOAFISICA)
                                LEFT JOIN NEWUNIK.T_FILIALFUNCIONARIO tf ON tf.IDFILIALFUNCIONARIO  = CT.IDFILIALFUNCIONARIO 
                                LEFT JOIN NEWUNIK.T_CENTROCUSTO cc ON cc.IDCENTROCUSTO = CT.IDCENTROCUSTO 
                                WHERE {filter} ";

                return (await conn.QueryAsync<BlockedAccount>(basequery, new
                {
                    BlockTypes = filterOptions.BlockTypeId,
                    Groups = filterOptions.GroupId,
                    Shops = filterOptions.ShopId,
                    StartDate = filterOptions.StartDate,
                    EndDate = filterOptions.EndDate
                })).ToList();
            }
        }
        public async Task<Account> Create(Account account)
        {
            using var conn = new DbSession(ConnectionString).Connection;
            //var parms = new OracleDynamicParameters();
            //parms.Add(":Name", account.Name, OracleMappingType.Varchar2, ParameterDirection.Input);
            //parms.Add(":Id", account.Name, OracleMappingType.Double, ParameterDirection.Output);

            //string query = @"INSERT INTO BI_UNIK.VM_T_PESSOA (IDPESSOA, NOME) VALUES(S_PESSOA.nextval, :Name)  returning IDPESSOA into :Id";
            //using (var transaction = conn.BeginTransaction())
            //{
            //    await transaction.ExecuteAsync(query, parms);
            //    account.Id = parms.Get<long>("Id");
            //    transaction.Commit();
            //}
            return account;
        }
        public async Task<ResponseDto<Account>> GetAllPaged(long convenio_id, int limit, int skip, string search, string order, string? accountStatus, string? cardStatus, string? cardsFilter)
        {
            using (var conn = new DbSession(ConnectionString).Connection)
            {
                string filter = "1 = 1";
                string orderBy = "PF.IDPESSOAFISICA ASC";
                if (string.IsNullOrEmpty(cardsFilter)){
                    cardsFilter = "last";
                }
                if (!String.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    filter = $"(LOWER(CT.NOME_CARTAO) LIKE '%{search}%' OR PF.CPF LIKE '%{search.Replace(".", "").Replace("-", "")}%' OR LOWER(PF.EMAIL) LIKE '%{search}%')";
                }
                if (!String.IsNullOrEmpty(order))
                {
                    var part = order.Split(' ');
                    switch (part[0].ToLower())
                    {
                        case "id":
                            orderBy = "PF.IDPESSOAFISICA " + part[1];
                            break;
                        case "name":
                            orderBy = "CT.NOME_CARTAO " + part[1];
                            break;
                        case "cpf":
                            orderBy = "PF.CPF " + part[1];
                            break;
                        case "email":
                            orderBy = "PF.EMAIL " + part[1];
                            break;
                        case "cardlimit":
                            orderBy = "LI.LIMITEAPLICADO " + part[1];
                            break;
                        case "cardnumber":
                            orderBy = "CL.NUMEROGERADO " + part[1];
                            break;
                        case "status":
                            orderBy = "CT.STATUS_CLIENTE " + part[1];
                            break;
                        case "cardstatus":
                            orderBy = "CL.STATUS " + part[1];
                            break;
                    }
                }
                if (!String.IsNullOrEmpty(accountStatus))
                {
                    filter += $" AND CT.STATUS_CLIENTE IN :AccountStatus ";
                }
                if (!String.IsNullOrEmpty(cardStatus))
                {
                    filter += $" AND CL.STATUS IN :CardStatus ";
                }
                string cardsJoin = " and ((CL.IDCARTAOLOJA = (select max(x.IDCARTAOLOJA) from NEWUNIK.T_CARTAOLOJA x where x.IDCLIENTEPESSOAFISICA = PF.IDPESSOAFISICA AND x.STATUS = 'V')) OR " +
                    "(CL.IDCARTAOLOJA = (select max(x.IDCARTAOLOJA) from NEWUNIK.T_CARTAOLOJA x where x.IDCLIENTEPESSOAFISICA = PF.IDPESSOAFISICA AND x.STATUS = 'B') AND NOT EXISTS (select 1 from NEWUNIK.T_CARTAOLOJA xx where xx.IDCLIENTEPESSOAFISICA = PF.IDPESSOAFISICA AND xx.STATUS = 'V')) OR" +
                    "(CL.IDCARTAOLOJA = (select max(x.IDCARTAOLOJA) from NEWUNIK.T_CARTAOLOJA x where x.IDCLIENTEPESSOAFISICA = PF.IDPESSOAFISICA AND x.STATUS = 'I') AND NOT EXISTS (select 1 from NEWUNIK.T_CARTAOLOJA xx where xx.IDCLIENTEPESSOAFISICA = PF.IDPESSOAFISICA AND xx.STATUS in ('B','V')))) ";
                if (cardsFilter == "all")
                {
                    cardsJoin = "";
                }
                string countquery = $@"SELECT count(*) AS COUNT 
                                        FROM NEWUNIK.T_PESSOAFISICA PF 
                                        JOIN NEWUNIK.T_CLIENTETITULARPESSOAFISICA CT ON PF.IDPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA 
                                        LEFT JOIN NEWUNIK.T_ENDERECO E ON E.IDENDERECO = CT.IDENDERECORESIDENCIAL                                 
                                        JOIN newunik.t_cartaoloja CL ON CL.IDCLIENTEPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA {cardsJoin}
                                        JOIN newunik.t_limitecredito LI ON LI.IDCLIENTETITULARPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA  AND LI.IDLIMITECREDITO = (SELECT max(tl2.IDLIMITECREDITO) FROM NEWUNIK.T_LIMITECREDITO tl2 WHERE tl2.IDCLIENTETITULARPESSOAFISICA = LI.IDCLIENTETITULARPESSOAFISICA)
                                        WHERE CT.IDLOJA = :Convenio AND {filter}";
                string basequery = $@"SELECT PF.IDPESSOAFISICA        AS Id,
                                    PF.SEXO                  AS Gender,
                                    PF.TEMPOEMPREGADO        AS TimeEmployed,
                                    PF.RG                    AS Rg,
                                    PF.TEMPOMORADIA          AS TimeResidence,
                                    PF.ESTADONATAL           AS BirthState,
                                    PF.CIDADENATAL           AS BirthCity,
                                    PF.EMAIL                 AS Email,
                                    PF.IDESCOLARIDADE        AS ScholarshipId,
                                    PF.IDESTADOCIVIL         AS MaritalStatusId,
                                    PF.CPF                   AS Cpf,
                                    PF.IDPROFISSAO           AS OccupationId,
                                    PF.DATANASCIMENTO        AS BirthDate,
                                    PF.IDMORADIA             AS ResidenceTypeId,
                                    PF.ORGAOEMISSOR          AS RgIssuer,
                                    PF.DATAEXPEDICAO         AS RgIssuedDate,
                                    PF.NACIONALIDADE         AS Nationality,
                                    PF.ITF_ENVIADA           AS ItfSent,
                                    PF.LOCALEMISSAO          AS IssuePlace,
                                    PF.CNH                   AS Cnh,
                                    PF.NOMEMAE               AS MothersName,
                                    CT.IDFILIALFUNCIONARIO   AS BranchId,
                                    CT.IDCENTROCUSTO         AS CostCenterId,
                                    CT.STATUSIMPCONTRATO     AS ContractImpStatus,
                                    CT.ULTIMADATAIMPCONTRATO AS ContractImpLastDate,
                                    CT.IDLOJA                AS Convenio,
                                    CT.NOME_CARTAO           AS Name,
                                    CT.STATUS_CLIENTE        AS Status,
                                    CT.IDULTIMOTIPOBLOQUEIO  AS BlockTypeId,
                                    CT.IDLOJACADASTRO        AS OriginalConvenio,
                                    CT.IDENDERECORESIDENCIAL AS HomeAddressId,
                                    CT.CAMPOAUX1             AS RegistrationNumber,
                                    (SELECT CONCAT(T.AREA,T.TELEFONE) 
	FROM NEWUNIK.T_TELEFONE_PESSOA TP 
	INNER JOIN NEWUNIK.T_TELEFONE T ON T.IDTELEFONE = TP.IDTELEFONE 
	WHERE T.TIPOTELEFONE = 'X' AND TP.IDPESSOA = PF.IDPESSOAFISICA AND ROWNUM = 1) AS PhoneNumber,
                                    E.BAIRRO                 AS Neighborhood,
                                    E.NUMERO                 AS AddressNumber,
                                    E.LOGRADOURO             AS Street,
                                    E.LOCALIDADE             AS CityName,
                                    E.UF                     AS State,
                                    E.CEP                    AS ZipCode,
                                    E.COMPLEMENTO            AS AddressComplement,
                                    LI.LIMITEAPLICADO/100        AS CardLimit,
                                    LI.DATA                  AS CardLimitUpdatedAt,
                                    CL.NUMEROGERADO          AS CardNumber,
                                    CL.STATUS          AS CardStatus,
                                    (SELECT OBSERVACAO FROM
	                                NEWUNIK.t_bloqueiocliente BC
	                                WHERE BC.IDCLIENTETITULARPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA AND ROWNUM = 1
	                                ) AS Reason
                            FROM NEWUNIK.T_PESSOAFISICA PF
                                JOIN NEWUNIK.T_CLIENTETITULARPESSOAFISICA CT ON PF.IDPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA
                                LEFT JOIN NEWUNIK.T_ENDERECO E ON E.IDENDERECO  = CT.IDENDERECORESIDENCIAL
                                JOIN NEWUNIK.t_cartaoloja CL ON CL.IDCLIENTEPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA {cardsJoin}
                                JOIN NEWUNIK.t_limitecredito LI ON LI.IDCLIENTETITULARPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA AND LI.IDLIMITECREDITO = (SELECT max(tl2.IDLIMITECREDITO) FROM NEWUNIK.T_LIMITECREDITO tl2 WHERE tl2.IDCLIENTETITULARPESSOAFISICA = LI.IDCLIENTETITULARPESSOAFISICA)
                            WHERE CT.IDLOJA = :Convenio AND {filter} ORDER BY {orderBy}";
                string query = $"SELECT b.* FROM(SELECT a.*, rownum r__ FROM ( {basequery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";
                var count = await conn.QueryFirstOrDefaultAsync<long>(countquery, new
                {
                    Convenio = convenio_id,
                    AccountStatus = (!String.IsNullOrEmpty(accountStatus) ? accountStatus.Split(',') : null),
                    CardStatus = (!String.IsNullOrEmpty(cardStatus) ? cardStatus.Split(',') : null)
                });
                var accounts = await conn.QueryAsync<Account>(query, new
                {
                    Convenio = convenio_id,
                    AccountStatus = (!String.IsNullOrEmpty(accountStatus) ? accountStatus.Split(',') : null),
                    CardStatus = (!String.IsNullOrEmpty(cardStatus) ? cardStatus.Split(',') : null),
                    pageNumber = (skip / limit) + 1,
                    pageSize = limit
                });
                return new ResponseDto<Account>
                {
                    CurrentPage = (skip / limit) + 1,
                    Data = accounts.ToList(),
                    PerPage = limit,
                    Total = count
                };
            }
        }
        public async Task<List<Account>> GetAllByConvenio(long convenioId)
        {
            string query = @"SELECT PF.IDPESSOAFISICA        AS Id,
                                    PF.SEXO                  AS Gender,
                                    PF.TEMPOEMPREGADO        AS TimeEmployed,
                                    PF.RG                    AS Rg,
                                    PF.TEMPOMORADIA          AS TimeResidence,
                                    PF.ESTADONATAL           AS BirthState,
                                    PF.CIDADENATAL           AS BirthCity,
                                    PF.EMAIL                 AS Email,
                                    PF.IDESCOLARIDADE        AS ScholarshipId,
                                    PF.IDESTADOCIVIL         AS MaritalStatusId,
                                    PF.CPF                   AS Cpf,
                                    PF.IDPROFISSAO           AS OccupationId,
                                    PF.DATANASCIMENTO        AS BirthDate,
                                    PF.IDMORADIA             AS ResidenceTypeId,
                                    PF.ORGAOEMISSOR          AS RgIssuer,
                                    PF.DATAEXPEDICAO         AS RgIssuedDate,
                                    PF.NACIONALIDADE         AS Nationality,
                                    PF.ITF_ENVIADA           AS ItfSent,
                                    PF.LOCALEMISSAO          AS IssuePlace,
                                    PF.CNH                   AS Cnh,
                                    PF.NOMEMAE               AS MothersName,
                                    CT.IDFILIALFUNCIONARIO   AS BranchId,
                                    CT.IDCENTROCUSTO         AS CostCenterId,
                                    CT.STATUSIMPCONTRATO     AS ContractImpStatus,
                                    CT.ULTIMADATAIMPCONTRATO AS ContractImpLastDate,
                                    CT.IDLOJA                AS Convenio,
                                    CT.NOME_CARTAO           AS Name,
                                    CT.STATUS_CLIENTE        AS Status,
                                    CT.IDULTIMOTIPOBLOQUEIO  AS BlockTypeId,
                                    CT.IDLOJACADASTRO        AS OriginalConvenio,
                                    CT.IDENDERECORESIDENCIAL AS HomeAddressId,
                                    CT.CAMPOAUX1             AS RegistrationNumber,
                                    (SELECT CONCAT(T.AREA,T.TELEFONE) 
	FROM NEWUNIK.T_TELEFONE_PESSOA TP 
	INNER JOIN NEWUNIK.T_TELEFONE T ON T.IDTELEFONE = TP.IDTELEFONE 
	WHERE T.TIPOTELEFONE = 'X' AND TP.IDPESSOA = PF.IDPESSOAFISICA AND ROWNUM = 1) AS PhoneNumber,
                                    E.BAIRRO                 AS Neighborhood,
                                    E.NUMERO                 AS AddressNumber,
                                    E.LOGRADOURO             AS Street,
                                    E.LOCALIDADE             AS CityName,
                                    E.UF                     AS State,
                                    E.CEP                    AS ZipCode,
                                    E.COMPLEMENTO            AS AddressComplement,
                                    LI.LIMITEAPLICADO/100        AS CardLimit,
                                    LI.DATA                  AS CardLimitUpdatedAt,
                                    CL.NUMEROGERADO          AS CardNumber,
                                    CL.STATUS                AS CardStatus
                            FROM NEWUNIK.T_PESSOAFISICA PF
                                JOIN NEWUNIK.T_CLIENTETITULARPESSOAFISICA CT ON PF.IDPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA
                                LEFT JOIN NEWUNIK.T_ENDERECO E ON E.IDENDERECO  = CT.IDENDERECORESIDENCIAL
                                JOIN NEWUNIK.t_cartaoloja CL ON CL.IDCLIENTEPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA
                                JOIN NEWUNIK.t_limitecredito LI ON LI.IDCLIENTETITULARPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA AND LI.IDLIMITECREDITO = (SELECT max(tl2.IDLIMITECREDITO) FROM NEWUNIK.T_LIMITECREDITO tl2 WHERE tl2.IDCLIENTETITULARPESSOAFISICA = LI.IDCLIENTETITULARPESSOAFISICA)
                            WHERE CT.IDLOJA = :Convenio
                             ORDER BY PF.IDPESSOAFISICA DESC";
            using (var conn = new DbSession(ConnectionString).Connection)
            {
                return (await conn.QueryAsync<Account>(query, new
                {
                    Convenio = convenioId
                })).ToList();
            }
        }
        public async Task<Account> GetByRegistrationNumber(string registration, long convenio)
        {
            string query = @"SELECT PF.IDPESSOAFISICA        AS Id,
                                    PF.SEXO                  AS Gender,
                                    PF.TEMPOEMPREGADO        AS TimeEmployed,
                                    PF.RG                    AS Rg,
                                    PF.TEMPOMORADIA          AS TimeResidence,
                                    PF.ESTADONATAL           AS BirthState,
                                    PF.CIDADENATAL           AS BirthCity,
                                    PF.EMAIL                 AS Email,
                                    PF.IDESCOLARIDADE        AS ScholarshipId,
                                    PF.IDESTADOCIVIL         AS MaritalStatusId,
                                    PF.CPF                   AS Cpf,
                                    PF.IDPROFISSAO           AS OccupationId,
                                    PF.DATANASCIMENTO        AS BirthDate,
                                    PF.IDMORADIA             AS ResidenceTypeId,
                                    PF.ORGAOEMISSOR          AS RgIssuer,
                                    PF.DATAEXPEDICAO         AS RgIssuedDate,
                                    PF.NACIONALIDADE         AS Nationality,
                                    PF.ITF_ENVIADA           AS ItfSent,
                                    PF.LOCALEMISSAO          AS IssuePlace,
                                    PF.CNH                   AS Cnh,
                                    PF.NOMEMAE               AS MothersName,
                                    CT.IDFILIALFUNCIONARIO   AS BranchId,
                                    CT.IDCENTROCUSTO         AS CostCenterId,
                                    CT.STATUSIMPCONTRATO     AS ContractImpStatus,
                                    CT.ULTIMADATAIMPCONTRATO AS ContractImpLastDate,
                                    CT.IDLOJA                AS Convenio,
                                    CT.NOME_CARTAO           AS Name,
                                    CT.STATUS_CLIENTE        AS Status,
                                    CT.IDULTIMOTIPOBLOQUEIO  AS BlockTypeId,
                                    CT.IDLOJACADASTRO        AS OriginalConvenio,
                                    CT.IDENDERECORESIDENCIAL AS HomeAddressId,
                                    CT.CAMPOAUX1             AS RegistrationNumber,
                                    (SELECT CONCAT(T.AREA,T.TELEFONE) 
	FROM NEWUNIK.T_TELEFONE_PESSOA TP 
	INNER JOIN NEWUNIK.T_TELEFONE T ON T.IDTELEFONE = TP.IDTELEFONE 
	WHERE T.TIPOTELEFONE = 'X' AND TP.IDPESSOA = PF.IDPESSOAFISICA AND ROWNUM = 1) AS PhoneNumber,
                                    E.BAIRRO                 AS Neighborhood,
                                    E.NUMERO                 AS AddressNumber,
                                    E.LOGRADOURO             AS Street,
                                    E.LOCALIDADE             AS CityName,
                                    E.UF                     AS State,
                                    E.CEP                    AS ZipCode,
                                    E.COMPLEMENTO            AS AddressComplement
                            FROM NEWUNIK.T_PESSOAFISICA PF
                                JOIN NEWUNIK.T_CLIENTETITULARPESSOAFISICA CT ON PF.IDPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA
                                LEFT JOIN NEWUNIK.T_ENDERECO E ON E.IDENDERECO  = CT.IDENDERECORESIDENCIAL
                            WHERE CT.CAMPOAUX1 = :RegistrationNumber AND CT.STATUS_CLIENTE <> 'C' 
                            AND CT.IDLOJA = :Convenio";

            using (var conn = new DbSession(ConnectionString).Connection)
            {
                return (await conn.QueryFirstOrDefaultAsync<Account>(query, new
                {
                    RegistrationNumber = registration,
                    Convenio = convenio
                }));
            }
        }
        public async Task<Account> GetByCpfAndConvenio(string cpf, long convenio)
        {
            string query = @"SELECT PF.IDPESSOAFISICA        AS Id,
                                    PF.SEXO                  AS Gender,
                                    PF.TEMPOEMPREGADO        AS TimeEmployed,
                                    PF.RG                    AS Rg,
                                    PF.TEMPOMORADIA          AS TimeResidence,
                                    PF.ESTADONATAL           AS BirthState,
                                    PF.CIDADENATAL           AS BirthCity,
                                    PF.EMAIL                 AS Email,
                                    PF.IDESCOLARIDADE        AS ScholarshipId,
                                    PF.IDESTADOCIVIL         AS MaritalStatusId,
                                    PF.CPF                   AS Cpf,
                                    PF.IDPROFISSAO           AS OccupationId,
                                    PF.DATANASCIMENTO        AS BirthDate,
                                    PF.IDMORADIA             AS ResidenceTypeId,
                                    PF.ORGAOEMISSOR          AS RgIssuer,
                                    PF.DATAEXPEDICAO         AS RgIssuedDate,
                                    PF.NACIONALIDADE         AS Nationality,
                                    PF.ITF_ENVIADA           AS ItfSent,
                                    PF.LOCALEMISSAO          AS IssuePlace,
                                    PF.CNH                   AS Cnh,
                                    PF.NOMEMAE               AS MothersName,
                                    CT.IDFILIALFUNCIONARIO   AS BranchId,
                                    CT.IDCENTROCUSTO         AS CostCenterId,
                                    CT.STATUSIMPCONTRATO     AS ContractImpStatus,
                                    CT.ULTIMADATAIMPCONTRATO AS ContractImpLastDate,
                                    CT.IDLOJA                AS Convenio,
                                    CT.NOME_CARTAO           AS Name,
                                    CT.STATUS_CLIENTE        AS Status,
                                    CT.IDULTIMOTIPOBLOQUEIO  AS BlockTypeId,
                                    CT.IDLOJACADASTRO        AS OriginalConvenio,
                                    CT.IDENDERECORESIDENCIAL AS HomeAddressId,
                                    CT.CAMPOAUX1             AS RegistrationNumber,
                                    (SELECT CONCAT(T.AREA,T.TELEFONE) 
	FROM NEWUNIK.T_TELEFONE_PESSOA TP 
	INNER JOIN NEWUNIK.T_TELEFONE T ON T.IDTELEFONE = TP.IDTELEFONE 
	WHERE T.TIPOTELEFONE = 'X' AND TP.IDPESSOA = PF.IDPESSOAFISICA AND ROWNUM = 1) AS PhoneNumber,
                                    E.BAIRRO                 AS Neighborhood,
                                    E.NUMERO                 AS AddressNumber,
                                    E.LOGRADOURO             AS Street,
                                    E.LOCALIDADE             AS CityName,
                                    E.UF                     AS State,
                                    E.CEP                    AS ZipCode,
                                    E.COMPLEMENTO            AS AddressComplement,
                                    LI.LIMITEAPLICADO/100        AS CardLimit,
                                    LI.DATA                  AS CardLimitUpdatedAt,
                                    CL.NUMEROGERADO          AS CardNumber,
                                    CL.STATUS                AS CardStatus
                            FROM NEWUNIK.T_PESSOAFISICA PF
                                JOIN NEWUNIK.T_CLIENTETITULARPESSOAFISICA CT ON PF.IDPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA
                                JOIN NEWUNIK.t_cartaoloja CL ON CL.IDCLIENTEPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA
                                JOIN NEWUNIK.t_limitecredito LI ON LI.IDCLIENTETITULARPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA AND LI.IDLIMITECREDITO = (SELECT max(tl2.IDLIMITECREDITO) FROM NEWUNIK.T_LIMITECREDITO tl2 WHERE tl2.IDCLIENTETITULARPESSOAFISICA = LI.IDCLIENTETITULARPESSOAFISICA)
                                LEFT JOIN NEWUNIK.T_ENDERECO E ON E.IDENDERECO  = CT.IDENDERECORESIDENCIAL
                            WHERE PF.CPF = :Cpf AND CT.STATUS_CLIENTE <> 'C' 
                            AND CT.IDLOJA = :Convenio";

            using (var conn = new DbSession(ConnectionString).Connection)
            {
                return (await conn.QueryFirstOrDefaultAsync<Account>(query, new
                {
                    Cpf = cpf.Replace(".", "").Replace("-", ""),
                    Convenio = convenio
                }));
            }
        }
        public async Task<Account> GetByCpfAndGrupo(string cpf, long grupo)
        {
            string query = @"SELECT PF.IDPESSOAFISICA        AS Id,
                                    PF.SEXO                  AS Gender,
                                    PF.TEMPOEMPREGADO        AS TimeEmployed,
                                    PF.RG                    AS Rg,
                                    PF.TEMPOMORADIA          AS TimeResidence,
                                    PF.ESTADONATAL           AS BirthState,
                                    PF.CIDADENATAL           AS BirthCity,
                                    PF.EMAIL                 AS Email,
                                    PF.IDESCOLARIDADE        AS ScholarshipId,
                                    PF.IDESTADOCIVIL         AS MaritalStatusId,
                                    PF.CPF                   AS Cpf,
                                    PF.IDPROFISSAO           AS OccupationId,
                                    PF.DATANASCIMENTO        AS BirthDate,
                                    PF.IDMORADIA             AS ResidenceTypeId,
                                    PF.ORGAOEMISSOR          AS RgIssuer,
                                    PF.DATAEXPEDICAO         AS RgIssuedDate,
                                    PF.NACIONALIDADE         AS Nationality,
                                    PF.ITF_ENVIADA           AS ItfSent,
                                    PF.LOCALEMISSAO          AS IssuePlace,
                                    PF.CNH                   AS Cnh,
                                    PF.NOMEMAE               AS MothersName,
                                    CT.IDFILIALFUNCIONARIO   AS BranchId,
                                    CT.IDCENTROCUSTO         AS CostCenterId,
                                    CT.STATUSIMPCONTRATO     AS ContractImpStatus,
                                    CT.ULTIMADATAIMPCONTRATO AS ContractImpLastDate,
                                    CT.IDLOJA                AS Convenio,
                                    CT.NOME_CARTAO           AS Name,
                                    CT.STATUS_CLIENTE        AS Status,
                                    CT.IDULTIMOTIPOBLOQUEIO  AS BlockTypeId,
                                    CT.IDLOJACADASTRO        AS OriginalConvenio,
                                    CT.IDENDERECORESIDENCIAL AS HomeAddressId,
                                    CT.CAMPOAUX1             AS RegistrationNumber,
                                    (SELECT CONCAT(T.AREA,T.TELEFONE) 
	FROM NEWUNIK.T_TELEFONE_PESSOA TP 
	INNER JOIN NEWUNIK.T_TELEFONE T ON T.IDTELEFONE = TP.IDTELEFONE 
	WHERE T.TIPOTELEFONE = 'X' AND TP.IDPESSOA = PF.IDPESSOAFISICA AND ROWNUM = 1) AS PhoneNumber,
                                    E.BAIRRO                 AS Neighborhood,
                                    E.NUMERO                 AS AddressNumber,
                                    E.LOGRADOURO             AS Street,
                                    E.LOCALIDADE             AS CityName,
                                    E.UF                     AS State,
                                    E.CEP                    AS ZipCode,
                                    E.COMPLEMENTO            AS AddressComplement,
                                    CL.NUMEROGERADO          AS CardNumber,
                                    CL.STATUS                AS CardStatus
                            FROM NEWUNIK.T_PESSOAFISICA PF
                                JOIN NEWUNIK.T_CLIENTETITULARPESSOAFISICA CT ON PF.IDPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA
                                JOIN NEWUNIK.t_cartaoloja CL ON CL.IDCLIENTEPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA
                                JOIN NEWUNIK.T_LOJA L ON L.IDLOJA = CT.IDLOJA
                                LEFT JOIN NEWUNIK.T_ENDERECO E ON E.IDENDERECO  = CT.IDENDERECORESIDENCIAL
                            WHERE PF.CPF = :Cpf AND CT.STATUS_CLIENTE <> 'C' 
                            AND L.IDGRUPOCONVENIO = :Grupo";

            using (var conn = new DbSession(ConnectionString).Connection)
            {
                return (await conn.QueryFirstOrDefaultAsync<Account>(query, new
                {
                    Cpf = cpf,
                    Grupo = grupo
                }));
            }
        }
        public async Task<Account> Get(long id, long convenio)
        {
            string query = @"SELECT PF.IDPESSOAFISICA        AS Id,
                                    PF.SEXO                  AS Gender,
                                    PF.TEMPOEMPREGADO        AS TimeEmployed,
                                    PF.RG                    AS Rg,
                                    PF.TEMPOMORADIA          AS TimeResidence,
                                    PF.ESTADONATAL           AS BirthState,
                                    PF.CIDADENATAL           AS BirthCity,
                                    PF.EMAIL                 AS Email,
                                    PF.IDESCOLARIDADE        AS ScholarshipId,
                                    PF.IDESTADOCIVIL         AS MaritalStatusId,
                                    PF.CPF                   AS Cpf,
                                    PF.IDPROFISSAO           AS OccupationId,
                                    PF.DATANASCIMENTO        AS BirthDate,
                                    PF.IDMORADIA             AS ResidenceTypeId,
                                    PF.ORGAOEMISSOR          AS RgIssuer,
                                    PF.DATAEXPEDICAO         AS RgIssuedDate,
                                    PF.NACIONALIDADE         AS Nationality,
                                    PF.ITF_ENVIADA           AS ItfSent,
                                    PF.LOCALEMISSAO          AS IssuePlace,
                                    PF.CNH                   AS Cnh,
                                    PF.NOMEMAE               AS MothersName,
                                    (SELECT CONCAT(T.AREA,T.TELEFONE) 
	                                    FROM NEWUNIK.T_TELEFONE_PESSOA TP 
	                                    INNER JOIN NEWUNIK.T_TELEFONE T ON T.IDTELEFONE = TP.IDTELEFONE 
	                                    WHERE T.TIPOTELEFONE = 'X' AND TP.IDPESSOA = PF.IDPESSOAFISICA AND ROWNUM = 1) AS PhoneNumber,
                                    CT.IDFILIALFUNCIONARIO   AS BranchId,
                                    CT.IDCENTROCUSTO         AS CostCenterId,
                                    CT.STATUSIMPCONTRATO     AS ContractImpStatus,
                                    CT.ULTIMADATAIMPCONTRATO AS ContractImpLastDate,
                                    CT.IDLOJA                AS Convenio,
                                    CT.NOME_CARTAO           AS Name,
                                    CT.STATUS_CLIENTE        AS Status,
                                    CT.IDULTIMOTIPOBLOQUEIO  AS BlockTypeId,
                                    CT.IDLOJACADASTRO        AS OriginalConvenio,
                                    CT.IDENDERECORESIDENCIAL AS HomeAddressId,
                                    CT.CAMPOAUX1             AS RegistrationNumber,
                                    (SELECT CONCAT(T.AREA,T.TELEFONE) 
	FROM NEWUNIK.T_TELEFONE_PESSOA TP 
	INNER JOIN NEWUNIK.T_TELEFONE T ON T.IDTELEFONE = TP.IDTELEFONE 
	WHERE T.TIPOTELEFONE = 'X' AND TP.IDPESSOA = PF.IDPESSOAFISICA AND ROWNUM = 1) AS PhoneNumber,
                                    E.BAIRRO                 AS Neighborhood,
                                    E.NUMERO                 AS AddressNumber,
                                    E.LOGRADOURO             AS Street,
                                    E.LOCALIDADE             AS CityName,
                                    E.UF                     AS State,
                                    E.CEP                    AS ZipCode,
                                    E.COMPLEMENTO            AS AddressComplement,
                                    LI.LIMITEAPLICADO/100        AS CardLimit,
                                    LI.DATA                  AS CardLimitUpdatedAt,
                                    CL.NUMEROGERADO          AS CardNumber,
                                    CL.STATUS                AS CardStatus
                            FROM NEWUNIK.T_PESSOAFISICA PF
                                JOIN NEWUNIK.T_CLIENTETITULARPESSOAFISICA CT ON PF.IDPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA
                                LEFT JOIN NEWUNIK.T_ENDERECO E ON E.IDENDERECO  = CT.IDENDERECORESIDENCIAL
                                JOIN NEWUNIK.t_cartaoloja CL ON CL.IDCLIENTEPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA
                                JOIN NEWUNIK.t_limitecredito LI ON LI.IDCLIENTETITULARPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA AND LI.IDLIMITECREDITO = (SELECT max(tl2.IDLIMITECREDITO) FROM NEWUNIK.T_LIMITECREDITO tl2 WHERE tl2.IDCLIENTETITULARPESSOAFISICA = LI.IDCLIENTETITULARPESSOAFISICA)
                            WHERE PF.IDPESSOAFISICA = :Id 
                            AND CT.IDLOJA = :Convenio";

            using (var conn = new DbSession(ConnectionString).Connection)
            {
                return (await conn.QueryFirstOrDefaultAsync<Account>(query, new
                {
                    Id = id,
                    Convenio = convenio
                }));
            }
        }


        public async Task<StatusDismissal> GetStatusDismissal(string cpf)
        {
            string query = @" SELECT PF.CPF,
                                CT.NOME_CARTAO AS NAME,
                                PF.EMAIL,
                                CT.IDCLIENTETITULARPESSOAFISICA AS PERSONID,
                                BC.IDBLOQUEIOCLIENTE AS IDBLOCK, 
                                BC.""DATA"",
                                BC.OBSERVACAO AS OBSERVATION,
                                BC.IDTIPOBLOQUEIOCLIENTE AS IDTYPEBLOCK,
                                CL.STATUS AS CARDSTATUS
                                FROM NEWUNIK.T_PESSOAFISICA PF
                                JOIN NEWUNIK.T_CLIENTETITULARPESSOAFISICA CT ON PF.IDPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA
                                JOIN NEWUNIK.T_BLOQUEIOCLIENTE BC ON CT.IDCLIENTETITULARPESSOAFISICA = BC.IDCLIENTETITULARPESSOAFISICA 
                                JOIN NEWUNIK.T_CARTAOLOJA CL ON CL.IDCLIENTEPESSOAFISICA = CT.IDCLIENTETITULARPESSOAFISICA
                                WHERE PF.CPF = :Cpf
                                AND BC.IDTIPOBLOQUEIOCLIENTE IN (-3, -18)";                 
            using (var conn = new DbSession(ConnectionString).Connection)
            {
                return (await conn.QueryFirstOrDefaultAsync<StatusDismissal>(query, new
                {
                    Cpf = cpf
                }));
            }
        }

    }
}
