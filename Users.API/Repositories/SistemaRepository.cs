using Users.API.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Users.API.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Users.API.Interfaces.Repositories;
using Dapper.Transaction;
using Dapper.Oracle;
using System.Data;

namespace Users.API.Repositories
{
  public class SistemaRepository : ISistemaRepository
  {
    private readonly IConfiguration _config;

    private readonly string portalConnectionString;
    public SistemaRepository(IConfiguration config)
    {
      _config = config;
      portalConnectionString = _config.GetConnectionString("PortalConnection");
    }

    public async Task<Sistema> Add(Sistema sistema)
    {
      using var conn = new DbSession(portalConnectionString).Connection;
      var parms = new OracleDynamicParameters();
      parms.Add(":Description", sistema.Description, OracleMappingType.Varchar2, ParameterDirection.Input);
      parms.Add(":Id", null, OracleMappingType.Double, ParameterDirection.Output);

      string query = @"INSERT INTO PORTALRH.T_SISTEMA (ID, DESCRICAO) VALUES(S_SISTEMA.nextval, :Description)  returning ID into :Id";
      await conn.ExecuteAsync(query, parms);
      sistema.Id = parms.Get<long>("Id");
      return sistema;

    }

    public async Task Delete(long id)
    {
      using var conn = new DbSession(portalConnectionString).Connection;
      string query = @"DELETE FROM PORTALRH.T_SISTEMA WHERE ID = :Id";
      await conn.ExecuteAsync(query, new { Id = id });
    }

    public async Task<Sistema> Get(long id)
    {
      using (var conn = new DbSession(portalConnectionString).Connection)
      {
        string query = @"SELECT ID AS Id, DESCRICAO AS Description FROM PORTALRH.T_SISTEMA WHERE ID = :Id ORDER BY ID ASC";
        return (await conn.QueryFirstOrDefaultAsync<Sistema>(query, new { Id = id }));
      }
    }

    public async Task<IEnumerable<Sistema>> GetAll()
    {
      using (var conn = new DbSession(portalConnectionString).Connection)
      {
        string query = @"SELECT ID AS Id, DESCRICAO AS Description FROM PORTALRH.T_SISTEMA ORDER BY ID ASC";
        return await conn.QueryAsync<Sistema>(query);
      }
    }

    public async Task<Sistema> Update(Sistema sistema)
    {
      using var conn = new DbSession(portalConnectionString).Connection;

      string query = @"UPDATE PORALRH.T_SISTEMA SET DESCRICAO = :Description WHERE ID = :Id";
      await conn.ExecuteAsync(query, new { Id = sistema.Id, Description = sistema.Description });
      return sistema;

    }
  }
}
