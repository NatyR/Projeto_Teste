using Accounts.API.Common.Entities;

namespace Accounts.API.Common.Repositories.Interfaces
{
    public interface IIntegrationRepositoryBase
    {
        void Add(IntegrationLog log);
    }
}
