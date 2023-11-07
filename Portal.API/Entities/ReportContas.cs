using System;

namespace Portal.API.Entities
{
    public class ReportContas
    {
        public string CodConvenio { get; set; }
        public string CodGrupoConvenio { get; set; }
        public string StatusConvenio { get; set; }
        public int IdConta { get; set; }
        public string Cpf { get; set; }
        public string NomePortador { get; set; }
        public int Matricula { get; set; }
        public string StatusConta { get; set; }
        public string DtCadConta { get; set; }
        public string DtCancelConta { get; set; }
        public decimal LimiteMensal { get; set; }
        public string CodFilial { get; set; }
        public string Filial { get; set; }
        public string CentroCusto { get; set; }
        public DateTime DataInicial { get; set; }
        public DateTime DataFim { get; set; }

    }
}
