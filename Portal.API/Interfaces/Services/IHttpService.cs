namespace Portal.API.Interfaces.Services
{
    public interface IHttpServicePortal
    {
        public string GetRequestIP(bool tryUseXForwardHeader = true);
        public string GetUserAgent();
        public T GetHeaderValueAs<T>(string headerName);
    }
}
