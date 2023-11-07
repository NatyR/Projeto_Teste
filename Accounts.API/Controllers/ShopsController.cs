using Accounts.API.Common.Dto.Branch;
using Accounts.API.Common.Dto.Shop;
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
    public class ShopsController : Controller
    {
        private readonly IShopService _service;

        public ShopsController(IShopService service)
        {
            _service = service;
        }

        [HttpGet("{group}")]
        [Authorize]
        [ProducesResponseType(typeof(List<ShopDto>), 200)]
        public async Task<ActionResult<List<ShopDto>>> GetAllByGroup([FromRoute] int group)
        {
            var response = (await _service.GetAllByGroup(group)).OrderBy(s => s.Id);
            return Ok(response);
        }

        [HttpGet("idShop/{shop}")]
        [Authorize]
        [ProducesResponseType(typeof(List<ShopDto>), 200)]
        public async Task<ActionResult<List<ShopDto>>> GetAllByShop([FromRoute] int shop)
        {
            var response = (await _service.GetAllByShop(shop)).OrderBy(s => s.Id);
            return Ok(response);
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(List<ShopDto>), 200)]
        public async Task<ActionResult<List<ShopDto>>> GetAllByGroups([FromBody] long[] groups)
        {
            var response = await _service.GetAllByGroups(groups);
            return Ok(response);
        }
        [HttpGet("{group}/limits")]
        [Authorize]
        [ProducesResponseType(typeof(List<ShopDto>), 200)]
        public async Task<ActionResult<List<ShopDto>>> GetLimitsByGroup([FromRoute] int group)
        {
            var response = await _service.GetLimitsByGroup(group);
            return Ok(response);
        }

    }
}
