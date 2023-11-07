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
    public class BannerRepository : IBannerRepository
    {
        private readonly IConfiguration _config;
        private readonly string portalConnectionString;

        public BannerRepository(IConfiguration config)
        {
            _config = config;
            portalConnectionString = _config.GetConnectionString("PortalConnection");
        }

        public async Task<Banner> Create(Banner banner)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var parms = new OracleDynamicParameters();
            parms.Add(":p_Title", banner.Title, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_Description", banner.Description, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_Action", banner.Action, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_Link", banner.Link, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_Image", banner.Image, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_DateStart", banner.DateStart, OracleMappingType.Date, ParameterDirection.Input);
            parms.Add(":p_DateEnd", banner.DateEnd, OracleMappingType.Date, ParameterDirection.Input);
            parms.Add(":p_Status", banner.Status, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_Order", banner.Order, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":Id", null, OracleMappingType.Double, ParameterDirection.Output);

            string query = @"INSERT INTO PORTALRH.T_BANNER (ID, TITULO, DESCRICAO, ACAO, LINK, IMAGEM, DATA_INICIO, DATA_TERMINO, SITUACAO, ORDEM) VALUES(S_BANNER.nextval, :p_Title, :p_Description,:p_Action, :p_Link, :p_Image, :p_DateStart, :p_DateEnd, :p_Status, :p_Order)  returning ID into :Id";
            await conn.ExecuteAsync(query, parms);
            banner.Id = parms.Get<long>("Id");
            return banner;
        }

        public async Task Delete(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"DELETE FROM PORTALRH.T_BANNER WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id });
        }

        public async Task Activate(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_BANNER SET SITUACAO=:Status WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id, Status = "ativo" });
        }
        public async Task Deactivate(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_BANNER SET SITUACAO=:Status WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id, Status = "inativo" });
        }

        public async Task<List<Banner>> GetActiveBanners()
        {
            using var conn = new DbSession(_config.GetConnectionString("PortalConnection")).Connection;
            string query = @"SELECT ID as Id, TITULO as Title, DESCRICAO as Description, ACAO as Action, LINK as Link, IMAGEM as Image, DATA_INICIO as DateStart, DATA_TERMINO as DateEnd, SITUACAO as Status, ORDEM as ""Order"" FROM PORTALRH.T_BANNER WHERE SITUACAO = 'ativo' AND DATA_INICIO <= trunc(sysdate) AND DATA_TERMINO >= trunc(sysdate) ORDER BY ORDEM";
            return (await conn.QueryAsync<Banner>(query)).ToList();
        }

        public async Task<Banner> Update(Banner banner)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_BANNER SET TITULO=:Title, DESCRICAO=:Description, ACAO=:Action, LINK=:Link, IMAGEM=:Image, DATA_INICIO=:DateStart, DATA_TERMINO=:DateEnd, SITUACAO=:Status, ORDEM =:p_Order WHERE ID=:Id";
            await conn.ExecuteAsync(query, new { Id = banner.Id, Title = banner.Title, Description = banner.Description, Action = banner.Action, Link = banner.Link, Image = banner.Image, DateStart = banner.DateStart, DateEnd = banner.DateEnd, Status = banner.Status, p_Order = banner.Order });
            return banner;
        }

        public async Task<ResponseDto<Banner>> GetAllPaged(int limit, int skip, string search, string order)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string filter = "1 = 1";
                string orderBy = "ORDEM ASC";
                if (!String.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    filter = $"(LOWER(TITULO) LIKE '%{search}%' OR LOWER(DESCRICAO) LIKE '%{search}%')";
                }
                if (!String.IsNullOrEmpty(order))
                {
                    var part = order.Split(' ');
                    switch (part[0].ToLower())
                    {
                        case "id":
                            orderBy = "ID " + part[1];
                            break;
                        case "title":
                            orderBy = "TITULO " + part[1];
                            break;
                        case "description":
                            orderBy = "DESCRICAO " + part[1];
                            break;
                        case "dateStart":
                            orderBy = "DATA_INICIO " + part[1];
                            break;
                        case "dateEnd":
                            orderBy = "DATA_TERMINO " + part[1];
                            break;
                    }
                }
                string countquery = $@"SELECT count(*) AS COUNT FROM PORTALRH.T_BANNER WHERE {filter}";
                string basequery = $@"SELECT ID as Id, TITULO as Title, DESCRICAO as Description, ACAO as Action, LINK as Link, IMAGEM as Image, DATA_INICIO as DateStart, DATA_TERMINO as DateEnd, SITUACAO as Status, ORDEM as ""Order"" FROM PORTALRH.T_BANNER WHERE {filter} ORDER BY {orderBy}";
                string query = $"SELECT b.* FROM(SELECT a.*, rownum r__ FROM ( {basequery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";
                var count = await conn.QueryFirstOrDefaultAsync<long>(countquery);
                var records = await conn.QueryAsync<Banner>(query, new
                {
                    pageNumber = (skip / limit) + 1,
                    pageSize = limit
                });
                return new ResponseDto<Banner>
                {
                    CurrentPage = (skip / limit) + 1,
                    Data = records.ToList(),
                    PerPage = limit,
                    Total = count
                };
            }
        }

        public async Task<IEnumerable<Banner>> GetAll()
        {
            using var conn = new DbSession(_config.GetConnectionString("PortalConnection")).Connection;
            string query = @"SELECT ID as Id, TITULO as Title, DESCRICAO as Description, ACAO as Action, LINK as Link, IMAGEM as Image, DATA_INICIO as DateStart, DATA_TERMINO as DateEnd, SITUACAO as Status, ORDEM as ""Order"" FROM PORTALRH.T_BANNER";
            return (await conn.QueryAsync<Banner>(query)).ToList();
        }

        public async Task<Banner> Get(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"SELECT  ID as Id, TITULO as Title, DESCRICAO as Description, ACAO as Action, LINK as Link, IMAGEM as Image, DATA_INICIO as DateStart, DATA_TERMINO as DateEnd, SITUACAO as Status, ORDEM as ""Order"" FROM PORTALRH.T_BANNER WHERE ID = :Id ORDER BY ID ASC";
            return await conn.QueryFirstOrDefaultAsync<Banner>(query, new { Id = id });
        }
    }
}
