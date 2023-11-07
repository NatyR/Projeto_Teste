using Dapper;
using Microsoft.Extensions.Configuration;
using Portal.API.Data;
using Portal.API.Entities;
using Portal.API.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Dapper.Oracle;
using System.Data;
using Portal.API.Common.Dto;

namespace Portal.API.Repositories
{
    public class FaqRepository : IFaqRepository
    {
        private readonly IConfiguration _config;

        private readonly string portalConnectionString;
        public FaqRepository(IConfiguration config)
        {
            _config = config;
            portalConnectionString = _config.GetConnectionString("PortalConnection");
        }

        public async Task<Faq> Create(Faq faq)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var parms = new OracleDynamicParameters();
            parms.Add(":p_Question", faq.Question, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_Answer", faq.Answer, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_Order", faq.Order, OracleMappingType.Double, ParameterDirection.Input);
            parms.Add(":p_Status", faq.Status, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":Id", null, OracleMappingType.Double, ParameterDirection.Output);

            string query = @"INSERT INTO PORTALRH.T_FAQ (ID, PERGUNTA, RESPOSTA, ORDEM, SITUACAO) VALUES(S_FAQ.nextval, :p_Question, :p_Answer, :p_Order, :p_Status)  returning ID into :Id";
            await conn.ExecuteAsync(query, parms);
            faq.Id = parms.Get<long>("Id");
            return faq;

        }

        public async Task Delete(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"DELETE FROM PORTALRH.T_FAQ WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id });
        }

        public async Task<Faq> Get(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"SELECT ID, PERGUNTA as Question, RESPOSTA as Answer, ORDEM as ""Order"", SITUACAO as Status FROM PORTALRH.T_FAQ WHERE ID = :Id ORDER BY ID ASC";
            return await conn.QueryFirstOrDefaultAsync<Faq>(query, new { Id = id });
        }

        public async Task<IEnumerable<Faq>> GetAll()
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT ID, PERGUNTA as Question, RESPOSTA as Answer, ORDEM as ""Order"", SITUACAO as Status FROM PORTALRH.T_FAQ ORDER BY ORDEM ASC";
                return await conn.QueryAsync<Faq>(query);
            }
        }

        public async Task<Faq> Update(Faq faq)
        {
            using var conn = new DbSession(portalConnectionString).Connection;

            string query = @"UPDATE PORTALRH.T_FAQ SET PERGUNTA = :Question, RESPOSTA = :Answer, ORDEM = :p_Order WHERE ID = :Id";
            await conn.ExecuteAsync(query, new {Id = faq.Id,Question = faq.Question, Answer = faq.Answer, p_Order = faq.Order});
            return faq;

        }

        public async Task<List<Faq>> GetActiveFaqs()
        {
            using var conn = new DbSession(_config.GetConnectionString("PortalConnection")).Connection;
            var query = @"SELECT ID, PERGUNTA as Question, RESPOSTA as Answer, ORDEM as ""Order"", SITUACAO as Status FROM PORTALRH.T_FAQ WHERE SITUACAO = 'ativo' order by ORDEM";
            return (await conn.QueryAsync<Faq>(query)).ToList();
        }

        public async Task<List<Faq>> GetFaqsForReorder(int order)
        {
            using var conn = new DbSession(_config.GetConnectionString("PortalConnection")).Connection;
            var query = @"SELECT ID, PERGUNTA as Question, RESPOSTA as Answer, ORDEM as ""Order"", SITUACAO as Status FROM PORTALRH.T_FAQ WHERE ORDEM >= :p_Order order by ORDEM";
            return (await conn.QueryAsync<Faq>(query, new {p_Order = order})).ToList();
        }

        public async Task<ResponseDto<Faq>> GetAllPaged(int limit, int skip, string search, string order)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string filter = "1 = 1";
                string orderBy = "ORDEM ASC";
                if (!String.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    filter = $"(LOWER(PERGUNTA) LIKE '%{search}%' OR LOWER(RESPOSTA) LIKE '%{search}%')";
                }
                if (!String.IsNullOrEmpty(order))
                {
                    var part = order.Split(' ');
                    switch (part[0].ToLower())
                    {
                        case "id":
                            orderBy = "ID " + part[1];
                            break;
                        case "question":
                            orderBy = "PERGUNTA " + part[1];
                            break;
                        case "status":
                            orderBy = "SITUACAO " + part[1];
                            break;
                    }
                }
                string countquery = $@"SELECT count(*) AS COUNT FROM PORTALRH.T_FAQ WHERE {filter}";
                string basequery = $@"SELECT ID, PERGUNTA as Question, RESPOSTA as Answer, ORDEM as ""Order"", SITUACAO as Status FROM PORTALRH.T_FAQ WHERE {filter} ORDER BY {orderBy}";
                string query = $"SELECT b.* FROM(SELECT a.*, rownum r__ FROM ( {basequery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";
                var count = await conn.QueryFirstOrDefaultAsync<long>(countquery);
                var records = await conn.QueryAsync<Faq>(query, new
                {
                    pageNumber = (skip / limit) + 1,
                    pageSize = limit
                });
                return new ResponseDto<Faq>
                {
                    CurrentPage = (skip / limit) + 1,
                    Data = records.ToList(),
                    PerPage = limit,
                    Total = count
                };
            }
        }
        public async Task Activate(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_FAQ SET SITUACAO=:Status WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id, Status = "ativo" });
        }
        public async Task Deactivate(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_FAQ SET SITUACAO=:Status WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id, Status = "inativo" });
        }

    }
}
