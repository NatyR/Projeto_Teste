using Accounts.API.Entities;
using Accounts.API.Interfaces.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Accounts.API.Data;

namespace Accounts.API.Repositories
{
    public class ShopRepository : IShopRepository
    {
        private readonly IConfiguration _config;

        private readonly string ASConnectionString;
        public ShopRepository(IConfiguration config)
        {
            _config = config;
            ASConnectionString = _config.GetConnectionString("ASConnection");
        }

        public async Task<List<Shop>> GetAllByGroup(long group)
        {
            string query = @"SELECT l.IDLOJA                    as Id, 
                                    l.DESCRICAOEXIBIVELCLIENTE  as Name ,
                                    l.LOGRADOURO                as StreetAddress ,
                                    l.NUMERO                    as AddressNumber ,
                                    l.COMPLEMENTO               as AddressComplement ,
                                    l.BAIRRO                    as Neighborhood ,
                                    l.CIDADE                    as CityName ,
                                    l.ESTADO                    as StateName ,
                                    l.CEP                       as ZipCode ,
                                    l.CNPJ                      as Cnpj,
                                    l.DATACADASTRO              as CreatedAt,
                                    l.STATUS                    as Status,
                                    tg.IDGRUPOCONVENIO          as IdGroup,
                                    tg.DESCRICAO                as GroupName, 
                                    tg.VALORLIMITE              as Limit,
                                    tg.VALORLIMITEDISPONIVEL    as AvailableLimit,
                                    ac.DIAPADRAOCORTE           as ClosingDay,
                                    ac.PROXIMOVENCIMENTO        as NextDateExpiration
                            FROM NEWUNIK.T_LOJA l
                                JOIN NEWUNIK.T_GRUPOCONVENIO tg ON l.IDGRUPOCONVENIO = tg.IDGRUPOCONVENIO
                                JOIN NEWUNIK.T_ACORDOCONVENIO ac ON l.IDLOJA = ac.IDLOJA
                            WHERE l.IDGRUPOCONVENIO = :groupId 
                              ORDER BY DESCRICAOEXIBIVELCLIENTE ASC";
            using (var conn = new DbSession(ASConnectionString).Connection)
            {
                return (await conn.QueryAsync<Shop>(query, new { groupId = group })).ToList();
            }
        }


         public async Task<List<Shop>> GetAllByShop(long shop)
        {
            string query = @"SELECT l.IDLOJA                    as Id, 
                                    l.DESCRICAOEXIBIVELCLIENTE  as Name ,
                                    l.LOGRADOURO                as StreetAddress ,
                                    l.NUMERO                    as AddressNumber ,
                                    l.COMPLEMENTO               as AddressComplement ,
                                    l.BAIRRO                    as Neighborhood ,
                                    l.CIDADE                    as CityName ,
                                    l.ESTADO                    as StateName ,
                                    l.CEP                       as ZipCode ,
                                    l.CNPJ                      as Cnpj,
                                    l.DATACADASTRO              as CreatedAt,
                                    l.STATUS                    as Status,
                                    tg.IDGRUPOCONVENIO          as IdGroup,
                                    tg.DESCRICAO                as GroupName, 
                                    tg.VALORLIMITE              as Limit,
                                    tg.VALORLIMITEDISPONIVEL    as AvailableLimit,
                                    ac.DIAPADRAOCORTE           as ClosingDay,
                                    ac.PROXIMOVENCIMENTO        as NextDateExpiration
                            FROM NEWUNIK.T_LOJA l
                                JOIN NEWUNIK.T_GRUPOCONVENIO tg ON l.IDGRUPOCONVENIO = tg.IDGRUPOCONVENIO
                                JOIN NEWUNIK.T_ACORDOCONVENIO ac ON l.IDLOJA = ac.IDLOJA
                            WHERE l.IDLOJA = :shopId 
                              ORDER BY DESCRICAOEXIBIVELCLIENTE ASC";
            using (var conn = new DbSession(ASConnectionString).Connection)
            {
                return (await conn.QueryAsync<Shop>(query, new { ShopId = shop })).ToList();
            }
        }

