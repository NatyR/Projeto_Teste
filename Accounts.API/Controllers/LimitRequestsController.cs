using Accounts.API.Common.Annotations.Validations;
using Accounts.API.Common.Dto;
using Accounts.API.Common.Dto.Account;
using Accounts.API.Common.Dto.User;
using Accounts.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace Cards.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LimitRequestsController : ControllerBase
    {
        private readonly ILimitRequestService _service;

        public LimitRequestsController(ILimitRequestService service)
        {
            _service = service;
        }

        /// <summary>
        /// Endpoint responsável por listar os colaboradores de um convenio
        /// </summary>
        [HttpGet("paged/{convenio_id}")]
        [Authorize]
        [ProducesResponseType(typeof(List<AccountDto>), 200)]
        public async Task<ActionResult<ResponseDto<AccountDto>>> GetAllPaged([FromRoute] long convenio_id, [FromQuery] string status, [FromQuery] int limit, [FromQuery] int skip, [FromQuery] string search, [FromQuery] string order)
        {
            var response = await _service.GetAllPaged(convenio_id,status, limit, skip, search, order);
            return Ok(response);
        }

        [HttpPut("approve")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Approve([FromBody] long[] ids)
        {
            await _service.Approve(ids, GetCurrentUser());
            return Ok();
        }
        [HttpPut("reject")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Reject([FromBody] long[] ids)
        {
            await _service.Reject(ids, GetCurrentUser());
            return Ok();
        }

        private UserDtoAccounts GetCurrentUser()
        {
            return new UserDtoAccounts()
            {
                Id = Int32.Parse(User.Claims.First(c => c.Type == "id").Value),
                Email = User.Claims.First(c => c.Type == ClaimTypes.Email).Value,
                Name = User.Claims.First(c => c.Type == ClaimTypes.Name).Value,
                UserType = User.Claims.First(c => c.Type == "userType").Value,
            };
        }
    }
}
