using Accounts.API.Common.Converters;
using CsvHelper.Configuration;

namespace Accounts.API.Common.Dto.Account
{
    public class CsvMapperAccountDto : ClassMap<AccountCsvDto>
    {

        public CsvMapperAccountDto()
        {
            Map(m => m.RegistrationNumber).Name("Matricula").TypeConverter<CustomStringConverter>();
            Map(m => m.Name).Name("Nome").TypeConverter<CustomStringConverter>();
            Map(m => m.Cpf).Name("CPF").TypeConverter<CustomStringConverter>();
            //Map(m => m.Rg).Name("RG").TypeConverter<CustomStringConverter>();
            //Map(m => m.RgIssuer).Name("Orgao").TypeConverter<CustomStringConverter>();
            Map(m => m.BirthDate).Name("Data Nascimento").TypeConverter<CustomStringConverter>();
            Map(m => m.AdmissionDate).Name("Data Admissao").TypeConverter<CustomStringConverter>();
            Map(m => m.MothersName).Name("Nome Mae").TypeConverter<CustomStringConverter>();
            //Map(m => m.FathersName).Name("Nome Pai").TypeConverter<CustomStringConverter>();
            Map(m => m.PhoneNumber).Name("Celular").TypeConverter<CustomStringConverter>();
            Map(m => m.Email).Name("Email").TypeConverter<CustomStringConverter>();
            Map(m => m.Cnpj).Name("CNPJ").TypeConverter<CustomStringConverter>();
            Map(m => m.BranchCode).Name("Codigo Filial").TypeConverter<CustomStringConverter>();
            Map(m => m.BranchName).Name("Nome Filial").TypeConverter<CustomStringConverter>();
            Map(m => m.CostCenterCode).Name("Codigo Centro de Custo").TypeConverter<CustomStringConverter>();
            Map(m => m.CostCenterName).Name("Nome Centro de Custo").TypeConverter<CustomStringConverter>();
            //Map(m => m.DeliveryUnit).Name("Unidade Entrega").TypeConverter<CustomStringConverter>();
            //Map(m => m.DeliveryToEmployee).Name("Entrega Colaborador").TypeConverter<CustomStringConverter>();
            //Map(m => m.Gender).Name("Sexo").TypeConverter<CustomStringConverter>();
            Map(m => m.Nacionality).Name("Nacionalidade").TypeConverter<CustomStringConverter>();
            Map(m => m.Occupation).Name("Cargo").TypeConverter<CustomStringConverter>();
            Map(m => m.ZipCode).Name("CEP").TypeConverter<CustomStringConverter>();
            Map(m => m.Street).Name("Endereco").TypeConverter<CustomStringConverter>();
            Map(m => m.AddressNumber).Name("Numero").TypeConverter<CustomStringConverter>();
            Map(m => m.AddressComplement).Name("Complemento").TypeConverter<CustomStringConverter>();
            Map(m => m.Neighborhood).Name("Bairro").TypeConverter<CustomStringConverter>();
            Map(m => m.CityName).Name("Cidade").TypeConverter<CustomStringConverter>();
            Map(m => m.State).Name("UF").TypeConverter<CustomStringConverter>();
            //Map(m => m.CardName).Name("Nome Cartao").TypeConverter<CustomStringConverter>();
            Map(m => m.CardLimit).Name("Limite").TypeConverter<CustomStringConverter>();


        }
    }
}
