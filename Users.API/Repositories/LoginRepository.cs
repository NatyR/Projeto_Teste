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
using System.Net;
using Users.API.Dto.Login;

namespace Users.API.Repositories
{
    public class LoginRepository : ILoginRepository
    {
        private readonly IConfiguration _config;

        private readonly string portalConnectionString;
        public LoginRepository(IConfiguration config)
        {
            _config = config;
            portalConnectionString = _config.GetConnectionString("PortalConnection");
        }

        public async Task<Login> Add(Login login)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var parms = new OracleDynamicParameters();
            parms.Add(":Ip", login.IP, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":Email", login.Email, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":UserAgent", login.UserAgent, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":Status", login.Status, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":UserId", login.UsuarioId, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":CreatedAt", login.CreatedAt, OracleMappingType.Date, ParameterDirection.Input);
            parms.Add(":Id", null, OracleMappingType.Double, ParameterDirection.Output);

            string query = @"INSERT INTO PORTALRH.T_LOGIN (
                    ID,
                    USUARIO_ID, 
                    EMAIL,
                    IP,
                    USERAGENT,
                    SITUACAO,
                    DATA_CADASTRO
                ) VALUES (
                    S_LOGIN.nextval, 
                    :UserId,
                    :Email,
                    :Ip,
                    :UserAgent,
                    :Status,
                    :CreatedAt)  returning ID into :Id";
            await conn.ExecuteAsync(query, parms);
            login.SetId(parms.Get<long>("Id"));
            return login;

        }

        public async Task Delete(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"DELETE FROM PORTALRH.T_LOGIN WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id });
        }
        
        public async Task<Login> Get(long id)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"
                    SELECT ID as Id,
                    USUARIO_ID as UserId,
                    EMAIL as Email, IP as IP, USERAGENT as UserAgent, SITUACAO as Status, DATA_CADASTRO as CreatedAt
                                FROM PORTALRH.T_LOGIN
                                 WHERE ID = :Id";
                return (await conn.QueryFirstOrDefaultAsync<Login>(query, new { Id = id }));
            }
        }

        public async Task<IEnumerable<Login>> GetAll()
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT ID as Id, USUARIO_ID as UserId, EMAIL as Email, IP as IP, USERAGENT as UserAgent, SITUACAO as Status, DATA_CADASTRO as CreatedAt FROM PORTALRH.T_LOGIN";
                return await conn.QueryAsync<Login>(query);
            }
        }

        public async Task<ResponseDto<Login>> GetAllPaged(LoginFilterDto filterOptions, int limit, int skip, string order)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string filter = "1 = 1";
                string orderBy = "ID ASC";
                if (!String.IsNullOrEmpty(filterOptions.SearchTerm))
                {
                    filter = $"(U.NOME LIKE '%{filterOptions.SearchTerm}%' OR  L.EMAIL LIKE '%{filterOptions.SearchTerm}%' OR L.IP LIKE '%{filterOptions.SearchTerm}%' OR L.USERAGENT LIKE '%{filterOptions.SearchTerm}%' OR TO_CHAR(L.USUARIO_ID) = '{filterOptions.SearchTerm}')";
                }
                if (filterOptions.LoginStartDate != null)
                {
                    filter += $" AND L.DATA_CADASTRO >= :LoginStartDate ";
                }
                if (filterOptions.LoginEndDate != null)
                {
                    filterOptions.LoginEndDate = filterOptions.LoginEndDate.Value.AddDays(1);
                    filter += $" AND L.DATA_CADASTRO <= :LoginEndDate ";
                }
                if (!String.IsNullOrEmpty(order))
                {
                    var part = order.Split(' ');
                    switch (part[0].ToLower())
                    {
                        case "id":
                            orderBy = "L.ID " + part[1];
                            break;
                        case "email":
                            orderBy = "L.EMAIL " + part[1];
                            break;
                        case "username":
                            orderBy = "U.NOME " + part[1];
                            break;
                        case "ip":
                            orderBy = "L.IP " + part[1];
                            break;
                        case "useragent":
                            orderBy = "L.USERAGENT " + part[1];
                            break;
                        case "status":
                            orderBy = "L.SITUACAO " + part[1];
                            break;
                        case "createdat":
                            orderBy = "L.DATA_CADASTRO " + part[1];
                            break;
                        case "userid":
                            orderBy = "L.USUARIO_ID " + part[1];
                            break;
                    }
                }
                string countquery = $"SELECT count(*) AS COUNT FROM PORTALRH.T_LOGIN L LEFT JOIN PORTALRH.T_USUARIO U on U.ID = L.USUARIO_ID  WHERE {filter}";
                string basequery = $@"SELECT L.ID as Id, L.USUARIO_ID as UsuarioId, U.NOME as UserName, L.EMAIL as Email, L.IP as IP, L.USERAGENT as UserAgent, L.SITUACAO as Status, L.DATA_CADASTRO as CreatedAt  FROM PORTALRH.T_LOGIN L LEFT JOIN PORTALRH.T_USUARIO U on U.ID = L.USUARIO_ID WHERE {filter} ORDER BY {orderBy}";
                string query = $"SELECT b.* FROM(SELECT a.*, rownum r__ FROM ( {basequery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b  WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";
                var count = await conn.QueryFirstOrDefaultAsync<long>(countquery, filterOptions);
                var logins = await conn.QueryAsync<Login>(query,new
                {
                    pageNumber = (skip/limit) +1,
                    pageSize = limit,
                    LoginStartDate = filterOptions.LoginStartDate,
                    LoginEndDate = filterOptions.LoginEndDate
                });
                return new ResponseDto<Login>
                {
                    CurrentPage = (skip / limit) + 1,
                    Data = logins.ToList(),
                    PerPage = limit,
                    Total = count
                };
            }
        }

       
        public async Task<Login> GetById(long id)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT ID as Id, USUARIO_ID as UsuarioId, EMAIL as Email, IP as IP, USERAGENT as UserAgent, SITUACAO as Status, DATA_CADASTRO as CreatedAt from PORTALRH.T_LOGIN L WHERE L.ID = " + id ;
                var login= await conn.QueryFirstOrDefaultAsync<Login>(query);
                return login;
            }
        }

  
    }
}
