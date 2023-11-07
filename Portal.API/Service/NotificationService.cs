using Amazon.S3.Model;
using AutoMapper;
using Portal.API.Common.Dto;
using Portal.API.Dto.Menu;
using Portal.API.Dto.Notification;
using Portal.API.Entities;
using Portal.API.Interfaces.Repositories;
using Portal.API.Interfaces.Services;
using Portal.API.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Service
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public NotificationService(INotificationRepository notificationRepository,
            IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }
        public async Task<NotificationDto> Create(NotificationAddDto notificacao)
        {
            if(notificacao.NotificationType is null || notificacao.NotificationType == 0)
                notificacao.NotificationType = Enum.NotificationTypeEnum.Outros;

            var entity = new Notification()
            {
                Title = notificacao.Title,
                Description = notificacao.Description,
                NotificationType = notificacao.NotificationType.Value

            };
            if (notificacao.Destination == "group")
            {
                var users = await _notificationRepository.GetUsersByGroup((long)notificacao.GroupId);
                foreach (var userId in users)
                {
                    entity.UserId = userId;
                    await _notificationRepository.Create(entity);
                }
                return _mapper.Map<NotificationDto>(entity);
            }
            else if (notificacao.Destination == "shop")
            {
                var users = await _notificationRepository.GetUsersByShop((long)notificacao.GroupId,(long)notificacao.ShopId);
                foreach (var userId in users)
                {
                    entity.UserId = userId;
                    await _notificationRepository.Create(entity);
                }
                var masters = await _notificationRepository.GetUsersByGroup((long)notificacao.GroupId);
                foreach (var userId in masters)
                {
                    entity.UserId = userId;
                    await _notificationRepository.Create(entity);
                }
                return _mapper.Map<NotificationDto>(entity);


            }
            else if (notificacao.Destination == "user")
            {
                entity.UserId = (long)notificacao.UserId;
                await _notificationRepository.Create(entity);
                return _mapper.Map<NotificationDto>(entity);
            }
            return null;
        }

        public async Task Delete(long id)
        {
            await _notificationRepository.Delete(id);
        }

        public async Task<NotificationDto> Get(long id)
        {
            return _mapper.Map<NotificationDto>(await _notificationRepository.Get(id));
        }

        public async Task<List<NotificationDto>> GetActiveNotifications(int userId)
        {
            return _mapper.Map<List<NotificationDto>>(await _notificationRepository.GetActiveNotifications(userId));
        }

        public async Task<List<NotificationDto>> GetAll(long userId)
        {
            return _mapper.Map<List<NotificationDto>>(await _notificationRepository.GetAll(userId));
        }

        public async Task<ResponseDto<NotificationDto>> GetAllPaged(int limit, int skip, string order, int idUser)
        {
            return _mapper.Map<ResponseDto<NotificationDto>>(await _notificationRepository.GetAllPaged(limit, skip, order, idUser));
        }

         public async Task<List<NotificationTypeDto>> GetTypes()
        {
            var entity = await _notificationRepository.GetTypes();
            return _mapper.Map<List<NotificationTypeDto>>(entity);
        }

        public async Task Read(long userId, long[] ids)
        {
            await _notificationRepository.Read(userId, ids);
        }

        public async Task Remove(long userId, long[] ids)
        {
            await _notificationRepository.Remove(userId, ids);
        }

        public async Task Unread(long userId, long[] ids)
        {
            await _notificationRepository.Unread(userId, ids);
        }

        public async Task<NotificationDto> Update(NotificationUpdateDto notification)
        {
            return _mapper.Map<NotificationDto>(await _notificationRepository.Update(_mapper.Map<Notification>(notification)));
        }

          public async Task<ResponseDto<NotificationDto>> GetActiveNotificationsByType(long userId, int type)
        {
            return _mapper.Map<ResponseDto<NotificationDto>>(await _notificationRepository.GetActiveNotificationsByType(userId, type));
        }
    }
}
