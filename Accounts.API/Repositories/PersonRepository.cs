using Accounts.API.Entities;
using Accounts.API.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Dapper;
using Microsoft.Extensions.Configuration;
using Accounts.API.Data;
using System.Data;

namespace Accounts.API.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly IConfiguration _config;

        private readonly string BIConnectionString;
        public PersonRepository(IConfiguration config)
        {
            _config = config;
            BIConnectionString = _config.GetConnectionString("BIConnection");
        }

        public async Task<Person> Create(Person person)
        {
            //using var conn = new DbSession(BIConnectionString).Connection;
            //var param = new DynamicParameters(new { Name = person.Name});
            //param.Add(name: "Id", dbType: DbType.UInt64, direction: ParameterDirection.Output);

            //string query = @"INSERT INTO BI_UNIK.VM_T_PESSOA
            //                        (IDPESSOA, NOME)
            //                        VALUES(S_PESSOA.nextval, :Name)  returning IDPESSOA into :Id";
            //await conn.ExecuteAsync(query, param);
            //person.Id = param.Get<long>("Id");
            return person;
        }

        
    }
}
