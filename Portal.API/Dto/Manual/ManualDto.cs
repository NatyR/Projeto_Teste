namespace Portal.API.Dto.Manual
{
    public class ManualDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string ManualType { get; set; }

        public string Url { get; set; }
        public string Description { get; set; }

        public int Order { get; set; }

        public string Status { get; set; }
    }
}
