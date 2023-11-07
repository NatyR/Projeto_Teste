using Maoli;
using System;

namespace Portal.API.Dto.Report
{
    public class ReportCartaoDto
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
        public DateTime DataEmissaoCartao { get; set; }
        public decimal LimiteMensal { get; set; }

        private int _NumeroCartao;
        public int NumeroCartao
        {
            get { return int.Parse(_NumeroCartao.ToString().Substring(_NumeroCartao.ToString().Length - 4)); }

            set { _NumeroCartao = value; }
        }
    }
}
