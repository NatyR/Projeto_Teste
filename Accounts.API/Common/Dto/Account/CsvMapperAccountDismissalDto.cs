using Accounts.API.Common.Converters;
using CsvHelper.Configuration;

namespace Accounts.API.Common.Dto.Account
{
    public class CsvMapperAccountDismissalDto : ClassMap<AccountDismissalCsvDto>
    {

        public CsvMapperAccountDismissalDto()
        {
         
            Map(m => m.Name).Name("Nome").TypeConverter<CustomStringConverter>();
            Map(m => m.Cpf).Name("CPF").TypeConverter<CustomStringConverter>();
            Map(m => m.ReasonDescription).Name("Tipo de Desligamento").TypeConverter<CustomStringConverter>();


        }
    }
}
