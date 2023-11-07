using Dapper;
using Dapper.Oracle;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Portal.API.Data;
using Portal.API.Entities;
using Portal.API.Interfaces.Repositories;

namespace Portal.API.Repositories 
{
    public class ProfileMenuPortalRepository : IProfileMenuPortalRepository
    {
        private readonly IConfiguration _config;

        private readonly string portalConnectionString;
        public ProfileMenuPortalRepository(IConfiguration config)
        {
            _config = config;
            portalConnectionString = _config.GetConnectionString("PortalConnection");
        }
        public async Task<ProfileMenu> Add(ProfileMenu profileMenu)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var parms = new OracleDynamicParameters();
            parms.Add(":ProfileId", profileMenu.ProfileId, OracleMappingType.Double, ParameterDirection.Input);
            parms.Add(":MenuId", profileMenu.MenuId, OracleMappingType.Double, ParameterDirection.Input);
            parms.Add(":p_CanInsert", profileMenu.CanInsert ? 1 : 0, OracleMappingType.Int16, ParameterDirection.Input);
            parms.Add(":p_CanDelete", profileMenu.CanDelete ? 1 : 0, OracleMappingType.Int16, ParameterDirection.Input);
            parms.Add(":p_CanUpdate", profileMenu.CanUpdate ? 1 : 0, OracleMappingType.Int16, ParameterDirection.Input);

            string query = @"INSERT INTO PORTALRH.T_PERFIL_MENU (
                    MENU_ID,
                    PERFIL_ID, 
                    ID_INCLUIR, 
                    ID_EXCLUIR, 
                    ID_ALTERAR
                ) VALUES (
                    :MenuId,
                    :ProfileId, 
                    :p_CanInsert, 
                    :p_CanDelete, 
                    :p_CanUpdate)";
            await conn.ExecuteAsync(query, parms);
            return profileMenu;
        }

        public async Task Delete(long profileId)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"DELETE FROM PORTALRH.T_PERFIL_MENU WHERE PERFIL_ID = :ProfileId";
            await conn.ExecuteAsync(query, new { ProfileId = profileId });
        }


        public async Task<IEnumerable<ProfileMenu>> GetByProfile(long profileId)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var query = "SELECT MENU_ID as MenuId, PERFIL_ID as ProfileId, ID_INCLUIR as CanInsert, ID_EXCLUIR as CanDelete, ID_ALTERAR as CanUpdate from PORTALRH.T_PERFIL_MENU where PERFIL_ID = :ProfileId";
            return (await conn.QueryAsync<ProfileMenu>(query, new { ProfileId = profileId })).ToList();
        }

    }
}
