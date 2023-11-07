using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Accounts.API.Common.Converters
{
    public class CustomStringConverter : StringConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return base.ConvertFromString(text, row, memberMapData);
        }
    }
}
