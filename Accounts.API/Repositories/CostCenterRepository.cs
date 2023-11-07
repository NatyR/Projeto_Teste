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
using Dapper.Oracle;

namespace Accounts.API.Repositories
{
    public class CostCenterRepository : ICostCenterRepository
    {
        private readonly IConfiguration _config;

        private readonly string ASConnectionString;
        public CostCenterRepository(IConfiguration config)
        {
            _config = config;
            ASConnectionString = _config.GetConnectionString("ASConnection");
        }

        public async Task<List<CostCenter>> GetAllByConvenio(long convenioId)
        {
            string query = @"SELECT IDCENTROCUSTO               AS Id, 
                                    IDLOJA                      AS ConvenioId,  
                                    DESCRICAO                   AS Description, 
                                    STATUS                      AS Status, 
                                    IDENDERECO                  AS AddressId, 
                                    CODIGOINTERNO               AS InternalCode, 
                                    IDFILIALFUNCIONARIO         AS BranchId, 
                                    UTILIZAENDERECOCONVENIO     AS UseConvenioAddress, 
                                    UTILIZAENDERECOFILIAL       AS UseBranchAddress
                            FROM NEWUNIK.T_CENTROCUSTO
                            WHERE IDLOJA = :convenioId
                             ORDER BY DESCRICAO ASC";
            using (var conn = new DbSession(ASConnectionString).Connection)
            {
                return (await conn.QueryAsync<CostCenter>(query, new { convenioId })).ToList();
            }
        }

    }
}
