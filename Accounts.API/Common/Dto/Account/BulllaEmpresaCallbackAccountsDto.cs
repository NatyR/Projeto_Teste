namespace Accounts.API.Common.Dto.Account
{
    public class BulllaEmpresaCallbackAccountsDto
    {
        public long idConta { get; set; }
        public int idTipoBloqueio { get; set; }
        public bool sucessoTransacao { get; set; }
        public long idRegistro { get; set; }
        public string statusConta { get; set; }
        public string mensagemProcessamento { get; set; }
    }
}
