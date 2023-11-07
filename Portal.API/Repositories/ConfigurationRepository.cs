using Dapper;
using Microsoft.Extensions.Configuration;
using Portal.API.Data;
using Portal.API.Entities;
using Portal.API.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Repositories
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly IConfiguration _config;
        private readonly string portalConnectionString;

        public ConfigurationRepository(IConfiguration config)
        {
            _config = config;
            portalConnectionString = _config.GetConnectionString("PortalConnection");
        }
        public async Task<Configuration> GetConfiguration()
        {
            using var conn = new DbSession(portalConnectionString).Connection;
            var query = @"SELECT ID, LINK_FACEBOOK as LinkFacebook, LINK_LINKEDIN as LinkLinkedin, LINK_YOUTUBE as LinkYoutube, LINK_INSTAGRAM as LinkInstagram, EMAIL_CONTATO as EmailContato, EMAIL_SUPORTE as EmailSuporte, LINK_WEBSITE as LinkWebsite, TELEFONE_CONTATO as TelefoneContato, TELEFONE_SUPORTE as TelefoneSuporte, TELEFONE_WHATSAPP as TelefoneWhatsapp, HORARIO_EMPRESA as HorarioEmpresa, HORARIO_COLABORADOR as HorarioColaborador
                        FROM PORTALRH.T_CONFIGURACAO WHERE ID = 1";
            return await conn.QueryFirstOrDefaultAsync<Configuration>(query);
        }
        
        public async Task<Configuration> Update(Configuration config)
        {
            using var conn = new DbSession(portalConnectionString).Connection;

            string query = @"UPDATE PORTALRH.T_CONFIGURACAO SET 
                                LINK_FACEBOOK=:LinkFacebook, 
                                LINK_LINKEDIN=:LinkLinkedin, 
                                LINK_YOUTUBE=:LinkYoutube, 
                                LINK_INSTAGRAM=:LinkInstagram, 
                                EMAIL_CONTATO=:EmailContato, 
                                EMAIL_SUPORTE=:EmailSuporte, 
                                LINK_WEBSITE=:LinkWebsite, 
                                TELEFONE_CONTATO=:TelefoneContato, 
                                TELEFONE_SUPORTE=:TelefoneSuporte, 
                                TELEFONE_WHATSAPP=:TelefoneWhatsapp,
                                HORARIO_EMPRESA=:HorarioEmpresa,
                                HORARIO_COLABORADOR=:HorarioColaborador
                            WHERE ID=1";
            await conn.ExecuteAsync(query, config);
            return config;
            
        }
    }
}
