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
using Users.API.Dto.User;

namespace Users.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IConfiguration _config;

        private readonly string portalConnectionString;
        private readonly string BIConnectionString;
        private readonly string ASConnectionString;
        public UserRepository(IConfiguration config)
        {
            _config = config;
            portalConnectionString = _config.GetConnectionString("PortalConnection");
            BIConnectionString = _config.GetConnectionString("BIConnection");
            ASConnectionString = _config.GetConnectionString("ASConnection");
        }

        public async Task<ResponseDto<User>> GetAllPaged(UserFilterDto filterOptions, int limit, int skip, string order)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string filter = "1 = 1";
                string orderBy = "U.ID DESC";
               
                if (!String.IsNullOrEmpty(filterOptions.SearchTerm))
                {
                    filterOptions.SearchTerm = filterOptions.SearchTerm.ToLower();
                    //remove non numeric characters
                    var cpf = new string(filterOptions.SearchTerm.Where(c => char.IsDigit(c)).ToArray());
                    if (cpf.Length > 0)
                    {
                        filter += $" AND (LOWER(U.NOME) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(UU.NOME) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(U.CPF) LIKE '%{cpf}%') ";
                    }
                    else
                    {
                        filter += $" AND (LOWER(U.NOME) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(UU.NOME) LIKE '%{filterOptions.SearchTerm}%') ";
                    }

                }
                if (filterOptions.GroupId != null && filterOptions.GroupId.Length > 0)
                {
                    filter += " AND U.GRUPO_ID = ANY :Groups ";
                }
                if (filterOptions.ShopId != null && filterOptions.ShopId.Length > 0)
                {
                    filter += " AND (U.ID_TIPO in (1,2) OR exists(SELECT 1 FROM T_USUARIO_CONVENIO UC WHERE UC.CONVENIO_ID = ANY :Shops AND UC.USUARIO_ID = U.ID))";
                }

                if (filterOptions.UserType > 0)
                {
                    filter += " AND U.ID_TIPO = :UserType ";
                }
               
                if (filterOptions.CreationStartDate != null)
                {
                    filter += $" AND U.DATA_CADASTRO >= :CreationStartDate ";
                }
                if (filterOptions.CreationEndDate != null)
                {
                    filterOptions.CreationEndDate = filterOptions.CreationEndDate.Value.AddDays(1);
                    filter += $" AND U.DATA_CADASTRO <= :CreationEndDate ";
                }
                if (!String.IsNullOrEmpty(order))
                {
                    var part = order.Split(' ');
                    switch (part[0].ToLower())
                    {
                        case "id":
                            orderBy = "U.ID " + part[1];
                            break;
                        case "name":
                            orderBy = "U.NOME " + part[1];
                            break;
                        case "cpf":
                            orderBy = "U.CPF " + part[1];
                            break;
                        case "status":
                            orderBy = "U.ID_STATUS " + part[1];
                            break;
                        case "createdat":
                            orderBy = "U.DATA_CADASTRO " + part[1];
                            break;
                    }
                }
                string countquery = $@"SELECT count(*) AS COUNT 
                                        FROM PORTALRH.T_USUARIO U
                                        LEFT JOIN PORTALRH.T_PERFIL P ON U.PERFIL_ID = P.ID 
                                        LEFT JOIN PORTALRH.T_USUARIO UU ON U.USUARIO_CADASTRO_ID = UU.ID
                                        WHERE {filter}";
                string basequery = $@"SELECT    U.ID AS Id, 
                                                U.NOME AS Name, 
                                                U.EMAIL AS Email, 
                                                U.TELEFONE AS Telefone, 
                                                U.SENHA AS Password, 
                                                U.ID_STATUS AS Status, 
                                                U.TOKEN_RECUPERACAO AS RecoverPasswordToken, 
                                                U.DATA_EXP_RECUPERACAO AS RecoverPasswordTokenExp, 
                                                U.FALHA_LOGIN AS FailedLogins, 
                                                U.CPF AS Cpf, 
                                                U.MATRICULA AS RegistrationNumber, 
                                                U.GRUPO_ID AS GroupId, 
                                                U.PERFIL_ID AS ProfileId, 
                                                U.ID_TIPO AS UserType, 
                                                U.DATA_CADASTRO AS CreatedAt, 
                                                U.USUARIO_CADASTRO_ID AS CreatedBy,
                                                U.DATA_EDICAO AS UpdatedAt 
                                    FROM PORTALRH.T_USUARIO U 
                                    LEFT JOIN PORTALRH.T_PERFIL P ON U.PERFIL_ID = P.ID 
                                    LEFT JOIN PORTALRH.T_USUARIO UU ON U.USUARIO_CADASTRO_ID = UU.ID
                                    WHERE {filter} ORDER BY {orderBy}";
                string query = $"SELECT b.* FROM(SELECT a.*, rownum r__ FROM ( {basequery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";
                var count = await conn.QueryFirstOrDefaultAsync<long>(countquery, new
                {
                    UserType = filterOptions.UserType,
                    Groups = filterOptions.GroupId,
                    Shops = filterOptions.ShopId,
                    CreationStartDate = filterOptions.CreationStartDate,
                    CreationEndDate = filterOptions.CreationEndDate
                });
                var data = await conn.QueryAsync<User>(query, new
                {
                    UserType = filterOptions.UserType,
                    Groups = filterOptions.GroupId,
                    Shops = filterOptions.ShopId,
                    CreationStartDate = filterOptions.CreationStartDate,
                    CreationEndDate = filterOptions.CreationEndDate,
                    pageNumber = (skip / limit) + 1,
                    pageSize = limit
                });
                return new ResponseDto<User>
                {
                    CurrentPage = (skip / limit) + 1,
                    Data = data.ToList(),
                    PerPage = limit,
                    Total = count
                };
            }
        }
        public async Task<List<User>> GetAll(UserFilterDto filterOptions)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string filter = "1 = 1";
                string orderBy = "U.ID DESC";

                if (!String.IsNullOrEmpty(filterOptions.SearchTerm))
                {
                    filterOptions.SearchTerm = filterOptions.SearchTerm.ToLower();
                    //remove non numeric characters
                    var cpf = new string(filterOptions.SearchTerm.Where(c => char.IsDigit(c)).ToArray());
                    if (cpf.Length > 0)
                    {
                        filter += $" AND (LOWER(U.NOME) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(UU.NOME) LIKE '%{filterOptions.SearchTerm}%' OR LOWER(U.CPF) LIKE '%{cpf}%') ";
                    }
                    else
                    {
                        filter += $" AND (LOWER(U.NOME) LIKE '%{filterOptions.SearchTerm}%'  OR LOWER(UU.NOME) LIKE '%{filterOptions.SearchTerm}%') ";
                    }

                }
                if (filterOptions.GroupId != null && filterOptions.GroupId.Length > 0)
                {
                    filter += " AND U.GRUPO_ID = ANY :Groups ";
                }
                if (filterOptions.ShopId != null && filterOptions.ShopId.Length > 0)
                {
                    filter += " AND (U.ID_TIPO in (1,2) OR exists(SELECT 1 FROM T_USUARIO_CONVENIO UC WHERE UC.CONVENIO_ID = ANY :Shops AND UC.USUARIO_ID = U.ID))";
                }

                if (filterOptions.UserType > 0)
                {
                    filter += " AND U.ID_TIPO = :UserType ";
                }
                if (filterOptions.SistemaId > 0)
                {
                    filter += " AND ((P.SISTEMA_ID = :SistemaId) OR (U.PERFIL_ID IS NULL and 2 = :SistemaId)) ";
                }

                if (filterOptions.CreationStartDate != null)
                {
                    filter += $" AND U.DATA_CADASTRO >= :CreationStartDate ";
                }
                if (filterOptions.CreationEndDate != null)
                {
                    filterOptions.CreationEndDate = filterOptions.CreationEndDate.Value.AddDays(1);
                    filter += $" AND U.DATA_CADASTRO <= :CreationEndDate ";
                }
                


                string query = $@"SELECT    U.ID AS Id, 
                                                U.NOME AS Name, 
                                                U.EMAIL AS Email, 
                                                U.TELEFONE AS Telefone, 
                                                U.SENHA AS Password, 
                                                U.ID_STATUS AS Status, 
                                                U.TOKEN_RECUPERACAO AS RecoverPasswordToken, 
                                                U.DATA_EXP_RECUPERACAO AS RecoverPasswordTokenExp, 
                                                U.FALHA_LOGIN AS FailedLogins, 
                                                U.CPF AS Cpf, 
                                                U.MATRICULA AS RegistrationNumber, 
                                                U.GRUPO_ID AS GroupId, 
                                                U.PERFIL_ID AS ProfileId, 
                                                U.ID_TIPO AS UserType, 
                                                U.DATA_CADASTRO AS CreatedAt, 
                                                U.USUARIO_CADASTRO_ID AS CreatedBy
                                    FROM PORTALRH.T_USUARIO U 
                                    LEFT JOIN PORTALRH.T_PERFIL P ON U.PERFIL_ID = P.ID 
                                    LEFT JOIN PORTALRH.T_USUARIO UU ON U.USUARIO_CADASTRO_ID = UU.ID
                                    WHERE {filter} ORDER BY {orderBy}";
                return (await conn.QueryAsync<User>(query, new
                {
                    UserType = filterOptions.UserType,
                    Groups = filterOptions.GroupId,
                    Shops = filterOptions.ShopId,
                    SistemaId = filterOptions.SistemaId,
                    CreationStartDate = filterOptions.CreationStartDate,
                    CreationEndDate = filterOptions.CreationEndDate
                })).ToList();

            }
        }
        public async Task<User> Add(User user)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var parms = new OracleDynamicParameters();
            parms.Add(":MotherName", user.MothersName, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":BirthDate", user.BirthDate, OracleMappingType.Date, ParameterDirection.Input);
            parms.Add(":Name", user.Name, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":Email", user.Email.ToLower(), OracleMappingType.Varchar2, ParameterDirection.Input) ;
            parms.Add(":Telefone", user.Telefone, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":Password", user.Password, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":Cpf", user.Cpf, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":RegistrationNumber", user.RegistrationNumber, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":ProfileId", user.ProfileId, OracleMappingType.Double, ParameterDirection.Input);
            parms.Add(":GroupId", user.GroupId, OracleMappingType.Double, ParameterDirection.Input);
            parms.Add(":UserType", user.UserType, OracleMappingType.Double, ParameterDirection.Input);
            parms.Add(":CreatedAt", user.CreatedAt, OracleMappingType.Date, ParameterDirection.Input);
            parms.Add(":CreatedBy", user.CreatedBy, OracleMappingType.Double, ParameterDirection.Input);
            parms.Add(":Id", null, OracleMappingType.Double, ParameterDirection.Output);

            string query = @"INSERT INTO PORTALRH.T_USUARIO (
                    ID,
                    NOME, 
                    EMAIL,
                    MOTHER_NAME,
                    BIRTH_DATE,
                    TELEFONE,
                    SENHA,
                    CPF,
                    MATRICULA,
                    PERFIL_ID,
                    GRUPO_ID,
                    ID_TIPO,
                    DATA_CADASTRO,
                    USUARIO_CADASTRO_ID
                ) VALUES (
                    S_USUARIO.nextval, 
                    :Name,
                    :Email,
                    :MotherName,
                    :BirthDate,
                    :Telefone,
                    :Password,
                    :Cpf,
                    :RegistrationNumber,
                    :ProfileId,
                    :GroupId,
                    :UserType,
                    :CreatedAt,
                    :CreatedBy
                )  returning ID into :Id";
            await conn.ExecuteAsync(query, parms);
            user.Id = parms.Get<long>("Id");
            return user;

        }

        public async Task Delete(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"DELETE FROM PORTALRH.T_USUARIO WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id });
        }

        public async Task<User> Get(long id)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT ID AS Id, NOME AS Name, EMAIL AS Email, CPF AS Cpf, TELEFONE AS Telefone, SENHA AS Password, ID_STATUS AS Status, TOKEN_RECUPERACAO AS RecoverPasswordToken, DATA_EXP_RECUPERACAO AS RecoverPasswordTokenExp, FALHA_LOGIN AS FailedLogins, CPF AS Cpf, MATRICULA AS RegistrationNumber, GRUPO_ID AS GroupId, PERFIL_ID AS ProfileId, ID_TIPO AS UserType, BIRTH_DATE AS  BirthDate,MOTHER_NAME AS MothersName, DATA_CADASTRO AS CreatedAt, USUARIO_CADASTRO_ID AS CreatedBy FROM PORTALRH.T_USUARIO WHERE ID = :Id";
                return (await conn.QueryFirstOrDefaultAsync<User>(query, new { Id = id }));
            }
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT ID AS Id, NOME AS Name, EMAIL AS Email, CPF AS Cpf, TELEFONE AS Telefone, SENHA AS Password, ID_STATUS AS Status, TOKEN_RECUPERACAO AS RecoverPasswordToken, DATA_EXP_RECUPERACAO AS RecoverPasswordTokenExp, FALHA_LOGIN AS FailedLogins, CPF AS Cpf, MATRICULA AS RegistrationNumber, GRUPO_ID AS GroupId, PERFIL_ID AS ProfileId, ID_TIPO AS UserType, DATA_CADASTRO AS CreatedAt, USUARIO_CADASTRO_ID AS CreatedBy  FROM PORTALRH.T_USUARIO ORDER BY NOME ASC";
                return await conn.QueryAsync<User>(query);
            }
        }

        public async Task<ResponseDto<User>> GetAllPaged(long? groupId, int sistema_id, int limit, int skip, string search, string order)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string filter = "1 = 1";
                string orderBy = "ID ASC";
                if (!String.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    filter = $"(NOME LIKE '%{search}%' OR REPLACE(REPLACE(CPF, '.', ''), '-', '') LIKE '%{search.Replace(".", "").Replace("-", "")}%' OR EMAIL LIKE '%{search}%' OR TELEFONE LIKE '%{search}%' OR MATRICULA LIKE '%{search}%' OR TO_CHAR(GRUPO_ID) = '{search}')";
                }
                if (!String.IsNullOrEmpty(order))
                {
                    var part = order.Split(' ');
                    switch (part[0].ToLower())
                    {
                        case "id":
                            orderBy = "U.ID " + part[1];
                            break;
                        case "name":
                            orderBy = "U.NOME " + part[1];
                            break;
                        case "cpf":
                            orderBy = "U.CPF " + part[1];
                            break;
                        case "email":
                            orderBy = "U.EMAIL " + part[1];
                            break;
                        case "telefone":
                            orderBy = "U.TELEFONE " + part[1];
                            break;
                        case "usertype":
                            orderBy = "U.ID_TIPO " + part[1];
                            break;
                        case "status":
                            orderBy = "U.ID_STATUS " + part[1];
                            break;
                        case "groupid":
                            orderBy = "U.GRUPO_ID " + part[1];
                            break;
                        case "profileid":
                            orderBy = "U.PERFIL_ID " + part[1];
                            break;
                    }
                }
                string countquery = $"SELECT COUNT(*) AS COUNT FROM PORTALRH.T_USUARIO U   LEFT JOIN PORTALRH.T_PERFIL P ON U.PERFIL_ID = P.ID   WHERE (U.GRUPO_ID = :groupId or :groupId is null)  AND ((P.SISTEMA_ID = {sistema_id})  OR (P.SISTEMA_ID is null and 2 = {sistema_id}))  AND {filter}";
                string basequery = $@"SELECT U.ID AS Id, 
                                            U.NOME AS Name, 
                                            U.EMAIL AS Email,  
                                            U.TELEFONE AS Telefone,  
                                            U.SENHA AS Password,  
                                            U.ID_STATUS AS Status,  
                                            U.TOKEN_RECUPERACAO AS RecoverPasswordToken,  
                                            U.DATA_EXP_RECUPERACAO AS RecoverPasswordTokenExp,  
                                            U.FALHA_LOGIN AS FailedLogins,  
                                            U.CPF AS Cpf,  
                                            U.MATRICULA AS RegistrationNumber,  
                                            U.GRUPO_ID AS GroupId,  
                                            U.PERFIL_ID AS ProfileId,  
                                            U.ID_TIPO AS UserType,  
                                            U.DATA_CADASTRO AS CreatedAt,  
                                            U.USUARIO_CADASTRO_ID AS CreatedBy,
                                            U.DATA_EDICAO AS UpdatedAt
                                            FROM PORTALRH.T_USUARIO U  
                                            LEFT JOIN PORTALRH.T_PERFIL P ON U.PERFIL_ID = P.ID  
                                            WHERE (U.GRUPO_ID = :groupId or :groupId is null)  
                                            AND ((P.SISTEMA_ID = {sistema_id})  
                                                OR (P.SISTEMA_ID is null and 2 = {sistema_id}))  
                                            AND {filter} ORDER BY {orderBy}";
                string query = $"SELECT b.*, P.ID AS Id, P.DESCRICAO as Description, P.SISTEMA_ID AS SistemaId FROM(SELECT a.*, rownum r__ FROM ( {basequery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b LEFT JOIN PORTALRH.T_PERFIL P on P.ID = b.ProfileId  WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1) ORDER BY r__ asc";
                var count = await conn.QueryFirstOrDefaultAsync<long>(countquery, new { groupId });
                var users = await conn.QueryAsync<User, Profile, User>(query, (u, p) =>
                {
                    u.Profile = p;
                    return u;
                }, new
                {
                    groupId,
                    pageNumber = (skip / limit) + 1,
                    pageSize = limit
                }, splitOn: "Id");
                return new ResponseDto<User>
                {
                    CurrentPage = (skip / limit) + 1,
                    Data = users.ToList(),
                    PerPage = limit,
                    Total = count
                };
            }
        }

        public async Task<User> Update(User user)
        {
            using var conn = new DbSession(portalConnectionString).Connection;

            string query = @"UPDATE PORTALRH.T_USUARIO SET NOME= :Name, EMAIL= :Email, TELEFONE= :Telefone, CPF= :Cpf, MATRICULA= :RegistrationNumber, GRUPO_ID= :GroupId, PERFIL_ID= :ProfileId, ID_TIPO=:UserType ,BIRTH_DATE=:BirthDate,MOTHER_NAME=:MotherName, DATA_EDICAO=:Hoje WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = user.Id, Name = user.Name, Email = user.Email.ToLower(), Telefone = user.Telefone, Cpf = user.Cpf, RegistrationNumber = user.RegistrationNumber, GroupId = user.GroupId, ProfileId = user.ProfileId, Status = user.Status, UserType = user.UserType, BirthDate = user.BirthDate, MotherName = user.MothersName, Hoje = DateTime.Now });
            return user;

        }
        public async Task<User> UpdateStatus(User user)
        {
            using var conn = new DbSession(portalConnectionString).Connection;

            string query = @"UPDATE PORTALRH.T_USUARIO SET ID_STATUS=:Status, DATA_EDICAO=:Hoje WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = user.Id, Status = user.Status, Hoje = DateTime.Now });
            return user;

        }
        public async Task<IEnumerable<User>> GetAllUsers()
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"SELECT ID AS Id, NOME AS Name, EMAIL AS Email, CPF AS Cpf, TELEFONE AS Telefone, SENHA AS Password,
                    FALHA_LOGIN AS FailedLogins, ID_STATUS AS Status, ID_TIPO AS UserType, DATA_CADASTRO AS CreatedAt, USUARIO_CADASTRO_ID AS CreatedBy  from PORTALRH.T_USUARIO ORDER BY ID ASC";
            var user = await conn.QueryAsync<User>(query);
            return user;
        }
        public async Task<User> GetUserById(long id)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT U.ID AS Id, U.NOME AS Name, U.EMAIL AS Email, U.TELEFONE AS Telefone, U.SENHA AS Password, U.ID_STATUS AS Status, U.TOKEN_RECUPERACAO AS RecoverPasswordToken, U.DATA_EXP_RECUPERACAO AS RecoverPasswordTokenExp, U.FALHA_LOGIN AS FailedLogins, U.CPF AS Cpf, U.MATRICULA AS RegistrationNumber, U.GRUPO_ID AS GroupId, U.PERFIL_ID AS ProfileId, U.ID_TIPO AS UserType, U.DATA_CADASTRO AS CreatedAt, U.USUARIO_CADASTRO_ID AS CreatedBy from PORTALRH.T_USUARIO U WHERE U.ID = " + id + " ORDER BY ID ASC";
                var user = await conn.QueryFirstOrDefaultAsync<User>(query);
                return user;
            }
        }

        public async Task<User> GetUserByEmail(string email)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT 
                    U.ID AS Id, 
                    U.NOME AS Name, 
                    LOWER(U.EMAIL) AS Email,
                    U.CPF AS Cpf, 
                    U.TELEFONE AS Telefone,
                    U.SENHA AS Password,
                    U.FALHA_LOGIN AS FailedLogins,
                    U.PERFIL_ID AS ProfileId, 
                    U.ID_STATUS AS Status,
                    U.ID_TIPO AS UserType,
                    U.DATA_CADASTRO AS CreatedAt, 
                    U.USUARIO_CADASTRO_ID AS CreatedBy,
                    P.ID AS Id,
                    P.DESCRICAO AS Description,
                    P.SISTEMA_ID AS SistemaId
                    from PORTALRH.T_USUARIO U
                    LEFT JOIN PORTALRH.T_PERFIL P on P.ID = U.PERFIL_ID
                    WHERE LOWER(U.EMAIL) = :Email and U.ID_STATUS <> 'CANCELADO'";
                var user = (await conn.QueryAsync<User, Profile, User>(query, (u, p) => { u.Profile = p; return u; }, new { Email = email }, splitOn: "Id")).FirstOrDefault();
                return user;
            }
        }
        public async Task<User> GetUserByCpf(string cpf)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT 
                    U.ID AS Id, 
                    U.NOME AS Name, 
                    LOWER(U.EMAIL) AS Email,
                    U.CPF AS Cpf, 
                    U.TELEFONE AS Telefone,
                    U.SENHA AS Password,
                    U.FALHA_LOGIN AS FailedLogins,
                    U.PERFIL_ID AS ProfileId, 
                    U.ID_STATUS AS Status,
                    U.ID_TIPO AS UserType,
                    U.DATA_CADASTRO AS CreatedAt,
                    U.USUARIO_CADASTRO_ID AS CreatedBy,
                    P.ID AS Id,
                    P.DESCRICAO AS Description,
                    P.SISTEMA_ID AS SistemaId
                    from PORTALRH.T_USUARIO U
                    LEFT JOIN PORTALRH.T_PERFIL P on P.ID = U.PERFIL_ID
                    WHERE CPF = :Cpf and U.ID_STATUS <> 'CANCELADO'";
                var user = (await conn.QueryAsync<User, Profile, User>(query, (u, p) => { u.Profile = p; return u; }, new { Cpf = cpf}, splitOn: "Id")).FirstOrDefault();
                return user;
            }
        }
        public async Task<User> GetUserByPhone(string phone)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT 
                    U.ID AS Id, 
                    U.NOME AS Name, 
                    U.EMAIL AS Email,
                    U.CPF AS Cpf, 
                    U.TELEFONE AS Telefone,
                    U.SENHA AS Password,
                    U.FALHA_LOGIN AS FailedLogins,
                    U.PERFIL_ID AS ProfileId, 
                    U.ID_STATUS AS Status,
                    U.ID_TIPO AS UserType,
                    U.DATA_CADASTRO AS CreatedAt, 
                    U.USUARIO_CADASTRO_ID AS CreatedBy,
                    P.ID AS Id,
                    P.DESCRICAO AS Description,
                    P.SISTEMA_ID AS SistemaId
                    from PORTALRH.T_USUARIO U
                    LEFT JOIN PORTALRH.T_PERFIL P on P.ID = U.PERFIL_ID
                    WHERE U.TELEFONE = :Telefone and U.ID_STATUS <> 'CANCELADO'";
                var user = (await conn.QueryAsync<User, Profile, User>(query, (u, p) => { u.Profile = p; return u; }, new { Telefone = phone }, splitOn: "Id")).FirstOrDefault();
                return user;
            }
        }
        public async Task CreateUser(User user)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = $@"INSERT INTO PORTALRH.T_USUARIO (
                    ID,
                    NOME, 
                    EMAIL,
                    TELEFONE,
                    SENHA,
                    ID_TIPO
                ) VALUES (
                    :Id, 
                    :Name,
                    :Email,
                    :Telefone,
                    :Password,
                    :UserType)";

                await conn.ExecuteAsync(query, user);
            }
        }
        public async Task RegisterFailedLogin(User user)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = $"UPDATE PORTALRH.T_USUARIO SET FALHA_LOGIN = FALHA_LOGIN + 1 WHERE ID = :Id";
                await conn.ExecuteAsync(query, user);
            }
        }
        public async Task ResetFailedLogin(User user)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = $"UPDATE PORTALRH.T_USUARIO SET FALHA_LOGIN = 0 WHERE ID = :Id";
                await conn.ExecuteAsync(query, user);
            }
        }
        public async Task<int> GetMaxCod()
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string maxCodQuery = "SELECT PORTALRH.S_USUARIO.nextval FROM DUAL";
                int maxCod = await conn.QueryFirstOrDefaultAsync<int>(maxCodQuery);
                return maxCod;
            }
        }

        public async Task<bool> GenerateRecoverToken(User user)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = $"UPDATE PORTALRH.T_USUARIO SET TOKEN_RECUPERACAO = :RecoverPasswordToken, DATA_EXP_RECUPERACAO = :RecoverPasswordTokenExp WHERE ID = :Id";
                await conn.ExecuteAsync(query, user);

                return true;
            }
        }

        public async Task<bool> ChangePassword(User user)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = $"UPDATE PORTALRH.T_USUARIO SET SENHA = '{user.Password}', FALHA_LOGIN = 0 WHERE ID = '{user.Id}'";
                await conn.ExecuteAsync(query);

                return true;
            }
        }

        public async Task<User> GetUserByRecoverToken(string token)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"SELECT 
                    ID AS Id, 
                    NOME AS Name, 
                    EMAIL AS Email,
                    CPF AS Cpf,
                    TELEFONE AS Telefone,
                    SENHA AS Password,
                    FALHA_LOGIN AS FailedLogins,
                    PERFIL_ID AS ProfileId, 
                    ID_STATUS AS Status,
                    ID_TIPO AS UserType,
                    TOKEN_RECUPERACAO AS RecoverPasswordToken,
                    DATA_EXP_RECUPERACAO AS RecoverPasswordTokenExp
                    from PORTALRH.T_USUARIO WHERE TOKEN_RECUPERACAO = '" + token.ToString() + "'";
                User user = await conn.QueryFirstOrDefaultAsync<User>(query);
                Console.WriteLine(token);
                return user;
            }
        }

        public async Task<List<Group>> GetGroupsByUser(User user, string search)
        {
            string query = $@"SELECT * FROM (
                                SELECT G.IDGRUPOCONVENIO AS Id, G.DESCRICAO AS Name 
                                FROM NEWUNIK.T_GRUPOCONVENIO G 
                                WHERE (G.IDGRUPOCONVENIO = :groupId or :userType = 1) 
                                AND (lower(G.DESCRICAO) LIKE lower('%' || :search || '%' ) or  TO_CHAR(G.IDGRUPOCONVENIO) = :search or :search = '')
                                ORDER BY DESCRICAO 
                                )
                                WHERE ROWNUM <= 100";
            using (var conn = new DbSession(ASConnectionString).Connection)
            {
                return (await conn.QueryAsync<Group>(query, new { search, groupId = user.GroupId, userType = user.UserType })).ToList();
            }
        }
        public async Task<Group> GetGroupById(long id, User user)
        {
            string query = $@"
                                SELECT G.IDGRUPOCONVENIO AS Id, G.DESCRICAO AS Name 
                                FROM NEWUNIK.T_GRUPOCONVENIO G 
                                WHERE G.STATUS = 'A' AND G.IDGRUPOCONVENIO = :id AND (G.IDGRUPOCONVENIO = :groupId or :userType = 1)                                 
                              ";
            using (var conn = new DbSession(ASConnectionString).Connection)
            {
                return (await conn.QueryFirstOrDefaultAsync<Group>(query, new { id, groupId = user.GroupId, userType = user.UserType }));
            }
        }
        public async Task<List<Group>> GetAllGroups()
        {
            string query = $@"SELECT G.IDGRUPOCONVENIO AS Id, G.DESCRICAO AS Name 
                                FROM NEWUNIK.T_GRUPOCONVENIO G 
                                WHERE G.STATUS = 'A' 
                                ORDER BY DESCRICAO 
                                ";
            using (var conn = new DbSession(ASConnectionString).Connection)
            {
                return (await conn.QueryAsync<Group>(query)).ToList();
            }
        }

        public async Task<List<Shop>> GetShopsByGroup(long? groupId)
        {
            string query = @"SELECT l.IDLOJA                    as Id, 
                                    l.DESCRICAOEXIBIVELCLIENTE  as Name ,
                                    l.LOGRADOURO                as StreetAddress ,
                                    l.NUMERO                    as AddressNumber ,
                                    l.COMPLEMENTO               as AddressComplement ,
                                    l.BAIRRO                    as Neighborhood ,
                                    l.CIDADE                    as CityName ,
                                    l.ESTADO                    as StateName ,
                                    l.CEP                       as ZipCode ,
                                    l.CNPJ                      as Cnpj,
                                    l.DATACADASTRO              as CreatedAt,
                                    l.STATUS                    as Status,
                                    tg.IDGRUPOCONVENIO          as IdGroup,
                                    tg.DESCRICAO                as GroupName,  
                                    tg.VALORLIMITE              as Limit,
                                    tg.VALORLIMITEDISPONIVEL    as AvailableLimit,
                                    ac.DIAPADRAOCORTE           as ClosingDay,
                                    ac.PROXIMOVENCIMENTO        as NextDateExpiration
                            FROM NEWUNIK.T_LOJA l
                                JOIN NEWUNIK.T_GRUPOCONVENIO tg ON l.IDGRUPOCONVENIO = tg.IDGRUPOCONVENIO
                                JOIN NEWUNIK.T_ACORDOCONVENIO ac ON l.IDLOJA = ac.IDLOJA
                            WHERE (l.IDGRUPOCONVENIO = :groupId OR :groupId is null)  
                              ORDER BY DESCRICAOEXIBIVELCLIENTE ASC";
            using (var conn = new DbSession(ASConnectionString).Connection)
            {
                return (await conn.QueryAsync<Shop>(query, new { groupId })).ToList();
            }
        }
        public async Task<List<Shop>> GetShopsByUser(long? userId)
        {
            long[] convenios;
            string query = @"select UC.CONVENIO_ID from PORTALRH.T_USUARIO_CONVENIO UC WHERE UC.USUARIO_ID = :userId";
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                convenios = (await conn.QueryAsync<long>(query, new { userId })).ToArray();
            }
            if (convenios != null && convenios.Length > 0)
            {
                query = @"SELECT l.IDLOJA                    as Id, 
                                    l.DESCRICAOEXIBIVELCLIENTE  as Name ,
                                    l.LOGRADOURO                as StreetAddress ,
                                    l.NUMERO                    as AddressNumber ,
                                    l.COMPLEMENTO               as AddressComplement ,
                                    l.BAIRRO                    as Neighborhood ,
                                    l.CIDADE                    as CityName ,
                                    l.ESTADO                    as StateName ,
                                    l.CEP                       as ZipCode ,
                                    l.CNPJ                      as Cnpj,
                                    l.DATACADASTRO              as CreatedAt,
                                    l.STATUS                    as Status,
                                    tg.IDGRUPOCONVENIO          as IdGroup,
                                    tg.DESCRICAO                as GroupName,  
                                    tg.VALORLIMITE              as Limit,
                                    tg.VALORLIMITEDISPONIVEL    as AvailableLimit,
                                    ac.DIAPADRAOCORTE           as ClosingDay,
                                    ac.PROXIMOVENCIMENTO        as NextDateExpiration
                            FROM NEWUNIK.T_LOJA l
                                JOIN NEWUNIK.T_GRUPOCONVENIO tg ON l.IDGRUPOCONVENIO = tg.IDGRUPOCONVENIO
                                JOIN NEWUNIK.T_ACORDOCONVENIO ac ON l.IDLOJA = ac.IDLOJA
                            WHERE l.IDLOJA in :convenios  
                              ORDER BY DESCRICAOEXIBIVELCLIENTE ASC";
                using (var conn = new DbSession(ASConnectionString).Connection)
                {
                    return (await conn.QueryAsync<Shop>(query, new { convenios })).ToList();
                }
            }
            else
            {
                return null;
            }
        }

        public async Task Activate(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_USUARIO SET ID_STATUS=:Status WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id, Status = "ATIVO" });
        }
        public async Task Deactivate(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_USUARIO SET ID_STATUS=:Status WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id, Status = "INATIVO" });
        }
        public async Task Cancel(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_USUARIO SET ID_STATUS=:Status WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id, Status = "CANCELADO" });
        }

        public async Task LockOrUnlock(long id, string status)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_USUARIO SET ID_STATUS=:Status WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id, Status = status });
        }
        public async Task<List<string>> PasswordOkToSave(long userId)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"SELECT SENHA
                FROM (
	            SELECT SENHA  FROM T_SENHA_HISTORICO tsh WHERE ID_USUARIO  = :Id
	            UNION 
	            SELECT SENHA FROM T_USUARIO tu WHERE ID  = :Id
            ) 
            ";

            var ret = await conn.QueryAsync<string>(query, new { Id = userId });
            return ret.ToList();

        }

        public async Task SaveOldEmail(long userId, string oldPassword, DateTime lastChange)
        {
            if (!string.IsNullOrEmpty(oldPassword))
            {
                var canSave = await PasswordOkToSave(userId);

                if (!canSave.Where(has => BCrypt.Net.BCrypt.Verify(oldPassword, has)).Any())
                {
                    using var conn = new DbSession(portalConnectionString).Connection;
                    var parms = new OracleDynamicParameters();

                    parms.Add(":oldPassword", oldPassword, OracleMappingType.Varchar2, ParameterDirection.Input);
                    parms.Add(":dataCriacao", lastChange, OracleMappingType.Date, ParameterDirection.Input);
                    parms.Add(":idUsuario", userId, OracleMappingType.Double, ParameterDirection.Input);

                    string query = @"INSERT INTO PORTALRH.T_SENHA_HISTORICO (
                     SENHA,
                     DATA_CRIACAO,
                     ID_USUARIO
                   ) VALUES (
                    :oldPassword,
                    :dataCriacao,:idUsuario

                  )";
                    await conn.ExecuteAsync(query, parms);
                }
            }
        }

        public async Task<Login> LastLogin(string email)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string query = @"  SELECT ID
                    USUARIO_ID	
                    ,EMAIL	
                    ,IP	
                    ,USERAGENT,
                    SITUACAO,
                    DATA_CADASTRO AS LastLogin FROM (SELECT s.*, rank() over (order BY  id desc) r from T_LOGIN s WHERE EMAIL = :Email) where r = 1";
                var ret = (await conn.QueryFirstOrDefaultAsync<Login>(query, new { Email = email }));
                return ret;
            }
        }


        public async Task<List<Notification>> GetAllUserNotification(long userId)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"SELECT ID AS Id, 
                                    TITULO AS Title, 
                                    DESCRICAO AS Description, 
                                    USUARIO_ID AS UserId, 
                                    DATA_LEITURA AS ReadAt, 
                                    DATA_CRIACAO AS CreatedAt

                            FROM PORTALRH.T_NOTIFICACAO 
                            WHERE USUARIO_ID = :UserId
                            ORDER BY ID DESC";
            return (await conn.QueryAsync<Notification>(query, new { UserId = userId })).ToList();
        }

        public async Task<Notification> CreateUserNotification(Notification notification)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var parms = new OracleDynamicParameters();
            parms.Add(":p_Title", notification.Title, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_Description", notification.Description, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_UserId", notification.UserId, OracleMappingType.Double, ParameterDirection.Input);
            parms.Add(":p_Created", DateTime.Now, OracleMappingType.Date, ParameterDirection.Input);
            parms.Add(":Id", null, OracleMappingType.Double, ParameterDirection.Output);

            string query = @"INSERT INTO PORTALRH.T_NOTIFICACAO
                                    (ID, TITULO, DESCRICAO, USUARIO_ID, DATA_CRIACAO, DATA_LEITURA)
                                    VALUES(S_NOTIFICACAO.nextval, :p_Title, :p_Description, :p_UserId, :p_Created, null)  returning ID into :Id";
            await conn.ExecuteAsync(query, parms);
            notification.Id = parms.Get<long>("Id");
            return notification;
        }


        public async Task<Notification> UpdateUserNotification(Notification notification)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_NOTIFICACAO SET TITULO=:Title, DESCRICAO=:Description, DATA_LEITURA=:DateRead WHERE ID=:Id";
            await conn.ExecuteAsync(query, new { Id = notification.Id, Title = notification.Title, Description = notification.Description, DateRead = notification.ReadAt });
            return notification;
        }

        public async Task DeleteUserNotificaction(long userId, long[] ids)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"DELETE FROM PORTALRH.T_NOTIFICACAO WHERE ID IN :Ids AND USUARIO_ID = :UserId";
            await conn.ExecuteAsync(query, new { Ids = ids, UserId = userId });
        }
         public async Task<List<UserMenuDto>> GetAllbyMenu(string searchTerm)
        {          
            using var conn = new DbSession(portalConnectionString).Connection;
  
            string filter = "";

             if (!String.IsNullOrEmpty(searchTerm))
                {
                    filter = $"AND (M.NOME) LIKE '%{searchTerm}%'";
                }

            var query = $@"SELECT 
                            U.ID AS Id,
                            U.NOME AS Name,
                            U.EMAIL AS Email,
                            U.GRUPO_ID AS GroupId,
                            U.ID_STATUS AS StatusId,
                            P.SISTEMA_ID AS SystemId,
                            PF.DESCRICAO AS DescriptionPerfil,
                            PM.MENU_ID AS MenuId,
                            M.NOME AS NameMenu,
                            M.TIPO AS TypeMenu,
                            M.URL AS UrlMenu,
                            S.DESCRICAO AS DescriptionMenu
                        FROM PORTALRH.T_USUARIO U
                            JOIN T_PERFIL PF ON PF.ID = U.PERFIL_ID
                            JOIN T_PERFIL_MENU PM ON PM.PERFIL_ID = PF.ID 
                            JOIN T_MENU M ON M.ID = PM.MENU_ID 
                            LEFT JOIN PORTALRH.T_MENU P ON P.ID = M.PARENT 
                            LEFT JOIN PORTALRH.T_SISTEMA S ON S.ID = M.SISTEMA_ID   
                        WHERE 1 = 1 
                        AND M.TIPO = 'MENU' 
                        AND M.SISTEMA_ID IN (1,2)
                        AND M.PARENT = 1
                        AND U.ID_STATUS = 'ATIVO'
                        {filter}";

                        return (await conn.QueryAsync<UserMenuDto>(query)).ToList(); 
            }

        }

    }

