using Portal.API.Common.Dto;
using Portal.API.Dto.Menu;
using Portal.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task<List<long>> GetUsersByGroup(long groupId);
        Task<List<long>> GetUsersByShop(long groupId,long shopId);
        Task<List<Notification>> GetActiveNotifications(long userId);
        Task<ResponseDto<Notification>> GetAllPaged(int limit, int skip, string order, int idUser);
        Task<List<Notification>> GetAll(long userId);
        Task<Notification> Get(long id);
        Task<List<NotificationType>> GetTypes();
        Task<ResponseDto<Notification>> GetActiveNotificationsByType(long UserId, int Type);
        Task<Notification> Create(Notification notification);
        Task<Notification> Update(Notification notification);
        Task Delete(long id);
        Task Read(long userId, long[] ids);
        Task Unread(long userId, long[] ids);
        Task Remove(long userId, long[] ids);
    }
}
