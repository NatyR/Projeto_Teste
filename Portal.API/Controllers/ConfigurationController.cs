
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Common.Annotations.Validations;
using Portal.API.Dto.Configuration;
using Portal.API.Entities;
using Portal.API.Integrations.Bullla.Interfaces;
using Portal.API.Integrations.Ploomes.Dto;
using Portal.API.Interfaces.Repositories;
using Portal.API.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Portal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationRepository _repository;
        private readonly IConfigurationService _configurationService;
        private readonly IBulllaIntegration _bulllaIntegration;

        public ConfigurationController(IConfigurationRepository repository,
            IBulllaIntegration bulllaIntegration,
            IConfigurationService configurationService)
        {
            _bulllaIntegration = bulllaIntegration;
            _repository = repository;
            _configurationService = configurationService;
        }
        // GET: api/<ConfigurationController>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<Configuration>> GetConfiguration()
        {
            var configuration = await _repository.GetConfiguration();
            return Ok(configuration);
        }

        [HttpGet("get-integration/{convenio}")]
        [Authorize]
        public async Task<IActionResult> GetIntegration(int convenio)
        {
            var email = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            
            var result = _bulllaIntegration.GetShops(email);

            var existente = result.Shops.Where(s => s.CompanyCode == convenio.ToString()).FirstOrDefault();

            //if(existente == null || String.IsNullOrEmpty(existente.CompanyCode))
            //{
            //    return NotFound();
            //}
            result.Email = email;
            
            return Ok(result);
        }


        [HttpPut]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put(int id, [FromBody] ConfigurationUpdateDto config)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _configurationService.Update(config);
            return Ok(config);
        }

        [HttpGet("owner/{grupo}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<PloomesOwnerDto>> GetOwnser([FromRoute] int grupo)
        {
            var owner = await _configurationService.GetOwner(grupo);
            return Ok(owner);
        }
    }
}
