using Users.API.Interfaces.Services;
using Users.API.Dto.User;
using Users.API.Interfaces.Repositories;
using AutoMapper;
using System.Threading.Tasks;
using System.Collections.Generic;
using Users.API.Entities;
using System;
using Newtonsoft.Json;
using Users.API.Common.Dto;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Users.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserShopRepository _userShopRepository;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IAuthService _authService;
        //Automapper
        private readonly IMapper _mapper;

        private RabbitService rabbitService;

        public UserService(IUserRepository userRepository,
            IUserShopRepository userShopRepository,
            IActionContextAccessor actionContextAccessor,
            IAuthService authService,
                              IMapper mapper,
            IConfiguration _configuration)
        {
            _userRepository = userRepository;
            _userShopRepository = userShopRepository;
            _actionContextAccessor = actionContextAccessor;
            _authService = authService;
            _mapper = mapper;
            rabbitService = new RabbitService(_configuration);
        }

        public async Task<ResponseDto<UserDto>> GetAllPaged(UserFilterDto filter, int limit, int skip, string order)
        {
            var entities = await _userRepository.GetAllPaged(filter, limit, skip, order);
            var users = _mapper.Map<ResponseDto<UserDto>>(entities);
            foreach(var user in users.Data)
            {
                user.UserTypeDescription = user.UserType.ToString();
                var shops = await _userRepository.GetShopsByUser(user.Id);
                if (user.GroupId != null)
                {
                    var group = await _userRepository.GetGroupById((long)user.GroupId, _mapper.Map<User>(user));
                    if (group != null)
                    {
                        user.GroupName = group.Name;
                    }
                }
                if (shops != null && shops.Count > 0)
                {
                    var shop = shops.FirstOrDefault();
                    user.ShopId = shop.Id;
                    user.ShopName = shop.Name;
                    user.ShopDocument = shop.Cnpj;
                    
                }
                if (user.CreatedBy != null && user.CreatedBy > 0)
                {
                    user.CreatedByUser = _mapper.Map<UserDto>(await _userRepository.GetUserById(user.CreatedBy));
                    if (user.CreatedByUser != null)
                        user.CreatedByUser.UserTypeDescription = user.CreatedByUser.UserType.ToString();
                }
            }
            return users;
        }
        public async Task<List<UserDto>> GetAll(UserFilterDto filter)
        {
            var entities = await _userRepository.GetAll(filter);
            var users = _mapper.Map<List<UserDto>>(entities);
            foreach (var user in users)
            {
                user.UserTypeDescription = user.UserType.ToString();
                var shops = await _userRepository.GetShopsByUser(user.Id);
                if (user.GroupId != null)
                {
                    var group = await _userRepository.GetGroupById((long)user.GroupId, _mapper.Map<User>(user));
                    if (group != null)
                    {
                        user.GroupName = group.Name;
                    }
                }
                if (shops != null && shops.Count > 0)
                {
                    var shop = shops.FirstOrDefault();
                    user.ShopId = shop.Id;
                    user.ShopName = shop.Name;
                    user.ShopDocument = shop.Cnpj;

                }
                if (user.CreatedBy != null && user.CreatedBy > 0)
                {
                    user.CreatedByUser = _mapper.Map<UserDto>(await _userRepository.GetUserById(user.CreatedBy));
                    if (user.CreatedByUser != null)
                        user.CreatedByUser.UserTypeDescription = user.CreatedByUser.UserType.ToString();
                }
            }
            return users;
        }

        public async Task<ResponseDto<UserDto>> GetAllPaged(long? groupId, int sistema_id, int limit, int skip, string search, string order)
        {
            var entities = await _userRepository.GetAllPaged(groupId, sistema_id, limit, skip, search, order);
            var dtos = _mapper.Map<ResponseDto<UserDto>>(entities);
            foreach (var user in dtos.Data)
            {
                user.UserTypeDescription = user.UserType.ToString();
                var shops = await _userRepository.GetShopsByUser(user.Id);
                if (user.GroupId != null)
                {
                    var group = await _userRepository.GetGroupById((long)user.GroupId, _mapper.Map<User>(user));
                    if (group != null)
                    {
                        user.GroupName = group.Name;
                    }
                }
                if (shops != null && shops.Count > 0)
                {
                    var shop = shops.FirstOrDefault();
                    user.ShopId = shop.Id;
                    user.ShopName = shop.Name;
                    user.ShopDocument = shop.Cnpj;

                }
                if (user.CreatedBy != null && user.CreatedBy > 0)
                {
                    user.CreatedByUser = _mapper.Map<UserDto>(await _userRepository.GetUserById(user.CreatedBy));
                    if (user.CreatedByUser != null)
                        user.CreatedByUser.UserTypeDescription = user.CreatedByUser.UserType.ToString();
                }
            }
            return dtos;
        }
        public async Task<IEnumerable<UserDto>> GetAll()
        {
            return _mapper.Map<IEnumerable<UserDto>>(await _userRepository.GetAll());
        }

        public async Task<UserDto> Get(long id)
        {
            var user = _mapper.Map<UserDto>(await _userRepository.Get(id));
            user.UserShop = _mapper.Map<List<UserShopDto>>(await _userShopRepository.GetByUserId(id));
            return user;
        }

        public async Task<UserDto> Add(UserAddDto user, int userId)
        {
            var existingCpf = await _userRepository.GetUserByCpf(user.Cpf);
            if (existingCpf != null)
            {
                //throw new Exception("CPF já cadastrado para outro usuário.");
                _actionContextAccessor.ActionContext.ModelState.AddModelError(nameof(user.Cpf), "CPF já cadastrado para outro usuário.");
                return null;
            }
            var existing = await _userRepository.GetUserByEmail(user.Email.ToLower());
            if (existing != null)
            {
                //throw new Exception("E-mail já cadastrado para outro usuário.");
                _actionContextAccessor.ActionContext.ModelState.AddModelError(nameof(user.Email), "E-mail já cadastrado para outro usuário.");
                return null;
            }
            var existingPhone = await _userRepository.GetUserByPhone(user.Telefone);
            if (existingPhone != null)
            {
                //throw new Exception("Celular já cadastrado para outro usuário.");
                _actionContextAccessor.ActionContext.ModelState.AddModelError(nameof(user.Telefone), "Celular já cadastrado para outro usuário.");
                return null;
            }
            if (user.UserType == Common.Enum.User.UserTypeEnum.Convencional && user.UserConvenios.Count == 0)
            {
                //throw new Exception("Selecione ao menos um convênio");
                _actionContextAccessor.ActionContext.ModelState.AddModelError(nameof(user.UserType), "Selecione ao menos um convênio");
                return null;
            }
            var entity = _mapper.Map<User>(user);
            entity.CreatedBy = userId;
            var newuser = await _userRepository.Add(entity);
            if (newuser.Id > 0)
            {
                foreach(var uc in user.UserConvenios)
                {
                    UserShop newUserShop = new UserShop()
                    {
                        ShopId = uc.ConvenioId,
                        ProfileId = uc.ProfileId,
                        UserId = newuser.Id
                    };
                    await _userShopRepository.Add(newUserShop);

                }
                await SendWelcome(newuser);
            }
            return _mapper.Map<UserDto>(newuser);
        }

        public async Task<UserDto> Update(UserUpdateDto user)
        {
            var existing = await _userRepository.GetUserByEmail(user.Email);
            if (existing != null && existing.Id != user.Id)
            {
                throw new Exception("E-mail já cadastrado para outro usuário");
            }
            var existingPhone = await _userRepository.GetUserByPhone(user.Telefone);
            if (existingPhone != null && existingPhone.Id != user.Id)
            {
                throw new Exception("Celular já cadastrado para outro usuário");
            }
            var res = await _userRepository.Update(_mapper.Map<User>(user));
            await _userShopRepository.Delete(user.Id);
            foreach (var uc in user.UserConvenios)
            {
                UserShop newUserShop = new UserShop()
                {
                    ShopId = uc.ConvenioId,
                    ProfileId = uc.ProfileId,
                    UserId = user.Id
                };
                await _userShopRepository.Add(newUserShop);

            }
            return _mapper.Map<UserDto>(res);
        }

        public async Task Activate(long[] ids)
        {
            foreach (var id in ids)
            {
                await _userRepository.Activate(id);
                var existing = await _userRepository.GetUserById(id);
                if (existing != null)
                {
                    await _authService.GenerateRecoverLink(existing.Email);
                }
            }
        }

        public async Task Deactivate(long[] ids)
        {
            foreach (var id in ids)
            {
                await _userRepository.Deactivate(id);
            }
        }

        public async Task Cancel(long[] ids)
        {
            foreach (var id in ids)
            {
                await _userRepository.Cancel(id);
            }
        }

        public async Task<UserDto> UpdateStatus(UserStatusDto user)
        {
            var res = await _userRepository.UpdateStatus(_mapper.Map<User>(user));
            return _mapper.Map<UserDto>(res);
        }
        public async Task Delete(long id)
        {
            await _userRepository.Delete(id);
        }
        public async Task<bool> SendWelcome(User user)
        {
            user.RecoverPasswordToken = Guid.NewGuid().ToString();
            user.RecoverPasswordTokenExp = DateTime.Now.AddDays(1);

            await _userRepository.GenerateRecoverToken(user);
            var firstName = user.Name.Split(' ')[0];
            var baseUrl = "https://empresas.bullla.com.br/reset-password/";
            await Task.Run(() => rabbitService.Publish("send-email", JsonConvert.SerializeObject(new SendEmailMessage()
            {
                from = "atendimento@bullla.com.br",
                to = user.Email.ToLower(),
                subject = "Seja bem vindo ao Bullla Empresas",
                html = $@"<p>Olá, {firstName}!</p><p>Tudo bem?</p>Seu cadastro no <b>Bullla Empresas</b> foi realizado com sucesso!😊</p><p>Acesse o link abaixo para criar uma senha e acessar o portal.</p><p class=""text-center""><a href=""{baseUrl}{user.RecoverPasswordToken}"" class=""btn-link"">Criar senha</a></p>",
                text = $@"Olá, {firstName}! Seu cadastro no Bullla Empresas foi realizado com sucesso. Acesse o link abaixo para criar uma senha e acessar o portal. Acesse o link {baseUrl}{user.RecoverPasswordToken}"
            })));

            return true;
        }

         public async Task<List<UserMenuDto>> GetAllbyMenu(string searchTerm)
        {
            return _mapper.Map<List<UserMenuDto>>(await _userRepository.GetAllbyMenu(searchTerm));
        }
        
    }
}