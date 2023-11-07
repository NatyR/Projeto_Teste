using System;

namespace Portal.API.Entities
{
    public class ReportCartao
    {

        public DateTime DataNascimentoPortador { get; set; }
        public string CodigoConvenio { get; set; }
        public string CodGrupoConvenio { get; set; }
        public string Cpf { get; set; }
        public string NomePortador { get; set; }
        public string Matricula { get; set; }
        public string StatusConta { get; set; }
        public int IdConta { get; set; }
        public string StatusCartao { get; set; }
        public int NumeroCartao { get; set; }
        public DateTime DataEmissaoCartao { get; set; }
        public decimal LimiteMensal { get; set; }


    }
}
