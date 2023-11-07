using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users.API.Common.Annotations.Validations;
using Users.API.Dto.Profile;
using Users.API.Entities;
using Users.API.Interfaces.Repositories;
using Users.API.Interfaces.Services;
using Users.API.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Users.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly IProfileService _profileService;
        public ProfilesController(IProfileService profileService)
        {
            _profileService = profileService;
        }


        [HttpPost]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] ProfileAddDto profile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var newProfile = await _profileService.Add(profile);
            return CreatedAtAction("Get", new { id = newProfile.Id }, profile);
        }

        [HttpPut("{id}")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put(int id, [FromBody] ProfileUpdateDto profile)
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

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _profileService.Delete(id);
            return NoContent();
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var profile = await _profileService.Get(id);
            if (profile == null)
            {
                return NotFound();
            }
            return Ok(profile);
        }

        [HttpGet("sistema/{id}")]
        [Authorize]
        public async Task<IActionResult> GetBySistema(int id)
        {
            var profile = await _profileService.GetBySistema(id);
            if (profile == null)
            {
                return NotFound();
            }
            return Ok(profile);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var profiles = await _profileService.GetAll();
            return Ok(profiles);
        }



    }
}
