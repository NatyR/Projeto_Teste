using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users.API.Common.Annotations.Validations;
using Users.API.Common.Dto;
using Users.API.Dto.Login;
using Users.API.Entities;
using Users.API.Interfaces.Repositories;
using Users.API.Interfaces.Services;
using Users.API.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Users.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;
        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpPost("paged")]
        [Authorize]
        [ProducesResponseType(typeof(List<LoginDto>), 200)]
        public async Task<ActionResult<ResponseDto<LoginDto>>> GetAllPaged([FromBody] LoginFilterDto filterOptions, [FromQuery] int limit, [FromQuery] int skip, [FromQuery] string order)
        {


            var response = await _loginService.GetAllPaged(filterOptions, limit, skip, order);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var login = await _loginService.Get(id);
            if (login == null)
            {
                return NotFound();
            }
            return Ok(login);
        }

      



    }
}
