using Portal.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Services
{
    public interface IBlogService
    {
        Task<List<Post>> GetPosts();
    }
}
