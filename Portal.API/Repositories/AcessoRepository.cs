using Portal.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Portal.API.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Portal.API.Interfaces.Repositories;
using Dapper.Oracle;
using System.Data;
using Portal.API.Common.Dto;
using System.Net;

namespace Portal.API.Repositories
{
    public class AcessoRepository : IAcessoRepository
    {
        private readonly IConfiguration _config;

        private readonly string portalConnectionString;
        public AcessoRepository(IConfiguration config)
        {
            _config = config;
            portalConnectionString = _config.GetConnectionString("PortalConnection");
        }

        public async Task<Acesso> Add(Acesso acesso)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var parms = new OracleDynamicParameters();
            parms.Add(":Ip", acesso.IP, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":Url", acesso.Url, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":Method", acesso.Method, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":PostData", acesso.PostData, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":UserId", acesso.UsuarioId, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":CreatedAt", acesso.CreatedAt, OracleMappingType.Date, ParameterDirection.Input);
            parms.Add(":Id", null, OracleMappingType.Double, ParameterDirection.Output);

            string query = @"INSERT INTO PORTALRH.T_ACESSO (
                    ID,
                    USUARIO_ID, 
                    URL,
                    IP,
                    METODO,
                    POSTDATA,
                    DATA_CADASTRO
                ) VALUES (
                    S_ACESSO.nextval, 
                    :UserId,
                    :Url,
                    :Ip,
                    :Method,
                    :PostData,
                    :CreatedAt)  returning ID into :Id";
            await conn.ExecuteAsync(query, parms);
            acesso.SetId(parms.Get<long>("Id"));
            return acesso;

        }

        public async Task Delete(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"DELETE FROM PORTALRH.T_ACESSO WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id });
        }
        
        public async Task<Acesso> Get(long id)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT ID as Id, SISTEMA_ID as SistemaId, USUARIO_ID as UsuarioId, IP as Ip, URL as Url, METODO as Method, POSTDATA as PostData, DATA_CADASTRO as CreatedAt
                                    FROM PORTALRH.T_ACESSO;
                                 WHERE ID = :Id";
                return (await conn.QueryFirstOrDefaultAsync<Acesso>(query, new { Id = id }));
            }
        }

        public async Task<IEnumerable<Acesso>> GetAll()
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT ID as Id, SISTEMA_ID as SistemaId, USUARIO_ID as UsuarioId, IP as Ip, URL as Url, METODO as Method, POSTDATA as PostData, DATA_CADASTRO as CreatedAt FROM PORTALRH.T_ACESSO";
                return await conn.QueryAsync<Acesso>(query);
            }
        }

        public async Task<ResponseDto<Acesso>> GetAllPaged(int limit, int skip, string search, string order)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string filter = "1 = 1";
                string orderBy = "ID ASC";
                if (!String.IsNullOrEmpty(search))
                {
                    filter = $"(IP LIKE '%{search}%' OR IP URL '%{search}%' OR METODO OR TO_CHAR(USUARIO_ID) = '{search}')";
                }
                if (!String.IsNullOrEmpty(order))
                {
                    var part = order.Split(' ');
                    switch (part[0].ToLower())
                    {
                        case "id":
                            orderBy = "A.ID " + part[1];
                            break;
                        case "nome":
                            orderBy = "tu.nome " + part[1];
                            break;
                        case "cpf":
                            orderBy = "tu.cpf " + part[1];
                            break;
                        case "sistemaid":
                            orderBy = "A.SISTEMA_ID " + part[1];
                            break;
                        case "ip":
                            orderBy = "A.IP " + part[1];
                            break;
                        case "url":
                            orderBy = "A.URL " + part[1];
                            break;
                        case "metodo":
                            orderBy = "A.METODO " + part[1];
                            break;
                        case "createdat":
                            orderBy = "A.DATA_CADASTRO " + part[1];
                            break;
                        case "userid":
                            orderBy = "A.USUARIO_ID " + part[1];
                            break;
                    }
                }
                string countquery = $"SELECT count(*) AS COUNT FROM PORTALRH.T_ACESSO A WHERE {filter}";
                string basequery = $@"SELECT A.ID as Id, A.USUARIO_ID as UsuarioId, A.IP as Ip, A.URL as Url, A.METODO as Method, A.POSTDATA as PostData, A.DATA_CADASTRO as CreatedAt, tu.nome as Nome, tu.cpf as Cpf FROM PORTALRH.T_ACESSO A LEFT JOIN PORTALRH.T_USUARIO tu ON A.usuario_id = tu.id WHERE {filter} ORDER BY {orderBy}";
                string query = $"SELECT b.* FROM(SELECT a.*, rownum r__ FROM ( {basequery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b  WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";
                var count = await conn.QueryFirstOrDefaultAsync<long>(countquery);
                var logins = await conn.QueryAsync<Acesso>(query,new
                {
                    pageNumber = (skip/limit) +1,
                    pageSize = limit
                });
                return new ResponseDto<Acesso>
                {
                    CurrentPage = (skip / limit) + 1,
                    Data = logins.ToList(),
                    PerPage = limit,
                    Total = count
                };
            }
        }

       
        public async Task<Acesso> GetById(long id)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT ID as Id, SISTEMA_ID as SistemaId, USUARIO_ID as UsuarioId, IP as Ip, URL as Url, METODO as Method, POSTDATA as PostData, DATA_CADASTRO as CreatedAt from PORTALRH.T_ACESSO A WHERE A.ID = " + id ;
                var login= await conn.QueryFirstOrDefaultAsync<Acesso>(query);
                return login;
            }
        }
        
       
    }
}
