using Users.API.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Users.API.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Users.API.Interfaces.Repositories;
using Dapper.Transaction;
using Dapper.Oracle;
using System.Data;

namespace Users.API.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly IConfiguration _config;

        private readonly string portalConnectionString;
        public ProfileRepository(IConfiguration config)
        {
            _config = config;
            portalConnectionString = _config.GetConnectionString("PortalConnection");
        }

        public async Task<Profile> Add(Profile profile)
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
            string query = @"DELETE FROM PORTALRH.T_PERFIL WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id });
        }

        public async Task<Profile> Get(long id)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT ID AS Id, DESCRICAO AS Description, SISTEMA_ID AS SistemaId, OBSERVACAO as Observation FROM PORTALRH.T_PERFIL WHERE ID = :Id";
                return (await conn.QueryFirstOrDefaultAsync<Profile>(query, new { Id = id }));
            }
        }

        public async Task<IEnumerable<Profile>> GetAll()
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT ID AS Id, DESCRICAO AS Description, SISTEMA_ID AS SistemaId, OBSERVACAO as Observation FROM PORTALRH.T_PERFIL ORDER BY ID ASC";
                return await conn.QueryAsync<Profile>(query);
            }
        }
        public async Task<IEnumerable<Profile>> GetBySistema(long sistemaId)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT ID AS Id, DESCRICAO AS Description, SISTEMA_ID AS SistemaId, OBSERVACAO as Observation FROM PORTALRH.T_PERFIL WHERE SISTEMA_ID = :SistemaId ORDER BY ID ASC";
                return await conn.QueryAsync<Profile>(query, new { SistemaId = sistemaId });
            }
        }

        public async Task<Profile> Update(Profile profile)
        {
            using var conn = new DbSession(portalConnectionString).Connection;

            string query = @"UPDATE PORTALRH.T_PERFIL SET DESCRICAO = :Description, SISTEMA_ID = :SistemaId, OBSERVACAO = :Observation WHERE ID = :Id";
            await conn.ExecuteAsync(query, profile);
            return profile;

        }
    }
}
