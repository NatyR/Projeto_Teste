using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users.API.Dto.Auth;
using Users.API.Entities;

namespace Users.API.Interfaces.Services
{
    public interface IAuthService
    {
        public Task<dynamic> Signin(SignInDto user);
        public Task<bool> Signup(User user);
        public Task<User> GetUserById(long id);

        public Task<bool> GenerateRecoverLink(string email);

        public Task<dynamic> ResetPassword(string password, string token);

        public Task<bool> CheckResetToken(string token); 
    }
}
