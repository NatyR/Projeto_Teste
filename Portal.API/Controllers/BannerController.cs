using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Common.Annotations.Validations;
using Portal.API.Common.Dto;
using Portal.API.Dto.Banner;
using Portal.API.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannerController : Controller
    {
        private readonly IBannerService _bannerService;
        public BannerController(IBannerService bannerService)
        {
            _bannerService = bannerService;

        }



        [HttpPost]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromForm] BannerAddDto faq)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var newFaq = await _bannerService.Create(faq);
            return CreatedAtAction("Get", new { id = newFaq.Id }, faq);
        }
        [HttpPut("activate")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Activate([FromBody] long[] ids)
        {

            await _bannerService.Activate(ids);
            return Ok();
        }
        [HttpPut("deactivate")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Deactivate([FromBody] long[] ids)
        {

            await _bannerService.Deactivate(ids);
            return Ok();
        }
        [HttpPut("{id}")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put(int id, [FromForm] BannerUpdateDto faq)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != faq.Id)
            {
                return BadRequest();
            }
            await _bannerService.Update(faq);
            return Ok(faq);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _bannerService.Delete(id);
            return NoContent();
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var faq = await _bannerService.Get(id);
            if (faq == null)
            {
                return NotFound();
            }
            return Ok(faq);
        }

    
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            var faqs = await _bannerService.GetActiveBanners();
            return Ok(faqs);
        }

        [HttpGet("paged")]
        [Authorize]
        [ProducesResponseType(typeof(List<BannerDto>), 200)]
        public async Task<ActionResult<ResponseDto<BannerDto>>> GetAllPaged([FromQuery] int limit, [FromQuery] int skip, [FromQuery] string search, [FromQuery] string order)
        {


            var response = await _bannerService.GetAllPaged(limit, skip, search, order);
            return Ok(response);
        }
        // GET: api/<ConfigurationController>
        [HttpGet("active")]
        public async Task<IEnumerable<BannerDto>> GetActiveBanners()
        {
            return await _bannerService.GetActiveBanners();
        }
    }
}
