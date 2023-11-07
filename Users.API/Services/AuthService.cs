using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users.API.Entities;
using Users.API.Exceptions;
using Users.API.Interfaces.Repositories;
using Users.API.Interfaces.Services;
using Newtonsoft.Json;
using Users.API.Dto.Auth;
using Microsoft.Extensions.Configuration;

namespace Users.API.Services
{
    public class AuthService : IAuthService
    {
        private IUserRepository _repository;
        private ILoginRepository _loginRepository;
        private IHttpService _httpService;
        private RabbitService rabbitService;
        public AuthService(IUserRepository repository,
            ILoginRepository loginRepository,
            IHttpService httpService,
            IConfiguration _configuration)
        {
            _repository = repository;
            _loginRepository = loginRepository;
            _httpService = httpService;
            rabbitService = new RabbitService(_configuration);
        }

        public async Task<User> GetUserById(long id)
        {
            User user = await _repository.GetUserById(id);

            user.Password = "";

            if (user is null)
            {
                throw new Exception("Usuário não encontrado");
            }

            return user;
        }


        public async Task<dynamic> Signin(SignInDto user)
        {
            var existsUser = await _repository.GetUserByEmail(user.Email.ToLower());
            if (existsUser == null) throw new InvalidUserLoginException("Dados inválidos (e-mail ou senha não conferem)");
            Login login = new Login();
            login.SetIP(_httpService.GetRequestIP());
            login.SetEmail(user.Email.ToLower());
            login.SetUserAgent(_httpService.GetUserAgent());
            var canLogin = await _repository.LastLogin(user.Email.ToLower());
            var userLastLogin = await _repository.Get(existsUser.Id);


            if (existsUser != null)
            {
                login.SetUsuarioId(existsUser.Id);
            }

            var maxDate = DateTime.Now.AddDays(-45);
            if (canLogin != null && canLogin.LastLogin < maxDate)
            {
                await _repository.Deactivate(existsUser.Id);
                throw new InvalidUserLoginException("Seu último login foi a mais de 45 dias. Usuário bloqueado, por favor, entre em contato com o suporte.");
            }

            if (userLastLogin != null)
            {
                TimeSpan lastLogin = ((TimeSpan)(DateTime.Now - userLastLogin.RecoverPasswordTokenExp));
                int daysRemaining = lastLogin.Days - 50;
                var allNotifications = await _repository.GetAllUserNotification(existsUser.Id);
                if (daysRemaining == 10 || (daysRemaining <= 5 && daysRemaining > 1))
                {
                    string pluralize = daysRemaining > 1 ? "s" : "";
                    string notification = $"Sua senha ira expirar em {daysRemaining} dia{pluralize}";



                    var newNotification = new Notification();
                    newNotification.Description = notification;
                    newNotification.Title = "Senha ira expirar em breve";
                    newNotification.CreatedAt = DateTime.Now;
                    newNotification.UserId = existsUser.Id;

                    var existsNotification = allNotifications.Where(x => x.Title == newNotification.Title && x.ReadAt == null).FirstOrDefault();


                    // Update notification
                    if (existsNotification == null)
                    {
                        await _repository.CreateUserNotification(newNotification);
                    }
                    else if (existsNotification.ReadAt == null)
                    {
                        newNotification.Id = existsNotification.Id;
                        await _repository.UpdateUserNotification(newNotification);
                    }
                }
                else if (lastLogin.Days >= 60)
                {
                    var ids = allNotifications.Select(x => x.Id);
                    if (ids.Any())
                    {
                        await _repository.DeleteUserNotificaction(existsUser.Id, ids.ToArray());
                    }
                    await GenerateRecoverLink(existsUser.Email);
                    throw new InvalidUserLoginException("Sua senha está expirada, reenviamos um novo e-mail para o cadastro de uma nova senha!");
                }
            }


            if (existsUser != null && existsUser.Status != "ATIVO")
            {
                login.SetStatus("INATIVO");
                await _loginRepository.Add(login);
                throw new InvalidUserLoginException("Dados inválidos (e-mail ou senha não conferem)");
            }

            if (existsUser != null && existsUser.FailedLogins > 4 && existsUser.Status == "INATIVO")
            {
                login.SetStatus("BLOQUEADO");
                await _loginRepository.Add(login);
                throw new InvalidUserLoginException("Conta bloqueada.");
            }

            if (existsUser != null && existsUser.Password == null)
            {
                login.SetStatus("SENHA_NAO_CADASTRADA");
                await _loginRepository.Add(login);
                throw new InvalidUserLoginException("Conta pendente de ativação.");
            }

            if (existsUser is null || !BCrypt.Net.BCrypt.Verify(user.Password, existsUser.Password))
            {
                login.SetStatus("INVALIDO");
                await _loginRepository.Add(login);
                if (existsUser != null && existsUser.FailedLogins <= 2)
                {
                    //Incremente failed logins
                    await _repository.RegisterFailedLogin(existsUser);
                    //se igual a 4 avisa utilize o esqueci minha senha.
                    if (existsUser.FailedLogins >= 2)
                    {
                        throw new InvalidUserLoginException("Dados inválidos (e-mail ou senha não conferem). Você tem só mais uma tentativa. Utilize o Esqueci minha senha.");
                    }
                }
                throw new InvalidUserLoginException("Dados inválidos (e-mail ou senha não conferem)");
            }



            login.SetStatus("SUCESSO");
            await _loginRepository.Add(login);
            await _repository.ResetFailedLogin(existsUser);

            string token = TokenService.GenerateToken(existsUser);

            existsUser.Password = "";
            
            return new
            {
                user = existsUser,
                token
            };
        }

