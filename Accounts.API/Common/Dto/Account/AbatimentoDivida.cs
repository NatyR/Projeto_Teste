namespace Accounts.API.Common.Dto.Account
{
    public class AbatimentoDivida
    {
        public string email { get; set; }
        public int idConta { get; set; }
        public string telefone { get; set; }
        public decimal valorAbatimento { get; set; }
        public FileDto file { get; set; }
        public string linkArquivo { get; set; }
    }
}
