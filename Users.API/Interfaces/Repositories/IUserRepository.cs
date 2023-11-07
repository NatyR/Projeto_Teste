using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Users.API.Common.Dto;
using Users.API.Dto.User;
using Users.API.Entities;

namespace Users.API.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAll();
        Task<ResponseDto<User>> GetAllPaged(long? groupId, int sistema_id, int limit, int skip, string search, string order);
        Task<ResponseDto<User>> GetAllPaged(UserFilterDto filter, int limit, int skip, string order);
        Task<List<User>> GetAll(UserFilterDto filter);
        Task<List<UserMenuDto>> GetAllbyMenu(string searchTerm);
        Task<User> Get(long id);
        Task<User> Add(User user);
        Task<User> Update(User user);
        Task<User> UpdateStatus(User user);
        Task Delete(long id);

        Task<IEnumerable<User>> GetAllUsers();
        Task<User> GetUserById(long id);
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserByCpf(string cpf);
        Task<User> GetUserByPhone(string phone);
        Task CreateUser(User user);
        Task RegisterFailedLogin(User user);
        Task ResetFailedLogin(User user);
        Task<int> GetMaxCod();
        Task<bool> GenerateRecoverToken(User user);
        Task<bool> ChangePassword(User user);
        Task<User> GetUserByRecoverToken(string token);
        Task<List<Group>> GetGroupsByUser(User user, string search);
        Task<Group> GetGroupById(long id, User user);
        Task<List<Group>> GetAllGroups();
        Task<List<Shop>> GetShopsByGroup(long? groupId);
        Task<List<Shop>> GetShopsByUser(long? userId);
        Task Activate(long id);
        Task Deactivate(long id);
        Task Cancel(long id);
        Task LockOrUnlock(long id, string status);

        Task<List<string>> PasswordOkToSave(long userId);
        Task SaveOldEmail(long userId, string oldPassword, DateTime lastChange);
        Task<Login> LastLogin(string email);

        Task<List<Notification>> GetAllUserNotification(long userId);
        Task<Notification> CreateUserNotification(Notification notification);
        Task<Notification> UpdateUserNotification(Notification notification);
        Task DeleteUserNotificaction(long userId,long[] ids);
    }
}
