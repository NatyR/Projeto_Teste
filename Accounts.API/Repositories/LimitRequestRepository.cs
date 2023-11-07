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

namespace Accounts.API.Repositories
{
    public class LimitRequestRepository : ILimitRequestRepository
    {
        private readonly IConfiguration _config;

        private readonly string ConnectionString;
        public LimitRequestRepository(IConfiguration config)
        {
            _config = config;
            ConnectionString = _config.GetConnectionString("PortalConnection");
        }

        public async Task<ResponseDto<LimitRequest>> GetAllPaged(long convenio_id, string status, int limit, int skip, string search, string order)
        {
            using (var conn = new DbSession(ConnectionString).Connection)
            {
                string filter = "1 = 1";
                string orderBy = "DATA_SITUACAO DESC";
                if (!String.IsNullOrEmpty(status))
                {
                    filter += $" AND SITUACAO in :Status ";
                }
                if (!String.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    filter += $" AND (LOWER(MATRICULA) LIKE '%{search}%' OR LOWER(NOME) LIKE '%{search}%' OR LOWER(CPF) LIKE '%{search}%') ";
                }
                if (!String.IsNullOrEmpty(order))
                {
                    var part = order.Split(' ');
                    switch (part[0].ToLower())
                    {
                        case "id":
                            orderBy = "ID " + part[1];
                            break;
                        case "name":
                            orderBy = "NOME " + part[1];
                            break;
                        case "cpf":
                            orderBy = "CPF " + part[1];
                            break;
                        case "status":
                            orderBy = "SITUACAO " + part[1];
                            break;
                        case "createdat":
                            orderBy = "DATA_CADASTRO " + part[1];
                            break;
                        case "statuschangedat":
                            orderBy = "DATA_SITUACAO " + part[1];
                            break;
                    }
                }
                string countquery = $@"SELECT count(*) AS COUNT 
                                        FROM PORTALRH.T_SOLICITACAO_LIMITE
                                        WHERE CONVENIO_ID = :Convenio AND {filter}";
                string basequery = $@"SELECT    ID                  AS Id, 
                                                MATRICULA           AS RegistrationNumber,  
                                                NOME                AS Name,     
                                                CPF                 AS Cpf,  
                                                LIMITE_ANTERIOR     AS PreviousLimit,  
                                                LIMITE_NOVO         AS NewLimit,  
                                                USUARIO_ID          AS UserId,  
                                                SITUACAO            AS Status,  
                                                DATA_SITUACAO       AS StatusChangedAt,  
                                                APROVADOR_ID        AS ApproverId,  
                                                DATA_CADASTRO       AS CreatedAt,  
                                                CONVENIO_ID         AS ShopId 
                                    FROM PORTALRH.T_SOLICITACAO_LIMITE
                                    WHERE CONVENIO_ID = :Convenio 
                                    AND {filter} ORDER BY {orderBy}";
                string query = $"SELECT b.* FROM(SELECT a.*, rownum r__ FROM ( {basequery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";
                var count = await conn.QueryFirstOrDefaultAsync<long>(countquery, new { Convenio = convenio_id, Status = status.Split(',') });
                var data = await conn.QueryAsync<LimitRequest>(query, new
                {
                    Convenio = convenio_id,
                    Status = status.Split(','),
                    pageNumber = (skip / limit) + 1,
                    pageSize = limit
                });
                return new ResponseDto<LimitRequest>
                {
                    CurrentPage = (skip / limit) + 1,
                    Data = data.ToList(),
                    PerPage = limit,
                    Total = count
                };
            }
        }
        public async Task<LimitRequest> GetById(long id)
        {
            using (var conn = new DbSession(ConnectionString).Connection)
            {
                string query = $@"SELECT    ID                  AS Id, 
                                            MATRICULA           AS RegistrationNumber,  
                                            NOME                AS Name,     
                                            CPF                 AS Cpf,  
                                            LIMITE_ANTERIOR     AS PreviousLimit,  
                                            LIMITE_NOVO         AS NewLimit,  
                                            USUARIO_ID          AS UserId,  
                                            SITUACAO            AS Status,  
                                            DATA_SITUACAO       AS StatusChangedAt,  
                                            APROVADOR_ID        AS ApproverId,  
                                            DATA_CADASTRO       AS CreatedAt,  
                                            CONVENIO_ID         AS ShopId 
                                    FROM PORTALRH.T_SOLICITACAO_LIMITE
                                    WHERE ID = :Id";
                return await conn.QueryFirstOrDefaultAsync<LimitRequest>(query, new
                {
                    Id = id
                });
            }
        }
        public async Task<LimitRequest> Create(LimitRequest limitRequest)
        {
            try
            {

                using var conn = new DbSession(ConnectionString).Connection;
                var parms = new OracleDynamicParameters();

                parms.Add(":p_RegistrationNumber", limitRequest.RegistrationNumber, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_Name", limitRequest.Name, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_Cpf", limitRequest.Cpf, OracleMappingType.Varchar2, ParameterDirection.Input);
                parms.Add(":p_PreviousLimit", limitRequest.PreviousLimit, OracleMappingType.Double, ParameterDirection.Input);
                parms.Add(":p_NewLimit", limitRequest.NewLimit, OracleMappingType.Double, ParameterDirection.Input);
                parms.Add(":p_ShopId", limitRequest.ShopId, OracleMappingType.Double, ParameterDirection.Input);
                parms.Add(":p_UserId", limitRequest.UserId, OracleMappingType.Double, ParameterDirection.Input);

                parms.Add(":NewId", null, OracleMappingType.Double, ParameterDirection.Output);


                string query = @"INSERT INTO PORTALRH.T_SOLICITACAO_LIMITE (ID, CONVENIO_ID, MATRICULA, NOME, CPF, LIMITE_ANTERIOR, LIMITE_NOVO, USUARIO_ID) VALUES(S_SOLICITACAO_LIMITE.nextval, :p_ShopId, :p_RegistrationNumber, :p_Name, :p_Cpf, :p_PreviousLimit, :p_NewLimit, :p_UserId) returning ID into :NewId";
                await conn.ExecuteAsync(query, parms);
                limitRequest.Id = parms.Get<long>("NewId");
                return limitRequest;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task Approve(long id, long userId)
        {
            using var conn = new DbSession(ConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_SOLICITACAO_LIMITE SET SITUACAO=:Status, APROVADOR_ID = :ApproverId, DATA_SITUACAO = CURRENT_TIMESTAMP WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id, Status = "APROVADO", ApproverId = userId });
        }
        public async Task Reject(long id, long userId)
        {
            using var conn = new DbSession(ConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_SOLICITACAO_LIMITE SET SITUACAO=:Status, APROVADOR_ID = :ApproverId, DATA_SITUACAO = CURRENT_TIMESTAMP WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id, Status = "REPROVADO", ApproverId = userId });
        }
    }
}
