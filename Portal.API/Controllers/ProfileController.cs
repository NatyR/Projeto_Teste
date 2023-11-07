using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Common.Annotations.Validations;
using Portal.API.Common.Dto;
using Portal.API.Dto.Profile;
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
    public class ProfileController : Controller
    {
        private readonly IProfileServicePortal _profileService;
        public ProfileController(IProfileServicePortal profileService)
        {
            _profileService = profileService;
        }


        [HttpPost]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] PortalProfileAddDto profile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var newRecord = await _profileService.Create(profile);
            return CreatedAtAction("Get", new { id = newRecord.Id }, profile);
        }

        [HttpPut("{id}")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put(int id, [FromBody] PortalProfileUpdateDto profile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != profile.Id)
            {
                return BadRequest();
            }

            await _profileService.Update(profile);
            return Ok(profile);
        }
        //[HttpPut("activate")]
        //[Authorize]
        //[ValidForm]
        //[ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        //public async Task<IActionResult> Activate([FromBody] long[] ids)
        //{

        //    await _profileService.Activate(ids);
        //    return Ok();
        //}
        //[HttpPut("deactivate")]
        //[Authorize]
        //[ValidForm]
        //[ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        //public async Task<IActionResult> Deactivate([FromBody] long[] ids)
        //{

        //    await _profileService.Deactivate(ids);
        //    return Ok();
        //}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _profileService.Delete(id);
            return Ok("Desativado com sucesso.");
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var res = await _profileService.Get(id);
            if (res == null)
            {
                return NotFound();
            }
            return Ok(res);
        }

       

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var faqs = await _profileService.GetAll();
            return Ok(faqs);
        }

        [HttpGet("paged")]
        [Authorize]
        [ProducesResponseType(typeof(List<PortalProfileDto>), 200)]
        public async Task<ActionResult<ResponseDto<PortalProfileDto>>> GetAllPaged([FromQuery] int limit, [FromQuery] int skip, [FromQuery] string search, [FromQuery] string order)
        {


            var response = await _profileService.GetAllPaged(limit, skip, search, order);
            return Ok(response);
        }
    }
}
