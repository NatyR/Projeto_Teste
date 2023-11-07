namespace Portal.API.Dto.DashboardNovo
{
    public class CartoesAtivos
    {
        public CartoesAtivos()
        {
            CartoesComTransacoes = "0";
            CartoesSemTransacoes = "0";
        }
        public string CartoesComTransacoes { get; set; }
        public string CartoesSemTransacoes { get; set; }
    }
}
