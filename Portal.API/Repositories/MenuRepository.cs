using Dapper;
using Dapper.Oracle;
using Microsoft.Extensions.Configuration;
using Portal.API.Common.Dto;
using Portal.API.Data;
using Portal.API.Dto.Menu;
using Portal.API.Entities;
using Portal.API.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly IConfiguration _config;
        private readonly string portalConnectionString;

        public MenuRepository(IConfiguration config)
        {
            _config = config;
            portalConnectionString = _config.GetConnectionString("PortalConnection");
        }

        public async Task<Menu> Create(Menu menu)
        {
            if (menu.Order < 1)
            {
                menu.Order = 1;
            }
            using var conn = new DbSession(portalConnectionString).Connection;
            var parms = new OracleDynamicParameters();
            parms.Add(":p_Name", menu.Name, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_Tipo", menu.Type, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_Parent", menu.ParentId, OracleMappingType.Double, ParameterDirection.Input);
            parms.Add(":p_Icone", menu.Icon, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_Order", menu.Order, OracleMappingType.Double, ParameterDirection.Input);
            parms.Add(":p_SistemaId", menu.SistemaId, OracleMappingType.Double, ParameterDirection.Input);
            parms.Add(":p_Url", menu.Url, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_CanInsert", menu.CanInsert ? 1 : 0, OracleMappingType.Int16, ParameterDirection.Input);
            parms.Add(":p_CanDelete", menu.CanDelete ? 1 : 0, OracleMappingType.Int16, ParameterDirection.Input);
            parms.Add(":p_CanUpdate", menu.CanUpdate ? 1 : 0, OracleMappingType.Int16, ParameterDirection.Input);
            parms.Add(":Id", null, OracleMappingType.Double, ParameterDirection.Output);

            string query = @"INSERT INTO PORTALRH.T_MENU (ID, NOME, TIPO, PARENT, ICONE, ORDEM, SISTEMA_ID, URL, ID_INCLUIR, ID_EXCLUIR, ID_ALTERAR) VALUES(S_MENU.nextval, :p_Name, :p_Tipo, :p_Parent, :p_Icone, :p_Order, :p_SistemaId, :p_Url, :p_CanInsert, :p_CanDelete, :p_CanUpdate) returning ID into :Id";
            await conn.ExecuteAsync(query, parms);
            menu.Id = parms.Get<long>("Id");
            await Reorder(menu);

            return menu;
        }

        public async Task Delete(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string queryUpdate = @"UPDATE PORTALRH.T_MENU SET PARENT = NULL WHERE PARENT = :Id";
            string queryDeletePermissions = @"DELETE FROM PORTALRH.T_PERFIL_MENU WHERE MENU_ID = :Id";
            string queryDelete = @"DELETE FROM PORTALRH.T_MENU WHERE ID = :Id";
            await conn.ExecuteAsync(queryUpdate, new { Id = id });
            await conn.ExecuteAsync(queryDeletePermissions, new { Id = id });
            await conn.ExecuteAsync(queryDelete, new { Id = id });
        }

        //public async Task<List<Menu>> GetUserMenuByConvenio()
        //{
        //    using var conn = new DbSession(portalConnectionString).Connection;
        //    var query = @"SELECT ID, NOME, TIPO, PARENT, ICONE, ORDEM, SISTEMA_ID, URL FROM PORTALRH.T_MENU";
        //    return (await conn.QueryAsync<Menu>(query)).ToList();
        //}

        public async Task<List<Menu>> GetBySistemaId(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var query = @"SELECT ID as Id, NOME as Name, TIPO as Type, PARENT as ParentId, ICONE as Icon, ORDEM as ""Order"", SISTEMA_ID as SistemaId, URL as Url, ID_INCLUIR as CanInsert, ID_EXCLUIR as CanDelete, ID_ALTERAR as CanUpdate   FROM PORTALRH.T_MENU where SISTEMA_ID = :SistemaId order by PARENT, ORDEM ";
            return (await conn.QueryAsync<Menu>(query, new { SistemaId = id })).ToList();
        }
        public async Task<List<Menu>> GetByUserAndConvenio(long user_id, long sistema_id, long profile_id, long? convenio_id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var query = @"SELECT DISTINCT M.ID as Id, M.NOME as Name, M.TIPO as Type, M.PARENT as ParentId, M.ICONE as Icon, M.ORDEM as ""Order"", M.SISTEMA_ID as SistemaId, M.URL  as Url , CASE WHEN M.TIPO = 'GRUPO' THEN 0 ELSE PM.ID_INCLUIR END as CanInsert, CASE WHEN M.TIPO = 'GRUPO' THEN 0 ELSE PM.ID_EXCLUIR END as CanDelete, CASE WHEN M.TIPO = 'GRUPO' THEN 0 ELSE PM.ID_ALTERAR END as CanUpdate
                            FROM PORTALRH.T_USUARIO U
                            LEFT JOIN PORTALRH.T_USUARIO_CONVENIO UC ON UC.USUARIO_ID = U.ID AND UC.CONVENIO_ID = :ConvenioId
                            INNER JOIN PORTALRH.T_PERFIL P on ((P.ID = U.PERFIL_ID AND U.ID_TIPO in (1,2)) OR (P.ID = UC.PERFIL_ID AND U.ID_TIPO not in (1,2)))
                            INNER JOIN PORTALRH.T_PERFIL_MENU PM on PM.PERFIL_ID = P.ID
                            INNER JOIN PORTALRH.T_MENU PMM on PMM.ID = PM.MENU_ID
                            INNER JOIN PORTALRH.T_MENU M on M.SISTEMA_ID = P.SISTEMA_ID and (M.ID = PMM.ID OR M.ID = PMM.PARENT)
                            WHERE U.ID = :UserId AND P.SISTEMA_ID = :SistemaId 
                            ORDER BY M.ORDEM, M.PARENT";

            return (await conn.QueryAsync<Menu>(query, new { SistemaId = sistema_id, UserId = user_id, ProfileId = profile_id, ConvenioId = convenio_id })).ToList();
        }
        public async Task<Menu> Update(Menu menu)
        {
            if (menu.Order < 1)
            {
                menu.Order = 1;
            }
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_MENU SET NOME=:Name, TIPO=:p_Type, PARENT=:Parent, ICONE=:Icon, ORDEM=:p_Order, SISTEMA_ID=:SistemaId, URL=:Url, ID_INCLUIR=:p_CanInsert, ID_EXCLUIR=:p_CanDelete, ID_ALTERAR=:p_CanUpdate WHERE ID=:Id";
            await conn.ExecuteAsync(query, new { Id = menu.Id, Name = menu.Name, p_Type = menu.Type, Parent = menu.ParentId, Icon = menu.Icon, p_Order = menu.Order, SistemaId = menu.SistemaId, Url = menu.Url, p_CanInsert = menu.CanInsert ? 1 : 0, p_CanDelete = menu.CanDelete ? 1 : 0, p_CanUpdate = menu.CanUpdate ? 1 : 0 });
            await Reorder(menu);
            return menu;
        }

        private async Task Reorder(Menu menu)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var query = @"SELECT ID as Id, NOME as Name, TIPO as Type, PARENT as ParentId, ICONE as Icon, ORDEM as ""Order"", SISTEMA_ID as SistemaId, URL as Url, ID_INCLUIR as CanInsert, ID_EXCLUIR as CanDelete, ID_ALTERAR as CanUpdate   FROM PORTALRH.T_MENU where SISTEMA_ID = :SistemaId and PARENT = :ParentId order by PARENT, ORDEM ";
            if(menu.ParentId == null)
            {
                query = @"SELECT ID as Id, NOME as Name, TIPO as Type, PARENT as ParentId, ICONE as Icon, ORDEM as ""Order"", SISTEMA_ID as SistemaId, URL as Url, ID_INCLUIR as CanInsert, ID_EXCLUIR as CanDelete, ID_ALTERAR as CanUpdate   FROM PORTALRH.T_MENU where SISTEMA_ID = :SistemaId and PARENT IS NULL order by PARENT, ORDEM ";
            }
            var queryUpdate = @"UPDATE PORTALRH.T_MENU set ORDEM = :Ordem where ID = :Id";
            var menus = (await conn.QueryAsync<Menu>(query, new { SistemaId = menu.SistemaId, ParentId = menu.ParentId })).ToList();
            
            var nrOrdem = 1;
            foreach(var m in menus.Where(m => m.Id != menu.Id))
            {
                if(nrOrdem == menu.Order)
                {
                    nrOrdem++;
                }
                await conn.ExecuteAsync(queryUpdate, new
                {
                    Id = m.Id,
                    Ordem = nrOrdem,
                });
                nrOrdem++;
            }
            

        }

        public async Task<ResponseDto<Menu>> GetAllPaged(int limit, int skip, string order, MenuFilterDto filterData)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string filter = "1 = 1";
                string orderBy = "M.ID ASC";
                if (!String.IsNullOrEmpty(filterData.SearchTerm))
                {
                    filterData.SearchTerm = filterData.SearchTerm.ToLower();
                    filter = $"(LOWER(M.NOME) LIKE '%{filterData.SearchTerm}%' OR LOWER(P.NOME) LIKE '%{filterData.SearchTerm}%')";
                }
                if (!String.IsNullOrEmpty(order))
                {
                    var part = order.Split(' ');
                    switch (part[0].ToLower())
                    {
                        case "id":
                            orderBy = "M.ID " + part[1];
                            break;
                        case "name":
                            orderBy = "M.NOME " + part[1];
                            break;
                        case "parent.name":
                            orderBy = "P.NOME " + part[1];
                            break;
                        case "sistema.description":
                            orderBy = "S.DESCRICAO " + part[1];
                            break;
                        case "order":
                            orderBy = "M.ORDEM " + part[1];
                            break;
                    }
                }
                if (!String.IsNullOrEmpty(filterData.Type))
                {
                    filter += " AND M.TIPO = :MenuType";
                }
                if(filterData.SistemaId != null)
                {
                    filter += " AND M.SISTEMA_ID = :SistemaId";                
                }
                if(filterData.GroupId != null && filterData.GroupId > 0)
                {
                    filter += " AND M.PARENT = :GroupId";
                } else if(filterData.GroupId == 0)
                {
                    filter += " AND M.PARENT IS NULL";
                }
                string countquery = $@"SELECT count(*) AS COUNT FROM PORTALRH.T_MENU M LEFT JOIN PORTALRH.T_MENU P ON P.ID = M.PARENT LEFT JOIN PORTALRH.T_SISTEMA S ON S.ID = M.SISTEMA_ID   WHERE {filter}";
                string basequery = $@"SELECT M.ID as Id, M.NOME as Name, M.TIPO as Type, M.PARENT as ParentId, M.ICONE as Icon, M.ORDEM as ""Order"", M.SISTEMA_ID as SistemaId, M.URL  as Url, P.ID as IdParent, P.NOME as NameParent, P.TIPO as TypeParent, P.PARENT as ParentIdParent, P.ICONE as IconParent, P.ORDEM as ""OrderParent"", P.SISTEMA_ID as SistemaIdParent, P.URL  as UrlParent, S.ID AS IdSistema, S.DESCRICAO AS DescriptionSistema, M.ID_INCLUIR as CanInsert, M.ID_EXCLUIR as CanDelete, M.ID_ALTERAR as CanUpdate FROM PORTALRH.T_MENU M LEFT JOIN PORTALRH.T_MENU P ON P.ID = M.PARENT LEFT JOIN PORTALRH.T_SISTEMA S ON S.ID = M.SISTEMA_ID WHERE {filter} ORDER BY {orderBy}";
                string query = $@"SELECT b.Id as Id, b.Name as Name, b.Type as ""Type"", b.ParentId as ParentId, b.Icon as Icon, b.""Order"" as ""Order"", b.SistemaId as SistemaId, b.Url  as Url, b.IdParent as Id, b.NameParent as Name, b.TypeParent as Type, b.ParentIdParent as ParentId, b.IconParent as Icon, b.""OrderParent"" as ""Order"", b.SistemaIdParent as SistemaId, b.UrlParent  as Url, b.IdSistema AS Id, b.DescriptionSistema AS Description, b.CanInsert as CanInsert, b.CanDelete as CanDelete, b.CanUpdate as CanUpdate FROM(SELECT a.*, rownum r__ FROM ( {basequery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";
                var count = await conn.QueryFirstOrDefaultAsync<long>(countquery,
                    new
                    {
                        MenuType = filterData.Type,
                        SistemaId =filterData.SistemaId,
                        GroupId = filterData.GroupId,
                    });
                var records = await conn.QueryAsync<Menu, Menu, Sistema, Menu>(query,
                    (m, p, s) => { m.Parent = p; m.Sistema = s; return m; },
                    new
                    {
                        MenuType = filterData.Type,
                        SistemaId = filterData.SistemaId,
                        GroupId = filterData.GroupId,
                        pageNumber = (skip / limit) + 1,
                        pageSize = limit
                    },
                    splitOn: "Id,Id,Id");
                return new ResponseDto<Menu>
                {
                    CurrentPage = (skip / limit) + 1,
                    Data = records.ToList(),
                    PerPage = limit,
                    Total = count
                };
            }
        }

        public async Task<Menu> Get(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var query = @"SELECT ID as Id, NOME as Name, TIPO as Type, PARENT as ParentId, ICONE as Icon, ORDEM as ""Order"", SISTEMA_ID as SistemaId, URL  as Url, ID_INCLUIR as CanInsert, ID_EXCLUIR as CanDelete, ID_ALTERAR as CanUpdate FROM PORTALRH.T_MENU where ID = :Id ";
            return await conn.QueryFirstOrDefaultAsync<Menu>(query, new { Id = id });
        }
    }
}
