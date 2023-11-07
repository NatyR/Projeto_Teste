namespace Portal.API.Entities
{
    public class DashboardIssueCancellations
    {
        public long IDGRUPOCONVENIO { get; set; }
        public long IDLOJA { get; set; }
        public long IDSERVICOSFINANCEIROS { get; set; }
        public int MES { get; set; }
        public int ANO { get; set; }
        public int CONTAS_NOVAS_MES { get; set; }
        public int CONTAS_CANCELADAS_MES { get; set; }

    }

    public class DashboardContasCanceladas
    {
        public long ID_GRUPO_CONVENIO { get; set; }
        public long ID_CONVENIO { get; set; }
        public int ANO { get; set; }
        public int MES { get; set; }
        public int CONTAS_CANCELADAS { get; set; }

    }
    public class DashboardContasNovas
    {
        public long ID_GRUPO_CONVENIO { get; set; }
        public long ID_CONVENIO { get; set; }
        public int ANO { get; set; }
        public int MES { get; set; }
        public int CONTAS_NOVAS { get; set; }

    }
}
