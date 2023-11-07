namespace Portal.API.Dto.Configuration
{
    public class ConfigurationUpdateDto
    {
        public int Id { get; set; }
        public string LinkFacebook { get; set; }
        public string LinkLinkedin { get; set; }
        public string LinkYoutube { get; set; }
        public string LinkInstagram { get; set; }
        public string LinkWebsite { get; set; }
        public string EmailContato { get; set; }
        public string EmailSuporte { get; set; }
        public string TelefoneContato { get; set; }
        public string TelefoneSuporte { get; set; }
        public string TelefoneWhatsapp { get; set; }
        public string HorarioEmpresa { get; set; }

        public string HorarioColaborador { get; set; }
    }
}
