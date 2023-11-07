using Accounts.API.Common.Dto.CostCenter;
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
    public class CostCentersController : Controller
    {
        private readonly ICostCenterService _service;

        public CostCentersController(ICostCenterService service)
        {
            _service = service;
        }

        [HttpGet("{convenio}")]
        [Authorize]
        [ProducesResponseType(typeof(List<CostCenterDto>), 200)]
        public async Task<ActionResult<List<CostCenterDto>>> GetAllByConvenio([FromRoute] int convenio)
        {
            var response = await _service.GetAllByConvenio(convenio);
            return Ok(response);
        }
    }
}
