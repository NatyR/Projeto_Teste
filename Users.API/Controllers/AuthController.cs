using Castle.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Users.API.Common.Attributes;
using Users.API.Dto.Auth;
using Users.API.Entities;
using Users.API.Exceptions;
using Users.API.Interfaces.Repositories;
using Users.API.Interfaces.Services;
using Users.API.Repositories;
using Users.API.Services;
using Users.API.ViewModels;

namespace Users.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthService service, ILogger<AuthController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
        [Route("signin")]
        public async Task<ActionResult<dynamic>> Signin([FromBody] SignInDto user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    string message = "";
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            message += error.ErrorMessage + " ";
                        }
                    }
                    return BadRequest(message );
                }
                _logger.LogInformation("Signin");
                return Ok(await _service.Signin(user));
            } catch(InvalidUserLoginException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("signup")]
        public async Task<ActionResult<dynamic>> Signup([FromBody] User user)
        {
            try
            {
                _logger.LogInformation("Signup");
                return Ok(await _service.Signup(user));
            } catch (UserAlreadyExistsException e)
            { 
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("me")]
        [Authorize]
        public async Task<ActionResult<User>> GetLoggedUser()
        {
            try
            {
                _logger.LogInformation("me");
                var userId = Convert.ToInt64(User.Claims.FirstOrDefault(x => x.Type == "id").Value);
                return Ok(await _service.GetUserById(userId));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("generate-reset-token")]
        public async Task<ActionResult<User>> GenerateRecoverLink([FromBody] GenerateRecoverLinkViewModel body)
        {
            try
            {
                _logger.LogInformation("generate-reset-token");
                return Ok(await _service.GenerateRecoverLink(body.Email));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message + e.InnerException.InnerException.Message);
            }
        }
        
        [HttpPost]
        [Route("reset-password")]
        public async Task<ActionResult<User>> RecoverPassword([FromBody] RecoverPasswordViewModel body)
        {
            try
            {
                _logger.LogInformation("reset-password");
                return Ok(await _service.ResetPassword(body.Password, body.Token));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("check-reset-token")]
        public async Task<ActionResult<dynamic>> CheckResetToken([FromBody] CheckTokenViewModel body)
        {
            try
            {
                _logger.LogInformation("check-reset-token");
                return Ok(new { valid = await _service.CheckResetToken(body.Token) });
            } catch (Exception e)
            {
                return BadRequest(new { valid = false, message = e.Message });
            }
        }

    }
}
