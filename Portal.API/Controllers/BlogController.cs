using Microsoft.AspNetCore.Mvc;
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
    public class BlogController : Controller
    {
        private readonly IBlogService _blogService;
        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }
        // GET: api/<ConfigurationController>
        [HttpGet]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<List<Post>> GetPosts()
        {
             return await _blogService.GetPosts();
        }
    }
}
