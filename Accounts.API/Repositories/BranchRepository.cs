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
    public class BranchRepository : IBranchRepository
    {
        private readonly IConfiguration _config;

        private readonly string ASConnectionString;
        public BranchRepository(IConfiguration config)
        {
            _config = config;
            ASConnectionString = _config.GetConnectionString("ASConnection");
        }

        public async Task<List<Branch>> GetAllByConvenio(long convenioId)
        {
            string query = @"SELECT F.IDFILIALFUNCIONARIO AS Id, 
                                    F.IDLOJA              AS ConvenioId, 
                                    F.CODIGO              AS Code, 
                                    F.DESCRICAO           AS Description, 
                                    F.IDENDERECO          AS AddressId, 
                                    F.UTILIZAENDERECOCONVENIO AS UseConvenioAddress,
                                    E.LOGRADOURO                as StreetAddress ,
                                    E.NUMERO                    as AddressNumber ,
                                    E.COMPLEMENTO               as AddressComplement ,
                                    E.BAIRRO                    as Neighborhood ,
                                    E.LOCALIDADE                as CityName ,
                                    E.UF                        as StateName ,
                                    E.CEP                       as ZipCode
                                FROM NEWUNIK.T_FILIALFUNCIONARIO F
                                LEFT JOIN NEWUNIK.T_ENDERECO E on E.IDENDERECO = F.IDENDERECO
                            WHERE F.IDLOJA = :convenioId
                             ORDER BY DESCRICAO ASC";
            using (var conn = new DbSession(ASConnectionString).Connection)
            {
                return (await conn.QueryAsync<Branch>(query,new{convenioId})).ToList();
            }
        }

        
    }
}
