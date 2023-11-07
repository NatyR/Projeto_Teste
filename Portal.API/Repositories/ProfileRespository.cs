using Dapper;
using Dapper.Oracle;
using Microsoft.Extensions.Configuration;
using Portal.API.Common.Dto;
using Portal.API.Data;
using Portal.API.Entities;
using Portal.API.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Repositories
{
    public class ProfileRepositoryPortal : IProfileRepositoryPortal
    {
        private readonly IConfiguration _config;

        private readonly string portalConnectionString;
        public ProfileRepositoryPortal(IConfiguration config)
        {
            _config = config;
            portalConnectionString = _config.GetConnectionString("PortalConnection");
        }
        public async Task<ResponseDto<ProfilePortal>> GetAllPaged(int limit, int skip, string search, string order)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string filter = "1 = 1";
                string orderBy = "P.ID ASC";
                if (!String.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    filter = $"(LOWER(P.DESCRICAO) LIKE '%{search}%')";
                }
                if (!String.IsNullOrEmpty(order))
                {
                    var part = order.Split(' ');
                    switch (part[0].ToLower())
                    {
                        case "id":
                            orderBy = "P.ID " + part[1];
                            break;
                        case "description":
                            orderBy = "P.DESCRICAO " + part[1];
                            break;
                        case "sistemaId":
                            orderBy = "S.DESCRICAO " + part[1];
                            break;
                    }
                }
                string countquery = $@"SELECT count(*) AS COUNT FROM PORTALRH.T_PERFIL P WHERE SITUACAO = 'ATIVO' AND {filter}";
                string basequery = $@"SELECT P.ID AS Id, P.DESCRICAO AS Description, P.SISTEMA_ID AS SistemaId, S.ID as SId, S.DESCRICAO as SDescription, OBSERVACAO as SObservation FROM PORTALRH.T_PERFIL P LEFT JOIN PORTALRH.T_SISTEMA S ON S.ID = P.SISTEMA_ID WHERE  SITUACAO = 'ATIVO' AND {filter} ORDER BY {orderBy}";
                string query = $"SELECT b.Id,b.Description,b.SistemaId,b.SId as Id,b.SDescription as Description, b.SObservation as Observation FROM(SELECT a.*, rownum r__ FROM ( {basequery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";
                var count = await conn.QueryFirstOrDefaultAsync<long>(countquery);
                var records = await conn.QueryAsync<ProfilePortal, Sistema, ProfilePortal>(query,
                    (profile, sistema) =>
                    {
                        profile.Sistema = sistema;
                        return profile;
                    },
                    new
                    {
                        pageNumber = (skip / limit) + 1,
                        pageSize = limit
                    },
                    splitOn: "Id");
                return new ResponseDto<ProfilePortal>
                {
                    CurrentPage = (skip / limit) + 1,
                    Data = records.ToList(),
                    PerPage = limit,
                    Total = count
                };
            }
        }
        public async Task<ProfilePortal> Create(ProfilePortal profile)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var parms = new OracleDynamicParameters();
            parms.Add(":Observation", profile.Observation, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":Description", profile.Description, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":SistemaId", profile.SistemaId, OracleMappingType.Double, ParameterDirection.Input);
            parms.Add(":Id", null, OracleMappingType.Double, ParameterDirection.Output);

            string query = @"INSERT INTO PORTALRH.T_PERFIL (ID, DESCRICAO, SISTEMA_ID, OBSERVACAO) VALUES(S_PERFIL.nextval, :Description, :SistemaId, :Observation)  returning ID into :Id";
            await conn.ExecuteAsync(query, parms);
            profile.Id = parms.Get<long>("Id");
            return profile;

        }

        public async Task Delete(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_PERFIL SET SITUACAO = 'DESATIVADO'  WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id });
        }

        public async Task<ProfilePortal> Get(long id)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT ID AS Id, DESCRICAO AS Description, SISTEMA_ID AS SistemaId, OBSERVACAO as Observation FROM PORTALRH.T_PERFIL WHERE SITUACAO = 'ATIVO' AND ID = :Id";
                return (await conn.QueryFirstOrDefaultAsync<ProfilePortal>(query, new { Id = id }));
            }
        }

        public async Task<IEnumerable<ProfilePortal>> GetAll()
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT ID AS Id, DESCRICAO AS Description, SISTEMA_ID AS SistemaId, OBSERVACAO as Observation FROM PORTALRH.T_PERFIL WHERE SITUACAO = 'ATIVO' ORDER BY ID ASC";
                return await conn.QueryAsync<ProfilePortal>(query);
            }
        }
        public async Task<IEnumerable<ProfilePortal>> GetBySistema(long sistemaId)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT ID AS Id, DESCRICAO AS Description, SISTEMA_ID AS SistemaId, OBSERVACAO as Observation FROM PORTALRH.T_PERFIL WHERE SITUACAO = 'ATIVO' AND SISTEMA_ID = :SistemaId ORDER BY ID ASC";
                return await conn.QueryAsync<ProfilePortal>(query, new { SistemaId = sistemaId });
            }
        }

        public async Task<ProfilePortal> Update(ProfilePortal profile)
        {
            using var conn = new DbSession(portalConnectionString).Connection;

            string query = @"UPDATE PORTALRH.T_PERFIL SET DESCRICAO = :Description, SISTEMA_ID = :SistemaId, OBSERVACAO = :Observation WHERE ID = :Id";
            await conn.ExecuteAsync(query, profile);
            return profile;

        }
    }
}
