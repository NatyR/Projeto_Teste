﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Common.Annotations.Validations;
using Portal.API.Common.Dto;
using Portal.API.Dto.Faq;
using Portal.API.Entities;
using Portal.API.Interfaces.Repositories;
using Portal.API.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaqController : ControllerBase
    {
        private readonly IFaqRepository _repository;
        private readonly IFaqService _faqService;
        public FaqController(
            IFaqRepository repository,
            IFaqService faqService)
        {
            _repository = repository;
            _faqService = faqService;
        }
        // GET: api/<ConfigurationController>
        //[HttpGet]
        //public async Task<List<Faq>> GetActiveFaqs()
        //{
        //    return await _repository.GetActiveFaqs();
        //}

        [HttpPut("activate")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Activate([FromBody] long[] ids)
        {

            await _faqService.Activate(ids);
            return Ok();
        }
        [HttpPut("deactivate")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Deactivate([FromBody] long[] ids)
        {

            await _faqService.Deactivate(ids);
            return Ok();
        }

        [HttpPost]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] FaqAddDto faq)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}
            var newFaq = await _faqService.Create(faq);
            return CreatedAtAction("Get", new { id = newFaq.Id }, faq);
        }

        [HttpPut("{id}")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put(int id, [FromBody] FaqUpdateDto faq)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != faq.Id)
            {
                return BadRequest();
            }
            await _faqService.Update(faq);
            return Ok(faq);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _faqService.Delete(id);
            return NoContent();
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var faq = await _faqService.Get(id);
            if (faq == null)
            {
                return NotFound();
            }
            return Ok(faq);
        }

        [HttpGet("paged")]
        [Authorize]
        [ProducesResponseType(typeof(List<FaqDto>), 200)]
        public async Task<ActionResult<ResponseDto<FaqDto>>> GetAllPaged([FromQuery] int limit, [FromQuery] int skip, [FromQuery] string search, [FromQuery] string order)
        {


            var response = await _faqService.GetAllPaged(limit, skip, search, order);
            return Ok(response);
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            var faqs = await _faqService.GetActiveFaqs();
            return Ok(faqs);
        }
    }
}