        public async Task<List<Shop>> GetAllByGroups(long[] groups)
        {
            string query = @"SELECT l.IDLOJA                    as Id, 
                                    l.DESCRICAOEXIBIVELCLIENTE  as Name ,
                                    l.LOGRADOURO                as StreetAddress ,
                                    l.NUMERO                    as AddressNumber ,
                                    l.COMPLEMENTO               as AddressComplement ,
                                    l.BAIRRO                    as Neighborhood ,
                                    l.CIDADE                    as CityName ,
                                    l.ESTADO                    as StateName ,
                                    l.CEP                       as ZipCode ,
                                    l.CNPJ                      as Cnpj,
                                    l.DATACADASTRO              as CreatedAt,
                                    l.STATUS                    as Status,
                                    tg.IDGRUPOCONVENIO          as IdGroup,
                                    tg.DESCRICAO                as GroupName, 
                                    tg.VALORLIMITE              as Limit,
                                    tg.VALORLIMITEDISPONIVEL    as AvailableLimit,
                                    ac.DIAPADRAOCORTE           as ClosingDay,
                                    ac.PROXIMOVENCIMENTO        as NextDateExpiration
                            FROM NEWUNIK.T_LOJA l
                                JOIN NEWUNIK.T_GRUPOCONVENIO tg ON l.IDGRUPOCONVENIO = tg.IDGRUPOCONVENIO
                                JOIN NEWUNIK.T_ACORDOCONVENIO ac ON l.IDLOJA = ac.IDLOJA
                            WHERE l.IDGRUPOCONVENIO = ANY :groupId AND l.STATUS = 'A'
                              ORDER BY DESCRICAOEXIBIVELCLIENTE ASC";
            using (var conn = new DbSession(ASConnectionString).Connection)
            {
                return (await conn.QueryAsync<Shop>(query, new { groupId = groups })).ToList();
            }
        }
        public async Task<List<GroupLimit>> GetLimitsByGroup(long group)
        {
            string query = @"SELECT tol.DATAOCORRENCIALIMITE as EventDate,
                                    tol.LIMITEANTERIOR as PreviousLimit,
                                    tol.VALOROCORRENCIA as ChangeValue,
                                    tol.NOVOLIMITE as NewLimit
                            FROM T_OCORRENCIA_LIMITE tol 
                            WHERE IDGRUPOCONVENIO = :groupId AND IDTIPOTRANSACAO ='NC' 
                              ORDER BY DATAOCORRENCIALIMITE DESC";
            using (var conn = new DbSession(ASConnectionString).Connection)
            {
                return (await conn.QueryAsync<GroupLimit>(query, new { groupId = group })).ToList();
            }
        }

        public async Task<Shop> GetByCnpj(string cnpj)
        {
            string query = @"SELECT l.IDLOJA                    as Id, 
                                    l.DESCRICAOEXIBIVELCLIENTE  as Name ,
                                    l.LOGRADOURO                as StreetAddress ,
                                    l.NUMERO                    as AddressNumber ,
                                    l.COMPLEMENTO               as AddressComplement ,
                                    l.BAIRRO                    as Neighborhood ,
                                    l.CIDADE                    as CityName ,
                                    l.ESTADO                    as StateName ,
                                    l.CEP                       as ZipCode ,
                                    l.CNPJ                      as Cnpj,
                                    l.DATACADASTRO              as CreatedAt,
                                    l.STATUS                    as Status,
                                    tg.IDGRUPOCONVENIO          as IdGroup,
                                    tg.DESCRICAO                as GroupName,  
                                    tg.VALORLIMITE              as Limit,
                                    tg.VALORLIMITEDISPONIVEL    as AvailableLimit,
                                    ac.DIAPADRAOCORTE           as ClosingDay
                            FROM NEWUNIK.T_LOJA l
                                JOIN NEWUNIK.T_GRUPOCONVENIO tg ON l.IDGRUPOCONVENIO = tg.IDGRUPOCONVENIO
                                JOIN NEWUNIK.T_ACORDOCONVENIO ac ON l.IDLOJA = ac.IDLOJA
                            WHERE l.CNPJ = :Cnpj  AND l.STATUS = 'A' ";
            using (var conn = new DbSession(ASConnectionString).Connection)
            {
                return (await conn.QueryFirstOrDefaultAsync<Shop>(query, new { Cnpj = cnpj }));
            }
        }

        public async Task<Shop> GetById(long id)
        {
            string query = @"SELECT l.IDLOJA                    as Id, 
                                    l.DESCRICAOEXIBIVELCLIENTE  as Name ,
                                    l.LOGRADOURO                as StreetAddress ,
                                    l.NUMERO                    as AddressNumber ,
                                    l.COMPLEMENTO               as AddressComplement ,
                                    l.BAIRRO                    as Neighborhood ,
                                    l.CIDADE                    as CityName ,
                                    l.ESTADO                    as StateName ,
                                    l.CEP                       as ZipCode ,
                                    l.CNPJ                      as Cnpj,
                                    l.DATACADASTRO              as CreatedAt,
                                    l.STATUS                    as Status,
                                    tg.IDGRUPOCONVENIO          as IdGroup,
                                    tg.DESCRICAO                as GroupName,  
                                    tg.VALORLIMITE              as Limit,
                                    tg.VALORLIMITEDISPONIVEL    as AvailableLimit,
                                    ac.DIAPADRAOCORTE           as ClosingDay
                            FROM NEWUNIK.T_LOJA l
                                JOIN NEWUNIK.T_GRUPOCONVENIO tg ON l.IDGRUPOCONVENIO = tg.IDGRUPOCONVENIO
                                JOIN NEWUNIK.T_ACORDOCONVENIO ac ON l.IDLOJA = ac.IDLOJA
                            WHERE  l.IDLOJA = :IdLoja  AND l.STATUS = 'A' ";
            using (var conn = new DbSession(ASConnectionString).Connection)
            {
                return (await conn.QueryFirstOrDefaultAsync<Shop>(query, new { IdLoja = id}));
            }
        }
    }
}
