namespace Portal.API.Dto.DashboardNovo
{
    public class BigCards
    {
        public BigCards()
        {
            TotalContas = "0";
            NovasContas = "0";
            CartoesParaAtivacao = "0";
            CartoesBloqueados = "0";
            ContasCanceladas = "0";
        }
        public string TotalContas          { get; set; }
        public string NovasContas          { get; set; }
        public string CartoesParaAtivacao  { get; set; }
        public string CartoesBloqueados    { get; set; }
        public string ContasCanceladas     { get; set; }
    }
}
