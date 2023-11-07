using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Common.Annotations.Validations;
using Portal.API.Common.Dto;
using Portal.API.Dto.Manual;
using Portal.API.Entities;
using Portal.API.Interfaces.Repositories;
using Portal.API.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManualController : Controller
    {
        private readonly IManualService _manualService;
        public ManualController(IManualService manualService)
        {
            _manualService = manualService;
        }


        [HttpPost]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromForm] ManualAddDto manual)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var newRecord = await _manualService.Create(manual);
            return CreatedAtAction("Get", new { id = newRecord.Id }, manual);
        }

        [HttpPut("{id}")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put(int id, [FromForm] ManualUpdateDto manual)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != manual.Id)
            {
                return BadRequest();
            }
            
            await _manualService.Update(manual);
            return Ok(manual);
        }
        [HttpPut("activate")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Activate([FromBody] long[] ids)
        {

            await _manualService.Activate(ids);
            return Ok();
        }
        [HttpPut("deactivate")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Deactivate([FromBody] long[] ids)
        {

            await _manualService.Deactivate(ids);
            return Ok();
        }
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _manualService.Delete(id);
            return NoContent();
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var faq = await _manualService.Get(id);
            if (faq == null)
            {
                return NotFound();
            }
            return Ok(faq);
        }

        [HttpGet("{id}/download")]
        [Authorize]
        public async Task<IActionResult> Download(int id)
        {
            var manual = await _manualService.Get(id);
            if (manual == null)
            {
                return NotFound();
            }
            var url = _manualService.GetSignedUrl(manual);
            return Redirect(url);
        }

        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var faqs = await _manualService.GetAll();
            return Ok(faqs);
        }

        [HttpGet("active")]
        [Authorize]
        public async Task<IActionResult> GetAllActive()
        {
            var manuals = await _manualService.GetActiveManuals();
            foreach(var manual in manuals)
            {
                if (manual.ManualType == "arquivo" && !String.IsNullOrEmpty(manual.FileName))
                {
                    manual.FileName = _manualService.GetSignedUrl(manual);
                }
            }
            return Ok(manuals);
        }

        [HttpGet("paged/{manualType}")]
        [Authorize]
        [ProducesResponseType(typeof(List<ManualDto>), 200)]
        public async Task<ActionResult<ResponseDto<ManualDto>>> GetAllPaged([FromRoute]string manualType, [FromQuery] int limit, [FromQuery] int skip, [FromQuery] string search, [FromQuery] string order)
        {


            var response = await _manualService.GetAllPaged(manualType, limit, skip, search, order);
            return Ok(response);
        }
        // GET: api/<ConfigurationController>
        //[HttpGet("active")]
        //public async Task<IEnumerable<ManualDto>> GetActiveManuals()
        //{
        //    return await _manualService.GetActiveMa();
        //}
    }
}
