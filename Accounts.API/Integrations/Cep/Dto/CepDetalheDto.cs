namespace Accounts.API.Integrations.Cep.Dto
{
    public class CepDetalheDto
    {
        public string tipo { get; set; }
        public string resultado { get; set; }
        public string resultado_txt { get; set; }
        public string uf { get; set; }
        public string cidade { get; set; }
        public string bairro { get; set; }
        public string nome { get; set; }
        public string nome_abreviado { get; set; }
        public string endereco { get; set; }
        public string tipo_logradouro { get; set; }
        public string logradouro { get; set; }
        public string logradouro_abreviado { get; set; }
        public string complemento { get; set; }
        public string ibge { get; set; }
        public string cep { get; set; }
    }
}
