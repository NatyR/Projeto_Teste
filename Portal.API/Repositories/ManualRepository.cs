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
    public class ManualRepository : IManualRepository
    {
        private readonly IConfiguration _config;
        private readonly string portalConnectionString;

        public ManualRepository(IConfiguration config)
        {
            _config = config;
            portalConnectionString = _config.GetConnectionString("PortalConnection");
        }

        public async Task<Manual> Create(Manual manual)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var parms = new OracleDynamicParameters();
            parms.Add(":p_Name", manual.Name, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_FileName", manual.FileName, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_Order", manual.Order, OracleMappingType.Double, ParameterDirection.Input);
            parms.Add(":p_ManualType", manual.ManualType, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_Url", manual.Url, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_Description", manual.Description, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_Status", manual.Status, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":Id", null, OracleMappingType.Double, ParameterDirection.Output);

            string query = @"INSERT INTO PORTALRH.T_MANUAL (ID, NOME, ARQUIVO, ORDEM, SITUACAO, TIPO, URL, DESCRICAO) VALUES(S_MANUAL.nextval, :p_Name, :p_FileName, :p_Order, :p_Status, :p_ManualType, :p_Url, :p_Description) returning ID into :Id";
            await conn.ExecuteAsync(query, parms);
            manual.Id = parms.Get<long>("Id");
            return manual;
        }

        public async Task Delete(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"DELETE FROM PORTALRH.T_MANUAL WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id });
        }

        public async Task<Manual> Get(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var query = @"SELECT ID as Id, NOME as Name, ARQUIVO as FileName, SITUACAO as Status, ORDEM as ""Order"", TIPO as ManualType, URL as Url, DESCRICAO as Description FROM PORTALRH.T_MANUAL WHERE ID = :Id";
            return (await conn.QueryAsync<Manual>(query,new {Id = id})).FirstOrDefault();
        }

        public async Task<List<Manual>> GetActiveManuals()
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var query = @"SELECT ID as Id, NOME as Name, ARQUIVO as FileName, SITUACAO as Status, ORDEM as ""Order"", TIPO as ManualType, URL as Url, DESCRICAO as Description FROM PORTALRH.T_MANUAL WHERE SITUACAO = 'ativo' ORDER BY ORDEM";
            return (await conn.QueryAsync<Manual>(query)).ToList();
        }

        public async Task<IEnumerable<Manual>> GetAll()
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var query = @"SELECT ID as Id, NOME as Name, ARQUIVO as FileName, SITUACAO as Status, ORDEM as ""Order"", TIPO as ManualType, URL as Url, DESCRICAO as Description FROM PORTALRH.T_MANUAL ORDER BY ORDEM";
            return (await conn.QueryAsync<Manual>(query)).ToList();
        }

        public async Task<ResponseDto<Manual>> GetAllPaged(string manualType, int limit, int skip, string search, string order)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string filter = "TIPO = :ManualType ";
                string orderBy = "ID ASC";
                if (!String.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    filter = filter +  $" AND (LOWER(NOME) LIKE '%{search}%')";
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
                        case "status":
                            orderBy = "SITUACAO " + part[1];
                            break;
                        case "order":
                            orderBy = "ORDEM " + part[1];
                            break;
                    }
                }
                string countquery = $@"SELECT count(*) AS COUNT FROM PORTALRH.T_MANUAL WHERE {filter}";
                string basequery = $@"SELECT ID as Id, NOME as Name, ARQUIVO as FileName, SITUACAO as Status, ORDEM as ""Order"", TIPO as ManualType, URL as Url, DESCRICAO as Description FROM PORTALRH.T_MANUAL WHERE {filter} ORDER BY {orderBy}";
                string query = $"SELECT b.* FROM(SELECT a.*, rownum r__ FROM ( {basequery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";
                var count = await conn.QueryFirstOrDefaultAsync<long>(countquery, new { ManualType = manualType });
                var records = await conn.QueryAsync<Manual>(query, new
                {
                    ManualType = manualType,
                    pageNumber = (skip / limit) + 1,
                    pageSize = limit
                });
                return new ResponseDto<Manual>
                {
                    CurrentPage = (skip / limit) + 1,
                    Data = records.ToList(),
                    PerPage = limit,
                    Total = count
                };
            }
        }

        public async Task<Manual> Update(Manual manual)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_MANUAL SET NOME=:Name, ARQUIVO=:FileName, ORDEM=:p_Order, TIPO=:ManualType, URL=:Url, DESCRICAO=:Description WHERE ID=:Id";
            await conn.ExecuteAsync(query, new { Id = manual.Id, Name = manual.Name, FileName = manual.FileName, p_Order = manual.Order, ManualType = manual.ManualType, Url = manual.Url, Description = manual.Description });
            return manual;

        }
        public async Task Activate(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_MANUAL SET SITUACAO=:Status WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id, Status = "ativo" });
        }
        public async Task Deactivate(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_MANUAL SET SITUACAO=:Status WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id, Status = "inativo" });
        }


    }
}
