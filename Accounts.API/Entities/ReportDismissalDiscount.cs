using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Linq;

namespace Accounts.API.Entities
{
    public class ReportDismissalDiscount
    {
        [JsonPropertyName("codigoRetorno")]
        public int CodigoRetorno { get; set; }

        [JsonPropertyName("mensagemRetorno")]
        public string MensagemRetorno { get; set; }

        [JsonPropertyName("totalPaginas")]
        public int TotalPaginas { get; set; }

        [JsonPropertyName("lista")]
        public List<object> Lista { get; set; }

    }

    public class ReportDismissalDiscountResponse
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("cpf")]
        public string Cpf { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("telefone")]
        public string Telefone { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("codigoGrupo")]
        public long CodigoGrupo { get; set; }

        [JsonPropertyName("nomeGrupo")]
        public string NomeGrupo { get; set; }

        [JsonPropertyName("codigoConvenio")]
        public long CodigoConvenio { get; set; }

        [JsonPropertyName("nomeConvenio")]
        public string NomeConvenio { get; set; }

        [JsonPropertyName("qtTransacoes")]
        public int QntTransacoes { get; set; }

        [JsonPropertyName("dataCancelamento")]
        public DateTime? DataCancelamento { get; set; }

        [JsonPropertyName("dividaTotal")]
        public double? DividaTotal { get; set; }

        [JsonPropertyName("descontoRescisao")]
        public double? DescontoRescisao { get; set; }

        [JsonPropertyName("dividaBoletada")]
        public double? DividaBoletada { get; set; }

        [JsonPropertyName("qtBoletos")]
        public int QntBoletos { get; set; }

        [JsonPropertyName("primeiroVencimento")]
        public DateTime? PrimeiroVencimento { get; set; }

        [JsonPropertyName("valorPrimeiroBoleto")]
        public double? ValorPrimeiroBoleto { get; set; }

        [JsonPropertyName("ultimoVencimento")]
        public DateTime? UltimoVencimento { get; set; }

        [JsonPropertyName("valorUltimoBoleto")]
        public double? ValorUltimoBoleto { get; set; }

        [JsonPropertyName("trct")]
        public string TrctArquivo { get; set; }

    }

    public class LimitedReportDismissalDiscountResponse
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("cpf")]
        public string Cpf { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("telefone")]
        public string Telefone { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("codigoConvenio")]
        public long CodigoConvenio { get; set; }

        [JsonPropertyName("nomeConvenio")]
        public string NomeConvenio { get; set; }

        [JsonPropertyName("qtTransacoes")]
        public int QntTransacoes { get; set; }

        [JsonPropertyName("dataCancelamento")]
        public DateTime? DataCancelamento { get; set; }

        [JsonPropertyName("dividaTotal")]
        public double? DividaTotal { get; set; }

        [JsonPropertyName("descontoRescisao")]
        public double? DescontoRescisao { get; set; }

        [JsonPropertyName("dividaBoletada")]
        public double? DividaBoletada { get; set; }

        [JsonPropertyName("qtBoletos")]
        public int QntBoletos { get; set; }

        [JsonPropertyName("primeiroVencimento")]
        public DateTime? PrimeiroVencimento { get; set; }

        [JsonPropertyName("valorPrimeiroBoleto")]
        public double? ValorPrimeiroBoleto { get; set; }

        [JsonPropertyName("ultimoVencimento")]
        public DateTime? UltimoVencimento { get; set; }

        [JsonPropertyName("valorUltimoBoleto")]
        public double? ValorUltimoBoleto { get; set; }

        [JsonPropertyName("trct")]
        public string TrctArquivo { get; set; }

    }
}
