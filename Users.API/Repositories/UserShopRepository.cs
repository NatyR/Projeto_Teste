using Users.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users.API.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Users.API.Interfaces.Repositories;
using Dapper.Oracle;
using System.Data;
using Users.API.Common.Dto;

namespace Users.API.Repositories
{
    public class UserShopRepository : IUserShopRepository
    {
        private readonly IConfiguration _config;

        private readonly string portalConnectionString;
        public UserShopRepository(IConfiguration config)
        {
            _config = config;
            portalConnectionString = _config.GetConnectionString("PortalConnection");
        }
        public async Task<List<UserShop>> GetByUserId(long userId)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var query = "SELECT USUARIO_ID as UserId, CONVENIO_ID as ShopId, PERFIL_ID as ProfileId from PORTALRH.T_USUARIO_CONVENIO where USUARIO_ID = :UserId";
            return (await conn.QueryAsync<UserShop>(query,new {UserId = userId})).ToList();
        }
        public async Task<UserShop> Add(UserShop user)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var parms = new OracleDynamicParameters();
            parms.Add(":UserId", user.UserId, OracleMappingType.Double, ParameterDirection.Input);
            parms.Add(":ShopId", user.ShopId, OracleMappingType.Double, ParameterDirection.Input);
            parms.Add(":ProfileId", user.ProfileId, OracleMappingType.Double, ParameterDirection.Input);

            string query = @"INSERT INTO PORTALRH.T_USUARIO_CONVENIO (
                    USUARIO_ID,
                    CONVENIO_ID, 
                    PERFIL_ID
                ) VALUES (
                    :UserId,
                    :ShopId,
                    :ProfileId)";
            await conn.ExecuteAsync(query, parms);
            return user;

        }

        public async Task Delete(long userId)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"DELETE FROM PORTALRH.T_USUARIO_CONVENIO WHERE USUARIO_ID = :Id";
            await conn.ExecuteAsync(query, new { Id = userId });
        }

    }
}
