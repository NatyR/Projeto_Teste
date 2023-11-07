using Portal.API.Common.Entities.Integrations;


namespace Portal.API.Common.Repositories.Interfaces
{
    public interface IIntegrationRepositoryBase
    {
        void Add(IntegrationLog log);
    }
}
