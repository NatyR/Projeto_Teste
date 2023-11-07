using Accounts.API.Integrations.Cep.Dto;
using System.Net.Http;
using System.Threading.Tasks;

namespace Accounts.API.Integrations.Cep
{
    public class CepIntegration
    {

        public static async Task<CepResultDto> ConsultaCep(string cep)
        {
            //Http request GET passing zipcode in url
            var client = new HttpClient();
            var response = await client.GetAsync("https://p2p.bullla.com.br/correios/correios/busca_cep/" + cep + ".json");
            return await response.Content.ReadAsAsync<CepResultDto>();
            
        }

    }
}
