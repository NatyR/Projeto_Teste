using Dapper;
using Dapper.Oracle;
using Microsoft.Extensions.Configuration;

using Accounts.API.Data;
using Accounts.API.Entities;
using Accounts.API.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Accounts.API.Common.Dto;
using Accounts.API.Common.Dto.Solicitation;
using Amazon.S3.Model;
using Maoli;

namespace Accounts.API.Repositories
{
    public class SolicitationRepository : ISolicitationRepository
    {
        private readonly IConfiguration _config;

        private readonly string ConnectionString;
        public SolicitationRepository(IConfiguration config)
        {
            _config = config;
            ConnectionString = _config.GetConnectionString("PortalConnection");
        }
        public async Task<ResponseDto<Solicitation>> GetAllPagedByConvenio(long convenio_id, SolicitationFilterDto filterOptions, int limit, int skip, string order)
        {
            using (var conn = new DbSession(ConnectionString).Connection)
            {
                string filter = "1 = 1";
                string orderBy = "S.ID DESC";
                if (!String.IsNullOrEmpty(filterOptions.SolicitationStatus))
                {
                    filter += $" AND S.SITUACAO in :Status ";
                }
                if (!String.IsNullOrEmpty(filterOptions.SearchTerm))
                {
                    filterOptions.SearchTerm = filterOptions.SearchTerm.ToLower();
                    //remove non numeric characters
                    var cpf = new string(filterOptions.SearchTerm.Where(c => char.IsDigit(c)).ToArray());
                    if (cpf.Length > 0)
                    {
                        filter += $" AND (LOWER(S.NOME) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(S.CPF) LIKE '%{cpf}%') ";
                    }
                    else
                    {
                        filter += $" AND (LOWER(S.NOME) LIKE '%{filterOptions.SearchTerm}%') ";
                    }

                }
                if (filterOptions.SolicitationType > 0)
                {
                    filter += $" AND S.TIPO_SOLICITACAO_ID = :SolicitationType ";
                }
                if (filterOptions.SolicitationStartDate != null)
                {
                    filter += $" AND S.DATA_SOLICITACAO >= :SolicitationStartDate ";
                }
                if (filterOptions.SolicitationEndDate != null)
                {
                    filterOptions.SolicitationEndDate = filterOptions.SolicitationEndDate.Value.AddDays(1);
                    filter += $" AND S.DATA_SOLICITACAO <= :SolicitationEndDate ";
                }
                if (!String.IsNullOrEmpty(order))
                {
                    var part = order.Split(' ');
                    switch (part[0].ToLower())
                    {
                        case "id":
                            orderBy = "S.ID " + part[1];
                            break;
                        case "name":
                            orderBy = "S.NOME " + part[1];
                            break;
                        case "cpf":
                            orderBy = "S.CPF " + part[1];
                            break;
                        case "status":
                            orderBy = "S.SITUACAO " + part[1];
                            break;
                        case "requestedat":
                            orderBy = "S.DATA_SOLICITACAO " + part[1];
                            break;
                        case "requestype":
                            orderBy = "S.TIPO_SOLICITACAO " + part[1];
                            break;
                    }
                }
                string countquery = $@"SELECT count(*) AS COUNT 
                                        FROM PORTALRH.T_SOLICITACAO S
                                        WHERE S.CONVENIO_ID = :Convenio AND {filter}";
                string basequery = $@"SELECT    S.ID                  AS Id, 
                                                S.TIPO_SOLICITACAO_ID AS SolicitationType,
                                                S.DATA_SOLICITACAO    AS RequestedAt,
                                                S.DATA_APROVACAO      AS ApprovedAt,
                                                S.USUARIO_ID          AS UserId,  
                                                S.SITUACAO            AS Status,  
                                                S.OBSERVACAO          AS Observation,
                                                S.CLIENTE_ID          AS ClientId,  
                                                S.CONVENIO_ID         AS ShopId ,
                                                S.NOME_CONVENIO       AS ShopName,
                                                S.CNPJ_CONVENIO       AS ShopDocument,
                                                S.GRUPO_ID            AS GroupId ,
                                                S.NOME_GRUPO          AS GroupName,
                                                S.NOME                AS Name,     
                                                S.EMAIL               AS Email,     
                                                S.TELEFONE            AS Phone,     
                                                S.CPF                 AS Cpf,  
                                                S.LIMITE_ANTERIOR     AS PreviousLimit,  
                                                S.LIMITE_NOVO         AS NewLimit,  
                                                S.TIPO_BLOQUEIO_ID    AS BlockTypeId,
                                                S.TIPO_BLOQUEIO       AS BlockType,
                                                S.SITUACAO_CONTA      AS AccountStatus,
                                                S.STATUS_CARTAO       AS CardStatus,
                                                S.RG                  AS Rg,
                                                S.DATA_NASCIMENTO     AS BirthDate,
                                                S.DATA_ADMISSAO       AS AdmissionDate,
                                                S.CEP                 AS ZipCode,
                                                S.ENDERECO            AS Street,
                                                S.NUMERO              AS AddressNumber,
                                                S.COMPLEMENTO         AS AddressComplement,
                                                S.BAIRRO              AS Neighborhood,
                                                S.CIDADE              AS CityName,
                                                S.UF                  AS State,
                                                S.CENTROCUSTO_ID      AS CostCenterId,
                                                S.NOME_CENTROCUSTO    AS CostCenterName,
                                                S.FILIAL_ID           AS BranchId,
                                                S.NOME_FILIAL         AS BranchName,
                                                U.NOME                AS UserName,
                                                CASE WHEN U.ID_TIPO = 1 THEN 'ADMINISTRADOR' WHEN U.ID_TIPO = 2 THEN 'MASTER' WHEN U.ID_TIPO = 3 THEN 'CONVENCIONAL' END AS UserType,
                                                TS.NOME               AS SolicitationTypeDescription
                                    FROM PORTALRH.T_SOLICITACAO S
                                    LEFT JOIN PORTALRH.T_USUARIO U on U.ID = S.USUARIO_ID
                                    LEFT JOIN PORTALRH.T_TIPO_SOLICITACAO TS on TS.ID = S.TIPO_SOLICITACAO_ID
                                    WHERE S.CONVENIO_ID = :Convenio 
                                    AND {filter} ORDER BY {orderBy}";
                string query = $"SELECT b.* FROM(SELECT a.*, rownum r__ FROM ( {basequery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";
                var count = await conn.QueryFirstOrDefaultAsync<long>(countquery, new
                {
                    Convenio = convenio_id,
                    Status = filterOptions.SolicitationStatus.Split(','),
                    SolicitationType = filterOptions.SolicitationType,
                    SolicitationStartDate = filterOptions.SolicitationStartDate,
                    SolicitationEndDate = filterOptions.SolicitationEndDate
                });
                var data = await conn.QueryAsync<Solicitation>(query, new
                {
                    Convenio = convenio_id,
                    Status = filterOptions.SolicitationStatus.Split(','),
                    SolicitationType = filterOptions.SolicitationType,
                    SolicitationStartDate = filterOptions.SolicitationStartDate,
                    SolicitationEndDate = filterOptions.SolicitationEndDate,
                    pageNumber = (skip / limit) + 1,
                    pageSize = limit
                });
                return new ResponseDto<Solicitation>
                {
                    CurrentPage = (skip / limit) + 1,
                    Data = data.ToList(),
                    PerPage = limit,
                    Total = count
                };
            }
        }
        public async Task<ResponseDto<Solicitation>> GetAllPaged(SolicitationFilterDto filterOptions, int limit, int skip, string order)
        {
            using (var conn = new DbSession(ConnectionString).Connection)
            {
                string filter = "1 = 1";
                string orderBy = "S.ID DESC";
                if (!String.IsNullOrEmpty(filterOptions.SolicitationStatus))
                {
                    filter += $" AND S.SITUACAO in :Status ";
                }
                if (!String.IsNullOrEmpty(filterOptions.SearchTerm))
                {
                    filterOptions.SearchTerm = filterOptions.SearchTerm.ToLower();
                    //remove non numeric characters
                    var document = new string(filterOptions.SearchTerm.Where(c => char.IsDigit(c)).ToArray());
                    if (document.Length > 0)
                    {
                        filter += $" AND (LOWER(S.NOME) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(U.NOME) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(S.NOME_GRUPO) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(S.NOME_CONVENIO) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(S.CNPJ_CONVENIO) LIKE '%{document}%' OR LOWER(S.CPF) LIKE '%{document}%' OR REGEXP_REPLACE(U.CPF, '[^0-9]+', '') LIKE '%{document}%') ";
                    }
                    else
                    {
                        filter += $" AND (LOWER(S.NOME) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(U.NOME) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(S.NOME_GRUPO) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(S.NOME_CONVENIO) LIKE '%{filterOptions.SearchTerm}%') ";
                    }
                }
                if (filterOptions.GroupId!= null && filterOptions.GroupId.Length > 0)
                {
                    filter += " AND S.GRUPO_ID = ANY :Groups ";
                }
                if (filterOptions.ShopId != null && filterOptions.ShopId.Length > 0)
                {
                    filter += " AND S.CONVENIO_ID = ANY :Shops ";
                }

                if (filterOptions.UserType > 0)
                {
                    filter += " AND U.ID_TIPO = :UserType ";
                }
                if (filterOptions.SolicitationType > 0)
                {
                    filter += $" AND S.TIPO_SOLICITACAO_ID = :SolicitationType ";
                }
                if (filterOptions.SolicitationStartDate != null)
                {
                    filter += $" AND S.DATA_SOLICITACAO >= :SolicitationStartDate ";
                }
                if (filterOptions.SolicitationEndDate != null)
                {
                    filterOptions.SolicitationEndDate = filterOptions.SolicitationEndDate.Value.AddDays(1);
                    filter += $" AND S.DATA_SOLICITACAO <= :SolicitationEndDate ";
                }
                if (!String.IsNullOrEmpty(order))
                {
                    var part = order.Split(' ');
                    switch (part[0].ToLower())
                    {
                        case "id":
                            orderBy = "S.ID " + part[1];
                            break;
                        case "name":
                            orderBy = "S.NOME " + part[1];
                            break;
                        case "cpf":
                            orderBy = "S.CPF " + part[1];
                            break;
                        case "status":
                            orderBy = "S.SITUACAO " + part[1];
                            break;
                        case "requestedat":
                            orderBy = "S.DATA_SOLICITACAO " + part[1];
                            break;
                        case "requestype":
                            orderBy = "S.TIPO_SOLICITACAO " + part[1];
                            break;
                    }
                }
                string countquery = $@"SELECT count(*) AS COUNT 
                                        FROM PORTALRH.T_SOLICITACAO S
                                        LEFT JOIN PORTALRH.T_USUARIO U on U.ID = S.USUARIO_ID
                                        LEFT JOIN PORTALRH.T_TIPO_SOLICITACAO TS on TS.ID = S.TIPO_SOLICITACAO_ID
                                        WHERE {filter}";
                string basequery = $@"SELECT    S.ID                  AS Id, 
                                                S.TIPO_SOLICITACAO_ID AS SolicitationType,
                                                S.DATA_SOLICITACAO    AS RequestedAt,
                                                S.DATA_APROVACAO      AS ApprovedAt,
                                                S.USUARIO_ID          AS UserId,  
                                                S.SITUACAO            AS Status,  
                                                S.OBSERVACAO          AS Observation,
                                                S.CLIENTE_ID          AS ClientId,  
                                                S.CONVENIO_ID         AS ShopId ,
                                                S.NOME_CONVENIO       AS ShopName,
                                                S.CNPJ_CONVENIO       AS ShopDocument,
                                                S.GRUPO_ID            AS GroupId ,
                                                S.NOME_GRUPO          AS GroupName,
                                                S.NOME                AS Name,     
                                                S.EMAIL               AS Email,     
                                                S.TELEFONE            AS Phone,     
                                                S.CPF                 AS Cpf,  
                                                S.LIMITE_ANTERIOR     AS PreviousLimit,  
                                                S.LIMITE_NOVO         AS NewLimit,  
                                                S.TIPO_BLOQUEIO_ID    AS BlockTypeId,
                                                S.TIPO_BLOQUEIO       AS BlockType,
                                                S.SITUACAO_CONTA      AS AccountStatus,
                                                S.STATUS_CARTAO       AS CardStatus,
                                                S.RG                  AS Rg,
                                                S.DATA_NASCIMENTO     AS BirthDate,
                                                S.DATA_ADMISSAO       AS AdmissionDate,
                                                S.CEP                 AS ZipCode,
                                                S.ENDERECO            AS Street,
                                                S.NUMERO              AS AddressNumber,
                                                S.COMPLEMENTO         AS AddressComplement,
                                                S.BAIRRO              AS Neighborhood,
                                                S.CIDADE              AS CityName,
                                                S.UF                  AS State,
                                                S.CENTROCUSTO_ID      AS CostCenterId,
                                                S.NOME_CENTROCUSTO    AS CostCenterName,
                                                S.FILIAL_ID           AS BranchId,
                                                S.NOME_FILIAL         AS BranchName,
                                                U.NOME                AS UserName,
                                                CASE WHEN U.ID_TIPO = 1 THEN 'ADMINISTRADOR' WHEN U.ID_TIPO = 2 THEN 'MASTER' WHEN U.ID_TIPO = 3 THEN 'CONVENCIONAL' END AS UserType,
                                                TS.NOME               AS SolicitationTypeDescription
                                    FROM PORTALRH.T_SOLICITACAO S
                                    LEFT JOIN PORTALRH.T_USUARIO U on U.ID = S.USUARIO_ID
                                    LEFT JOIN PORTALRH.T_TIPO_SOLICITACAO TS on TS.ID = S.TIPO_SOLICITACAO_ID
                                    WHERE {filter} ORDER BY {orderBy}";
                string query = $"SELECT b.* FROM(SELECT a.*, rownum r__ FROM ( {basequery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";
                var count = await conn.QueryFirstOrDefaultAsync<long>(countquery, new
                {
                    UserType = filterOptions.UserType,
                    Groups = filterOptions.GroupId,
                    Shops = filterOptions.ShopId,
                    Status = filterOptions.SolicitationStatus.Split(','),
                    SolicitationType = filterOptions.SolicitationType,
                    SolicitationStartDate = filterOptions.SolicitationStartDate,
                    SolicitationEndDate = filterOptions.SolicitationEndDate
                });
                var data = await conn.QueryAsync<Solicitation>(query, new
                {
                    UserType = filterOptions.UserType,
                    Groups = filterOptions.GroupId,
                    Shops = filterOptions.ShopId,
                    Status = filterOptions.SolicitationStatus.Split(','),
                    SolicitationType = filterOptions.SolicitationType,
                    SolicitationStartDate = filterOptions.SolicitationStartDate,
                    SolicitationEndDate = filterOptions.SolicitationEndDate,
                    pageNumber = (skip / limit) + 1,
                    pageSize = limit
                });
                return new ResponseDto<Solicitation>
                {
                    CurrentPage = (skip / limit) + 1,
                    Data = data.ToList(),
                    PerPage = limit,
                    Total = count
                };
            }
        }
        public async Task<List<Solicitation>> GetAll(SolicitationFilterDto filterOptions)
        {
            using (var conn = new DbSession(ConnectionString).Connection)
            {
                string filter = "1 = 1";
                string orderBy = "S.ID DESC";
                if (!String.IsNullOrEmpty(filterOptions.SolicitationStatus))
                {
                    filter += $" AND S.SITUACAO in :Status ";
                }
                if (!String.IsNullOrEmpty(filterOptions.SearchTerm))
                {
                    filterOptions.SearchTerm = filterOptions.SearchTerm.ToLower();
                    //remove non numeric characters
                    var document = new string(filterOptions.SearchTerm.Where(c => char.IsDigit(c)).ToArray());
                    if (document.Length > 0)
                    {
                        filter += $" AND (LOWER(S.NOME) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(U.NOME) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(S.NOME_GRUPO) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(S.NOME_CONVENIO) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(S.CNPJ_CONVENIO) LIKE '%{document}%' OR LOWER(S.CPF) LIKE '%{document}%' OR REGEXP_REPLACE(U.CPF, '[^0-9]+', '') LIKE '%{document}%') ";
                    }
                    else
                    {
                        filter += $" AND (LOWER(S.NOME) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(U.NOME) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(S.NOME_GRUPO) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(S.NOME_CONVENIO) LIKE '%{filterOptions.SearchTerm}%') ";
                    }

                }
                if (filterOptions.GroupId != null && filterOptions.GroupId.Length > 0)
                {
                    filter += " AND S.GRUPO_ID = ANY :Groups ";
                }
                if (filterOptions.ShopId != null && filterOptions.ShopId.Length > 0)
                {
                    filter += " AND S.CONVENIO_ID = ANY :Shops ";
                }
                if (filterOptions.UserType > 0)
                {
                    filter += " AND U.ID_TIPO = :UserType ";
                }
                if (filterOptions.SolicitationType > 0)
                {
                    filter += $" AND S.TIPO_SOLICITACAO_ID = :SolicitationType ";
                }
                if (filterOptions.SolicitationStartDate != null)
                {
                    filter += $" AND S.DATA_SOLICITACAO >= :SolicitationStartDate ";
                }
                if (filterOptions.SolicitationEndDate != null)
                {
                    filterOptions.SolicitationEndDate = filterOptions.SolicitationEndDate.Value.AddDays(1);
                    filter += $" AND S.DATA_SOLICITACAO <= :SolicitationEndDate ";
                }


                string query = $@"SELECT    S.ID                  AS Id, 
                                                S.TIPO_SOLICITACAO_ID AS SolicitationType,
                                                S.DATA_SOLICITACAO    AS RequestedAt,
                                                S.DATA_APROVACAO      AS ApprovedAt,
                                                S.USUARIO_ID          AS UserId,  
                                                S.SITUACAO            AS Status,  
                                                S.OBSERVACAO          AS Observation,
                                                S.CLIENTE_ID          AS ClientId,  
                                                S.CONVENIO_ID         AS ShopId ,
                                                S.NOME_CONVENIO       AS ShopName,
                                                S.CNPJ_CONVENIO       AS ShopDocument,
                                                S.GRUPO_ID            AS GroupId ,
                                                S.NOME_GRUPO          AS GroupName,
                                                S.NOME                AS Name,     
                                                S.EMAIL               AS Email,     
                                                S.TELEFONE            AS Phone,     
                                                S.CPF                 AS Cpf,  
                                                S.LIMITE_ANTERIOR     AS PreviousLimit,  
                                                S.LIMITE_NOVO         AS NewLimit,  
                                                S.TIPO_BLOQUEIO_ID    AS BlockTypeId,
                                                S.TIPO_BLOQUEIO       AS BlockType,
                                                S.SITUACAO_CONTA      AS AccountStatus,
                                                S.STATUS_CARTAO       AS CardStatus,
                                                S.RG                  AS Rg,
                                                S.DATA_NASCIMENTO     AS BirthDate,
                                                S.DATA_ADMISSAO       AS AdmissionDate,
                                                S.CEP                 AS ZipCode,
                                                S.ENDERECO            AS Street,
                                                S.NUMERO              AS AddressNumber,
                                                S.COMPLEMENTO         AS AddressComplement,
                                                S.BAIRRO              AS Neighborhood,
                                                S.CIDADE              AS CityName,
                                                S.UF                  AS State,
                                                S.CENTROCUSTO_ID      AS CostCenterId,
                                                S.NOME_CENTROCUSTO    AS CostCenterName,
                                                S.FILIAL_ID           AS BranchId,
                                                S.NOME_FILIAL         AS BranchName,
                                                U.NOME                AS UserName,
                                                CASE WHEN U.ID_TIPO = 1 THEN 'ADMINISTRADOR' WHEN U.ID_TIPO = 2 THEN 'MASTER' WHEN U.ID_TIPO = 3 THEN 'CONVENCIONAL' END AS UserType,
                                                TS.NOME               AS SolicitationTypeDescription
                                    FROM PORTALRH.T_SOLICITACAO S
                                    LEFT JOIN PORTALRH.T_USUARIO U on U.ID = S.USUARIO_ID
                                    LEFT JOIN PORTALRH.T_TIPO_SOLICITACAO TS on TS.ID = S.TIPO_SOLICITACAO_ID
                                    WHERE {filter} ORDER BY {orderBy}";
                return (await conn.QueryAsync<Solicitation>(query, new
                {
                    UserType = filterOptions.UserType,
                    Groups = filterOptions.GroupId,
                    Shops = filterOptions.ShopId,
                    Status = filterOptions.SolicitationStatus.Split(','),
                    SolicitationType = filterOptions.SolicitationType,
                    SolicitationStartDate = filterOptions.SolicitationStartDate,
                    SolicitationEndDate = filterOptions.SolicitationEndDate
                })).ToList();

            }
        }
        public async Task<Solicitation> GetById(long id)
        {
            using (var conn = new DbSession(ConnectionString).Connection)
            {
                string query = $@"SELECT    ID                  AS Id, 
                                                TIPO_SOLICITACAO_ID AS SolicitationType,
                                                DATA_SOLICITACAO    AS RequestedAt,
                                                DATA_APROVACAO      AS ApprovedAt,
                                                USUARIO_ID          AS UserId,  
                                                SITUACAO            AS Status,  
                                                OBSERVACAO          AS Observation,
                                                CLIENTE_ID          AS ClientId,  
                                                CONVENIO_ID         AS ShopId ,
                                                NOME_CONVENIO       AS ShopName,
                                                CNPJ_CONVENIO       AS ShopDocument,
                                                GRUPO_ID            AS GroupId ,
                                                NOME_GRUPO          AS GroupName,
                                                NOME                AS Name,     
                                                EMAIL               AS Email,     
                                                TELEFONE            AS Phone,     
                                                CPF                 AS Cpf,  
                                                LIMITE_ANTERIOR     AS PreviousLimit,  
                                                LIMITE_NOVO         AS NewLimit,  
                                                TIPO_BLOQUEIO_ID    AS BlockTypeId,
                                                TIPO_BLOQUEIO       AS BlockType,
                                                SITUACAO_CONTA      AS AccountStatus,
                                                STATUS_CARTAO       AS CardStatus,
                                                RG                  AS Rg,
                                                DATA_NASCIMENTO     AS BirthDate,
                                                DATA_ADMISSAO       AS AdmissionDate,
                                                CEP                 AS ZipCode,
                                                ENDERECO            AS Street,
                                                NUMERO              AS AddressNumber,
                                                COMPLEMENTO         AS AddressComplement,
                                                BAIRRO              AS Neighborhood,
                                                CIDADE              AS CityName,
                                                UF                  AS State,
                                                CENTROCUSTO_ID      AS CostCenterId,
                                                NOME_CENTROCUSTO    AS CostCenterName,
                                                FILIAL_ID           AS BranchId,
                                                NOME_FILIAL         AS BranchName
                                    FROM PORTALRH.T_SOLICITACAO
                                    WHERE ID = :Id";
                return await conn.QueryFirstOrDefaultAsync<Solicitation>(query, new
                {
                    Id = id
                });
            }
        }
        public async Task<List<SolicitationType>> GetTypes()
        {
            using (var conn = new DbSession(ConnectionString).Connection)
            {
                string query = $@"SELECT    ID                  AS Id, 
                                                NOME AS Name
                                    FROM PORTALRH.T_TIPO_SOLICITACAO
                                    ORDER BY NOME";
                return (await conn.QueryAsync<SolicitationType>(query)).ToList();
            }
        }
        public async Task<Solicitation> Create(Solicitation solicitation)
        {
            try
            {

                using var conn = new DbSession(ConnectionString).Connection;
                var parms = new OracleDynamicParameters();

                parms.Add(":p_SolicitationType", solicitation.SolicitationType, OracleMappingType.Double, ParameterDirection.Input);
                parms.Add(":p_RequestedAt", solicitation.RequestedAt, OracleMappingType.Date, ParameterDirection.Input);
                parms.Add(":p_ApprovedAt", solicitation.ApprovedAt, OracleMappingType.Date, ParameterDirection.Input);
                parms.Add(":p_UserId", solicitation.UserId, OracleMappingType.Double, ParameterDirection.Input);
                parms.Add(":p_Status", solicitation.Status, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_Observation", solicitation.Observation, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_ClientId", solicitation.ClientId, OracleMappingType.Double, ParameterDirection.Input);
                parms.Add(":p_ShopId", solicitation.ShopId, OracleMappingType.Double, ParameterDirection.Input);
                parms.Add(":p_ShopName", solicitation.ShopName, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_ShopDocument", solicitation.ShopDocument, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_GropuId", solicitation.GroupId, OracleMappingType.Double, ParameterDirection.Input);
                parms.Add(":p_GroupName", solicitation.GroupName, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_Name", solicitation.Name, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_Email", solicitation.Email, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_Phone", solicitation.Phone, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_Cpf", solicitation.Cpf, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_PreviousLimit", solicitation.PreviousLimit, OracleMappingType.Double, ParameterDirection.Input);
                parms.Add(":p_NewLimit", solicitation.NewLimit, OracleMappingType.Double, ParameterDirection.Input);
                parms.Add(":p_BlockTypeId", solicitation.BlockTypeId, OracleMappingType.Double, ParameterDirection.Input);
                parms.Add(":p_BlockType", solicitation.BlockType, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_AccountStatus", solicitation.AccountStatus, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_CardStatus", solicitation.CardStatus, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_Rg", solicitation.Rg, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_BirthDate", solicitation.BirthDate, OracleMappingType.Date, ParameterDirection.Input);
                parms.Add(":p_AdmissionDate", solicitation.AdmissionDate, OracleMappingType.Date, ParameterDirection.Input);
                parms.Add(":p_ZipCode", solicitation.ZipCode, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_Street", solicitation.Street, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_AddressNumber", solicitation.AddressNumber, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_AddressComplement", solicitation.AddressComplement, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_Neighborhood", solicitation.Neighborhood, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_CityName", solicitation.CityName, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_State", solicitation.State, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_CostCenterId", solicitation.CostCenterId, OracleMappingType.Double, ParameterDirection.Input);
                parms.Add(":p_CostCenterName", solicitation.CostCenterName, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_BranchId", solicitation.BranchId, OracleMappingType.Double, ParameterDirection.Input);
                parms.Add(":p_BranchName", solicitation.BranchName, OracleMappingType.Varchar2, ParameterDirection.Input);


                parms.Add(":NewId", null, OracleMappingType.Double, ParameterDirection.Output);


                string query = @"INSERT INTO PORTALRH.T_SOLICITACAO (ID, 
                                                                    TIPO_SOLICITACAO_ID, 
                                                                    DATA_SOLICITACAO, 
                                                                    DATA_APROVACAO, 
                                                                    USUARIO_ID, 
                                                                    SITUACAO, 
                                                                    OBSERVACAO, 
                                                                    CLIENTE_ID, 
                                                                    CONVENIO_ID,
                                                                    NOME_CONVENIO,
                                                                    CNPJ_CONVENIO,
                                                                    GRUPO_ID,
                                                                    NOME_GRUPO,
                                                                    NOME, 
                                                                    EMAIL,
                                                                    TELEFONE,
                                                                    CPF, 
                                                                    LIMITE_ANTERIOR, 
                                                                    LIMITE_NOVO, 
                                                                    TIPO_BLOQUEIO_ID, 
                                                                    TIPO_BLOQUEIO, 
                                                                    SITUACAO_CONTA,
                                                                    STATUS_CARTAO,
                                                                    RG,
                                                                    DATA_NASCIMENTO,
                                                                    DATA_ADMISSAO,
                                                                    CEP,
                                                                    ENDERECO,
                                                                    NUMERO,
                                                                    COMPLEMENTO,
                                                                    BAIRRO,
                                                                    CIDADE,
                                                                    UF,
                                                                    CENTROCUSTO_ID,
                                                                    NOME_CENTROCUSTO,
                                                                    FILIAL_ID,
                                                                    NOME_FILIAL) 
                                VALUES (PORTALRH.S_SOLICITACAO.NEXTVAL, 
                                        :p_SolicitationType, 
                                        :p_RequestedAt, 
                                        :p_ApprovedAt, 
                                        :p_UserId, 
                                        :p_Status, 
                                        :p_Observation, 
                                        :p_ClientId, 
                                        :p_ShopId, 
                                        :p_ShopName,
                                        :p_ShopDocument,
                                        :p_GropuId,
                                        :p_GroupName,
                                        :p_Name, 
                                        :p_Email,
                                        :p_Phone,
                                        :p_Cpf, 
                                        :p_PreviousLimit, 
                                        :p_NewLimit, 
                                        :p_BlockTypeId, 
                                        :p_BlockType, 
                                        :p_AccountStatus,
                                        :p_CardStatus,
                                        :p_Rg,
                                        :p_BirthDate,
                                        :p_AdmissionDate,
                                        :p_ZipCode,
                                        :p_Street,
                                        :p_AddressNumber,
                                        :p_AddressComplement,
                                        :p_Neighborhood,
                                        :p_CityName,
                                        :p_State,
                                        :p_CostCenterId,
                                        :p_CostCenterName,
                                        :p_BranchId,
                                        :p_BranchName
                                ) 
                                RETURNING ID INTO :NewId";
                await conn.ExecuteAsync(query, parms);
                solicitation.Id = parms.Get<long>("NewId");
                return solicitation;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task RegisterError(long id, Solicitation solicitation)
        {
            using var conn = new DbSession(ConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_SOLICITACAO SET SITUACAO=:Status, OBSERVACAO = :Observation, TIPO_BLOQUEIO_ID = :BlockTypeId WHERE ID = :Id and SITUACAO = 'PENDENTE'";
            await conn.ExecuteAsync(query, new { Id = id, Status = "ERRO", Observation = solicitation.Observation, BlockTypeId = solicitation.BlockTypeId });
        }
        public async Task RegisterSuccess(long id, Solicitation solicitation)
        {
            using var conn = new DbSession(ConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_SOLICITACAO SET SITUACAO=:Status, OBSERVACAO = :Observation, TIPO_BLOQUEIO_ID = :BlockTypeId, STATUS_CARTAO = :CardStatus WHERE ID = :Id and SITUACAO = 'PENDENTE'";
            await conn.ExecuteAsync(query, new { Id = id, Status = "CONCLUIDO", Observation = solicitation.Observation, BlockTypeId = solicitation.BlockTypeId, CardStatus = solicitation.CardStatus });
        }
    }
}
