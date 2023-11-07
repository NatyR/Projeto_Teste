namespace Accounts.API.Interfaces.Services
{
    public interface IHttpServiceAccounts
    {
        public string GetRequestIP(bool tryUseXForwardHeader = true);
        public string GetUserAgent();
        public T GetHeaderValueAs<T>(string headerName);
    }
}