        public async Task<bool> Signup(User model)
        {
            User user = model;

            User alreadyExistsUser = await _repository.GetUserByEmail(user.Email.ToLower());

            if (alreadyExistsUser is not null)
            {
                if (user.Email.ToLower() == alreadyExistsUser.Email.ToLower())
                {
                    throw new UserAlreadyExistsException($"O e-mail {user.Email.ToLower()} já está em uso.");
                }
            }

            int maxCod = await _repository.GetMaxCod();

            user.Id = maxCod;
            //user.CreatedAt = DateTime.Now;
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            await _repository.CreateUser(user);

            return true;
        }

        public async Task<bool> GenerateRecoverLink(string email)
        {
            User userExists = await _repository.GetUserByEmail(email);

            if (userExists is null)
            {
                throw new Exception("Usuário inválido.");
            }

            userExists.RecoverPasswordToken = Guid.NewGuid().ToString();
            userExists.RecoverPasswordTokenExp = DateTime.Now.AddDays(1);
            await _repository.GenerateRecoverToken(userExists);

            var firstName = userExists.Name.Split(' ')[0];
            await Task.Run(() => rabbitService.Publish("send-email", JsonConvert.SerializeObject(new SendEmailMessage()
            {
                from = "atendimento@bullla.com.br",
                to = email,
                subject = "Recuperação de senha",
                html = @$"<p><strong>Oi {firstName},</strong><br>Recebemos uma solicitação de redefinição da sua senha, clique abaixo para cadastrar sua nova senha.</p><p class=""text-center""><a href=""https://empresas.bullla.com.br/reset-password/{userExists.RecoverPasswordToken}"" class=""btn-link"">Criar senha</a></p><p>Caso não tenha solicitado, não se preocupe, apenas desconsidere este e-mail.</p>",
                text = @$"Oi {firstName}. Recebemos uma solicitação de redefinição da sua senha, clique abaixo para cadastrar sua nova senha. Acesse o link https://empresas.bullla.com.br/reset-password/" + userExists.RecoverPasswordToken
            })));

            return true;
        }

        public async Task<dynamic> ResetPassword(string password, string token)
        {
            User userExists = await _repository.GetUserByRecoverToken(token);

            if (userExists is null)
            {
                throw new Exception("Usuário inválido.");
            }


            List<string> listUserPassword = await _repository.PasswordOkToSave(userExists.Id);

            listUserPassword.ForEach(hashPassword =>
            {
                if (hashPassword != null && BCrypt.Net.BCrypt.Verify(password, hashPassword) && userExists?.Status == "ATIVO")
                {
                    throw new Exception("Não é possivel usar uma senha que já foi usada anteriormente.");
                }
            });

            if (userExists?.Status == "INATIVO")
            {
                throw new Exception("Usuário bloqueado por um administrador, por favor, entre em contato com o suporte.");
            }

            if (DateTime.Now > userExists.RecoverPasswordTokenExp)
            {
                await GenerateRecoverLink(userExists.Email);
                throw new Exception("Seu token está expirado, reenviamos um novo e-mail para o cadastro da senha!");
            }

            await _repository.SaveOldEmail(userExists.Id, userExists.Password, DateTime.Now);

            userExists.Password = BCrypt.Net.BCrypt.HashPassword(password);
            userExists.RecoverPasswordToken = null;

            await _repository.ChangePassword(userExists);
            await _repository.GenerateRecoverToken(userExists);

            Login login = new Login();
            login.SetIP(_httpService.GetRequestIP());
            login.SetEmail(userExists.Email.ToLower());
            login.SetUserAgent(_httpService.GetUserAgent());
            login.SetStatus("SUCESSO");
            await _loginRepository.Add(login);
            await _repository.ResetFailedLogin(userExists);

            userExists.Password = "";

            return new
            {
                user = userExists,
                token = TokenService.GenerateToken(userExists)
            };
        }

        public async Task<bool> CheckResetToken(string token)
        {
            User userExists = await _repository.GetUserByRecoverToken(token);

            if (userExists is null || userExists.Status != "ATIVO" || DateTime.Now > userExists.RecoverPasswordTokenExp)
            {
                throw new Exception("Token inválido");
            }

            return true;
        }
    }
}
