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
using Maoli;
using RabbitMQ.Client;
using System.Security.Policy;
using Users.API.Dto.Acesso;
using System.Net;

namespace Users.API.Repositories
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
            try
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
            catch (Exception ex)
            {
                throw ex;
            }
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
                string query = @"SELECT A.ID as Id, A.USUARIO_ID as UsuarioId, A.IP as Ip, A.URL as Url, A.METODO as Method, A.POSTDATA as PostData, A.DATA_CADASTRO as CreatedAt,
                                    U.NOME as Nome, U.CPF as Cpf, U.EMAIL as Email, U.TELEFONE as Telefone, P.DESCRICAO as Perfil
                                    FROM PORTALRH.T_ACESSO A 
                                    LEFT JOIN PORTALRH.T_USUARIO U on U.ID = A.USUARIO_ID
                                    LEFT JOIN PORTALRH.T_PERFIL P on P.ID = U.PERFIL_ID
                                 WHERE A.ID = :Id";
                return (await conn.QueryFirstOrDefaultAsync<Acesso>(query, new { Id = id }));
            }
        }

        public async Task<IEnumerable<Acesso>> GetAll()
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT ID as Id, USUARIO_ID as UsuarioId, IP as Ip, URL as Url, METODO as Method, POSTDATA as PostData, DATA_CADASTRO as CreatedAt FROM PORTALRH.T_ACESSO";
                return await conn.QueryAsync<Acesso>(query);
            }
        }

        public async Task<ResponseDto<AcessoDto>> GetAllPaged(AcessoFilterDto filterOptions, int limit, int skip, string order)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string filter = "1 = 1";
                string orderBy = "ID ASC";
                if (!String.IsNullOrEmpty(filterOptions.SearchTerm))
                {
                    filter = $"(tu.NOME LIKE '%{filterOptions.SearchTerm}%' OR A.IP LIKE '%{filterOptions.SearchTerm}%' OR A.METODO LIKE '%{filterOptions.SearchTerm}%' OR A.URL LIKE '%{filterOptions.SearchTerm}%' OR TO_CHAR(A.USUARIO_ID) = '{filterOptions.SearchTerm}')";
                }
                if (filterOptions.AccessStartDate != null)
                {
                    filter += $" AND A.DATA_CADASTRO >= :AccessStartDate ";
                }
                if (filterOptions.AccessEndDate != null)
                {
                    filterOptions.AccessEndDate = filterOptions.AccessEndDate.Value.AddDays(1);
                    filter += $" AND A.DATA_CADASTRO <= :AccessEndDate ";
                }
                if (!String.IsNullOrEmpty(order))
                {
                    var part = order.Split(' ');
                    switch (part[0].ToLower())
                    {
                        case "id":
                            orderBy = "A.ID " + part[1];
                            break;
                        case "ip":
                            orderBy = "A.IP " + part[1];
                            break;
                        case "nome":
                            orderBy = "tu.nome " + part[1];
                            break;
                        case "cpf":
                            orderBy = "tu.cpf " + part[1];
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

                string countquery = $"SELECT count(*) AS COUNT FROM PORTALRH.T_ACESSO A LEFT JOIN PORTALRH.T_USUARIO tu ON A.usuario_id = tu.id LEFT JOIN PORTALRH.T_PERFIL tp on tu.PERFIL_ID = tp.ID  WHERE {filter}";
                string basequery = $@"SELECT A.ID as Id, A.USUARIO_ID as UsuarioId, tu.nome as Nome, tu.cpf as Cpf, tu.TELEFONE as Telefone, tu.EMAIL as Email, A.IP as Ip, tp.DESCRICAO as Perfil, A.METODO as Method, A.URL as Url, A.POSTDATA as PostData, A.DATA_CADASTRO as CreatedAt FROM PORTALRH.T_ACESSO A LEFT JOIN PORTALRH.T_USUARIO tu ON A.usuario_id = tu.id LEFT JOIN PORTALRH.T_PERFIL tp on tu.PERFIL_ID = tp.ID WHERE {filter} ORDER BY {orderBy}";
                string query = $"SELECT b.* FROM(SELECT a.*, rownum r__ FROM ( {basequery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b  WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";
                var count = await conn.QueryFirstOrDefaultAsync<long>(countquery, filterOptions);
                var logins = await conn.QueryAsync<AcessoDto>(query,new
                {
                    pageNumber = (skip/limit) +1,
                    pageSize = limit,
                    AccessStartDate = filterOptions.AccessStartDate,
                    AccessEndDate = filterOptions.AccessEndDate,
                });
                return new ResponseDto<AcessoDto>
                {
                    CurrentPage = (skip / limit) + 1,
                    Data = logins.ToList(),
                    PerPage = limit,
                    Total = count
                };
            }
        }

        public async Task<List<AcessoDto>> GetAll(AcessoFilterDto filterOptions)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string filter = "1 = 1";
                string orderBy = "ID ASC";
                if (!String.IsNullOrEmpty(filterOptions.SearchTerm))
                {
                    filter = $"(A.IP LIKE '%{filterOptions.SearchTerm}%' OR A.METODO LIKE '%{filterOptions.SearchTerm}%' OR A.URL LIKE '%{filterOptions.SearchTerm}%' OR TO_CHAR(A.USUARIO_ID) = '{filterOptions.SearchTerm}')";
                }
                if (filterOptions.AccessStartDate != null)
                {
                    filter += $" AND A.DATA_CADASTRO >= :AccessStartDate ";
                }
                if (filterOptions.AccessEndDate != null)
                {
                    filterOptions.AccessEndDate = filterOptions.AccessEndDate.Value.AddDays(1);
                    filter += $" AND A.DATA_CADASTRO <= :AccessEndDate ";
                }
                string query = $@"SELECT A.ID as Id, A.USUARIO_ID as UsuarioId, tu.nome as Nome, tu.cpf as Cpf, tu.TELEFONE as Telefone, tu.EMAIL as Email, A.IP as Ip, tp.DESCRICAO as Perfil, A.METODO as Method, A.URL as Url, A.POSTDATA as PostData, A.DATA_CADASTRO as CreatedAt FROM PORTALRH.T_ACESSO A LEFT JOIN PORTALRH.T_USUARIO tu ON A.usuario_id = tu.id LEFT JOIN PORTALRH.T_PERFIL tp on tu.PERFIL_ID = tp.ID WHERE {filter} ORDER BY {orderBy}";
                return ( await conn.QueryAsync<AcessoDto>(query, new
                {
                    AccessStartDate = filterOptions.AccessStartDate,
                    AccessEndDate = filterOptions.AccessEndDate,
                })).ToList();
            }
        }



    }
}
