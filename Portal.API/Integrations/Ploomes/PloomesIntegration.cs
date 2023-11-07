using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Portal.API.Common;
using Portal.API.Common.Dto.Request;
using Portal.API.Common.Enum.Request;
using Portal.API.Common.Extensions.Request;
using Portal.API.Common.Helpers.Interfaces;
using Portal.API.Common.Repositories.Interfaces;
using Portal.API.Common.Services;
using Portal.API.Integrations.Interfaces;
using Portal.API.Integrations.Ploomes.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Portal.API.Integrations.Ploomes
{
    public class PloomesIntegration : IntegrationServiceBase, IPloomesIntegration
    {
        private readonly IRequestHelper _requestHelper;
        private readonly EnvironmentsBase environmentsBase;

        public PloomesIntegration(IConfiguration _configuration,
            IRequestHelper requestHelper, 
            IIntegrationRepositoryBase integrationRepositoryBase) : base(integrationRepositoryBase, 
                $"{Assembly.GetEntryAssembly().GetName().Name}_Ploomes")
        {
            environmentsBase = new EnvironmentsBase(_configuration);
            _requestHelper = requestHelper;
        }

        public PloomesOwnerDto getOwnerByConvenio(int grupo)
        {
            //convenio = 605050;
            var url = $"{environmentsBase.PloomesUrlApi}/Contacts";
            var parameters = GetParameters(grupo);
            var method = HttpMethodEnum.Get;
            var headers = GetDefaultHeaders();
            var request = new Func<HttpResponseDto>(() => _requestHelper.Execute(url, method, parameters, headers));
            var response = Execute(request);

            if (!response.StatusCode.IsSuccess())
                return null;

            var res = TryParseResponse(response.Content);

            if (res == null || res.value.Count() == 0)
                return null;
            return res.value.FirstOrDefault().Owner;
        }

        

        private PloomesContactResponseDto TryParseResponse(string response)
        {
            try
            {
                return JsonConvert.DeserializeObject<PloomesContactResponseDto>(response);
            }
            catch(Exception)
            {
                return null;
            }
        }
        private Dictionary<string, string> GetParameters(int grupo)
        {
            return new Dictionary<string, string>
            {
                { "$expand", "Owner" },
                //{ "$filter", $"(OtherProperties/any(o:+o/FieldId+eq+10044968+and+(o/StringValue+eq+'{convenio}')))"}
                { "$filter", $"(OtherProperties/any(o:+o/FieldId+eq+10001733+and+(o/StringValue+eq+'{grupo}')))"}
                
            };
        }
        private Dictionary<string, string> GetDefaultHeaders()
        {
            return new Dictionary<string, string>
            {
                {"user-key", $"{environmentsBase.PloomesUserKey}"},
                {"Content-Type", "application/json"},
                {"Accept", "application/json"},
            };
        }
    }
}
