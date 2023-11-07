namespace Portal.API.Entities
{
    public class ProfilePortal
    {
        public long Id { get; set; }

        public string Description { get; set; }

        public long SistemaId { get; set; }

        public Sistema Sistema { get; set; }

        public string Observation { get; set; }
    }
}
