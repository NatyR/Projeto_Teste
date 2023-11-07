namespace Accounts.API.Common.Dto.Account
{
    public class AccountOccurrenceDto
    {
        public int LineNumber { get; set; }
        public bool Fatal { get; set; }
        public string Field { get; set; }
        public string Message { get; set; }
    }
}
