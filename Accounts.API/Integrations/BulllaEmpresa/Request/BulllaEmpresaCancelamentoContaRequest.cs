namespace Accounts.API.Integrations.BulllaEmpresa.Request
{
    public class BulllaEmpresaCancelamentoContaRequest
    {
        public long idConta { get; set; }
        public long idRegistro { get; set; }
        public string idTipoBloqueio { get; set; }
    }
}
