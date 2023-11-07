namespace Portal.API.Dto.Faq
{
    public class FaqDto
    {
        public long Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public int Order { get; set; }
        public string Status { get; set; }
    }
}
