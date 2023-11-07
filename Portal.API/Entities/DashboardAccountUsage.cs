namespace Portal.API.Entities
{
    public class DashboardAccountUsage
    {
        public long GRUPO_CONVENIO { get; set; }
        public string NOME_GRUPO_CONVENIO { get; set; }
        public long CODIGO_CONVENIO { get; set; }
        public int ANO { get; set; }
        public int MES { get; set; }
        public decimal VALOR { get; set; }
        public int QUANTIDADE { get; set; }

    }
}
