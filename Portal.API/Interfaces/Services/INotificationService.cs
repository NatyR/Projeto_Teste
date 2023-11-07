using Portal.API.Common.Dto;
using Portal.API.Dto.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Services
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetAll(long userId);
        Task<List<NotificationDto>> GetActiveNotifications(int userId);
        Task<ResponseDto<NotificationDto>> GetAllPaged(int limit, int skip, string order, int idUser);
        Task<NotificationDto> Get(long id);
        Task<List<NotificationTypeDto>> GetTypes();
        Task<ResponseDto<NotificationDto>> GetActiveNotificationsByType(long UserId, int Type);
        Task<NotificationDto> Create(NotificationAddDto notification);
        Task<NotificationDto> Update(NotificationUpdateDto notification);
        Task Delete(long id);

        Task Read(long userId, long[] ids);
        Task Unread(long userId, long[] ids);
        Task Remove(long userId, long[] ids);

    }
}
