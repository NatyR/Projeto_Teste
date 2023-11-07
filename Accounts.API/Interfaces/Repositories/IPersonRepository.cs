using Accounts.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Interfaces.Repositories
{
    public interface IPersonRepository
    {
        Task<Person> Create(Person person);
    }
}
