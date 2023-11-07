namespace Users.API.Entities
{
    public class Profile
    {
        public long Id { get; set; }

        public string Description { get; set; }

        public long SistemaId { get; set; }

        public Sistema Sistema { get; set; }

        public string Observation { get; set; }


    }
}