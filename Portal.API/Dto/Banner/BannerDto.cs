using System;

namespace Portal.API.Dto.Banner
{
    public class BannerDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Action { get; set; }
        public string Link { get; set; }
        public string Image { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public string Status { get; set; }
        public int Order { get; set; }

        public string ImageSrc { get; set; }
    }
}
