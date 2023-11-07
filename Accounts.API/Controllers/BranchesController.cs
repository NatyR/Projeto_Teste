using Accounts.API.Common.Dto.Branch;
using Accounts.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchesController : Controller
    {
        private readonly IBranchService _service;

        public BranchesController(IBranchService service)
        {
            _service = service;
        }

        [HttpGet("{convenio}")]
        [Authorize]
        [ProducesResponseType(typeof(List<BranchDto>), 200)]
        public async Task<ActionResult<List<BranchDto>>> GetAllByConvenio([FromRoute] int convenio)
        {
            var response = await _service.GetAllByConvenio(convenio);
            return Ok(response);
        }
    }
}
