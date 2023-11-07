using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Entities
{
    public class Post
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Creator { get; set; }
        public DateTime PublishDate { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Summary { get; set; }
    }
}
