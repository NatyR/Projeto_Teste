using Dapper;
using Dapper.Oracle;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
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
    public class NotificationRepository : INotificationRepository
    {
        private readonly IConfiguration _config;
        private readonly string portalConnectionString;

        public NotificationRepository(IConfiguration config)
        {
            _config = config;
            portalConnectionString = _config.GetConnectionString("PortalConnection");
        }
        public async Task<Notification> Create(Notification notification)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var parms = new OracleDynamicParameters();
    
            parms.Add(":p_Title", notification.Title, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_Description", notification.Description, OracleMappingType.Varchar2, ParameterDirection.Input);
            parms.Add(":p_UserId", notification.UserId, OracleMappingType.Double, ParameterDirection.Input);
            parms.Add(":p_Created", DateTime.Now, OracleMappingType.Date, ParameterDirection.Input);
            parms.Add(":Id", null, OracleMappingType.Double, ParameterDirection.Output);
            parms.Add(":p_NotificationType", notification.NotificationType, OracleMappingType.Int16, ParameterDirection.Input);

            string query = @"INSERT INTO PORTALRH.T_NOTIFICACAO
                                    (ID, TITULO, DESCRICAO, USUARIO_ID, DATA_CRIACAO, DATA_LEITURA, TIPO_NOTIFICACAO_ID)
                                    VALUES(S_NOTIFICACAO.nextval, :p_Title, :p_Description, :p_UserId, :p_Created, null, :p_NotificationType)  returning ID into :Id";
            await conn.ExecuteAsync(query, parms);
            notification.Id = parms.Get<long>("Id");
            return notification;
        }

        public async Task Delete(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"DELETE FROM PORTALRH.T_NOTIFICACAO WHERE ID = :Id";
            await conn.ExecuteAsync(query, new { Id = id });
        }
        public async Task Remove(long userId, long[] ids)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"DELETE FROM PORTALRH.T_NOTIFICACAO WHERE ID IN :Ids AND USUARIO_ID = :UserId";
            await conn.ExecuteAsync(query, new { Ids = ids, UserId = userId });
        }
        public async Task Read(long userId, long[] ids)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_NOTIFICACAO SET DATA_LEITURA = sysdate WHERE ID IN :Ids AND USUARIO_ID = :UserId AND DATA_LEITURA IS NULL";
            await conn.ExecuteAsync(query, new { Ids = ids, UserId = userId });
        }
        public async Task Unread(long userId, long[] ids)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_NOTIFICACAO SET DATA_LEITURA = NULL WHERE ID IN :Ids AND USUARIO_ID = :UserId AND DATA_LEITURA IS NOT NULL";
            await conn.ExecuteAsync(query, new { Ids = ids, UserId = userId });
        }
        public async Task<Notification> Get(long id)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"SELECT N.ID AS Id, 
                                    N.TITULO AS Title, 
                                    N.DESCRICAO AS Description, 
                                    N.USUARIO_ID AS UserId, 
                                    N.DATA_LEITURA AS ReadAt, 
                                    N.DATA_CRIACAO AS CreatedAt,
                                    N.TIPO_NOTIFICACAO_ID AS NotificationType,
                                    (SELECT MAX(ID) from PORTALRH.T_NOTIFICACAO NN WHERE NN.USUARIO_ID = N.USUARIO_ID and NN.ID < N.ID) AS PreviousId,
                                    (SELECT MIN(ID) from PORTALRH.T_NOTIFICACAO NN WHERE NN.USUARIO_ID = N.USUARIO_ID and NN.ID > N.ID) AS NextId
                            FROM PORTALRH.T_NOTIFICACAO N
                            LEFT JOIN PORTALRH.T_TIPO_NOTIFICACAO TN ON TN.ID = N.TIPO_NOTIFICACAO_ID
                            WHERE N.ID = :Id
                            ORDER BY N.ID ASC";
            return (await conn.QueryFirstOrDefaultAsync<Notification>(query, new
            {
                Id = id
            }));
        }

        public async Task<List<Notification>> GetActiveNotifications(long UserId)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"SELECT N.ID AS Id, 
                                    N.TITULO AS Title, 
                                    N.DESCRICAO AS Description, 
                                    N.USUARIO_ID AS UserId, 
                                    N.DATA_LEITURA AS ReadAt, 
                                    N.DATA_CRIACAO AS CreatedAt,
                                    N.TIPO_NOTIFICACAO_ID AS NotificationType
                            FROM PORTALRH.T_NOTIFICACAO N
                            LEFT JOIN PORTALRH.T_TIPO_NOTIFICACAO TN ON TN.ID = N.TIPO_NOTIFICACAO_ID
                            WHERE n.USUARIO_ID = :UserId
                            AND   n.DATA_LEITURA is null
                            ORDER BY n.ID DESC";
            return (await conn.QueryAsync<Notification>(query,new
            {
                UserId
            })).ToList();
        }


        public async Task<List<Notification>> GetAll(long userId)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"SELECT N.ID AS Id, 
                                    N.TITULO AS Title, 
                                    N.DESCRICAO AS Description, 
                                    N.USUARIO_ID AS UserId, 
                                    N.DATA_LEITURA AS ReadAt, 
                                    N.DATA_CRIACAO AS CreatedAt,
                                    N.TIPO_NOTIFICACAO_ID AS NotificationType
                            FROM PORTALRH.T_NOTIFICACAO N
                            LEFT JOIN PORTALRH.T_TIPO_NOTIFICACAO TN ON TN.ID = N.TIPO_NOTIFICACAO_ID
                            WHERE N.USUARIO_ID = :UserId
                            ORDER BY N.ID DESC";
            return (await conn.QueryAsync<Notification>(query,new {UserId = userId})).ToList();
        }

        public async Task<List<long>> GetUsersByGroup(long groupId)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"SELECT ID AS Id FROM PORTALRH.T_USUARIO 
                            WHERE GRUPO_ID = :GroupId";
            return (await conn.QueryAsync<long>(query, new {GroupId = groupId})).ToList();
        }

        public async Task<List<long>> GetUsersByShop(long groupId, long shopId)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"SELECT ID AS Id FROM PORTALRH.T_USUARIO 
                            WHERE GRUPO_ID = :GroupId 
                            AND (exists(SELECT 1 from PORTALRH.T_USUARIO_CONVENIO UC WHERE UC.USUARIO_ID = T_USUARIO.ID AND UC.CONVENIO_ID = :ShopId) OR ID_TIPO = 2)";
            return (await conn.QueryAsync<long>(query, new { GroupId = groupId, ShopId = shopId })).ToList();
        }

        public async Task<Notification> Update(Notification notification)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"UPDATE PORTALRH.T_NOTIFICACAO SET TITULO=:Title, DESCRICAO=:Description, DATA_LEITURA=:DateRead WHERE ID=:Id";
            await conn.ExecuteAsync(query, new { Id = notification.Id,Title = notification.Title, Description = notification.Description, DateRead= notification.ReadAt});
            return notification;
        }

        public async Task<ResponseDto<Notification>> GetAllPaged(int limit, int skip, string order, int idUser)
        {
            using (var conn = new DbSession(portalConnectionString).Connection)
            {
                string filter = "N.USUARIO_ID = :UserId";
                string orderBy = "N.ID DESC";
                if (!String.IsNullOrEmpty(order))
                {
                    var part = order.Split(' ');
                    switch (part[0].ToLower())
                    {
                        case "title":
                            orderBy = "N.TITULO " + part[1];
                            break;
                        case "createdat":
                            orderBy = "N.DATA_CRIACAO " + part[1];
                            break;
                        case "description":
                            orderBy = "N.DESCRICAO " + part[1];
                            break;
                    }
                }


                string countquery = $@"SELECT count(*) AS COUNT FROM PORTALRH.T_NOTIFICACAO N  WHERE {filter}";
                string basequery = $@"SELECT N.ID AS Id,
                                        N.TITULO AS Title,
                                        N.DESCRICAO AS Description,
                                        N.USUARIO_ID AS UserId,
                                        N.DATA_LEITURA AS ReadAt,
                                        N.DATA_CRIACAO AS CreatedAt,
                                        N.TIPO_NOTIFICACAO_ID AS NotificationType
                                        FROM PORTALRH.T_NOTIFICACAO N
                                        LEFT JOIN PORTALRH.T_TIPO_NOTIFICACAO TN ON TN.ID = N.TIPO_NOTIFICACAO_ID
                                        WHERE {filter} ORDER BY {orderBy}";
                string query = $"SELECT b.* FROM(SELECT a.*, rownum r__ FROM ( {basequery} ) a WHERE rownum < ((:pageNumber * :pageSize) +1 ) ) b WHERE r__ >= (((:pageNumber - 1) * :pageSize) +1)";
                var count = await conn.QueryFirstOrDefaultAsync<long>(countquery,
                    new
                    {
                       UserId = idUser
                    });
                var records = await conn.QueryAsync<Notification>(query, new
                {
                    UserId = idUser,
                    pageNumber = (skip / limit) + 1,
                    pageSize = limit
                });
                return new ResponseDto<Notification>
                {
                    CurrentPage = (skip / limit) + 1,
                    Data = records.ToList(),
                    PerPage = limit,
                    Total = count
                };

            }
        }

        public async Task<List<NotificationType>> GetTypes()
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = $@"SELECT   
                                ID AS Id, 
                                NOME AS Name
                              FROM PORTALRH.T_TIPO_NOTIFICACAO
                              ORDER BY ID";
                return (await conn.QueryAsync<NotificationType>(query)).ToList();
        }


         public async Task<ResponseDto<Notification>> GetActiveNotificationsByType(long userId, int type)
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            string query = @"SELECT N.ID AS Id, 
                                    N.TITULO AS Title, 
                                    N.DESCRICAO AS Description, 
                                    N.USUARIO_ID AS UserId, 
                                    N.DATA_LEITURA AS ReadAt, 
                                    N.DATA_CRIACAO AS CreatedAt,
                                    N.TIPO_NOTIFICACAO_ID AS NotificationType
                            FROM PORTALRH.T_NOTIFICACAO N
                            LEFT JOIN PORTALRH.T_TIPO_NOTIFICACAO TN ON TN.ID = N.TIPO_NOTIFICACAO_ID
                            WHERE n.USUARIO_ID = :UserId
                            AND   n.DATA_LEITURA is null
                            AND   n.TIPO_NOTIFICACAO_ID = :Type
                            ORDER BY n.ID DESC";
            var records = await conn.QueryAsync<Notification>(query, new
                {
                    UserId = userId,
                    Type = type
                });

                return new ResponseDto<Notification>
                {
                    Data = records.ToList(),
                };
        }


    }
}
