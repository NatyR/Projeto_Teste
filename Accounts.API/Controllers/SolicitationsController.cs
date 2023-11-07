using Accounts.API.Common.Annotations.Validations;
using Accounts.API.Common.Dto;
using Accounts.API.Common.Dto.Account;
using Accounts.API.Common.Dto.Solicitation;
using Accounts.API.Common.Dto.User;
using Accounts.API.Common.Enum;
using Accounts.API.Entities;
using Accounts.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace Cards.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolicitationsController : ControllerBase
    {
        private readonly ISolicitationService _service;

        public SolicitationsController(ISolicitationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Endpoint responsável por listar as solicitacoes de um convenio
        /// </summary>
        [HttpPost("paged/{convenio_id}")]
        [Authorize]
        [ProducesResponseType(typeof(List<SolicitationDto>), 200)]
        public async Task<ActionResult<ResponseDto<SolicitationDto>>> GetAllPagedByConvenio([FromRoute] long convenio_id, [FromBody] SolicitationFilterDto filterOptions, [FromQuery] int limit, [FromQuery] int skip, [FromQuery] string order)
        {
            var response = await _service.GetAllPagedByConvenio(convenio_id,filterOptions, limit, skip, order);
            return Ok(response);
        }

        /// <summary>
        /// Endpoint responsável por listar todas as solicitacoes 
        /// </summary>
        [HttpPost("paged")]
        [Authorize]
        [ProducesResponseType(typeof(List<SolicitationDto>), 200)]
        public async Task<ActionResult<ResponseDto<SolicitationDto>>> GetAllPaged([FromBody] SolicitationFilterDto filterOptions, [FromQuery] int limit, [FromQuery] int skip, [FromQuery] string order)
        {
            var response = await _service.GetAllPaged(filterOptions, limit, skip, order);
            return Ok(response);
        }

        [HttpPost("csv")]
        [Authorize]
        public async Task<IActionResult> DownloadCsv([FromBody] SolicitationFilterDto filterOptions)
        {
            var res = await _service.GetAll(filterOptions);
            if (res == null)
            {
                return NotFound();
            }
            var csv = new StringBuilder();
            var enc = new UTF8Encoding(true);
            var header = new string[] { };
            switch ((SolicitationTypeEnum)filterOptions.SolicitationType)
            {
                case SolicitationTypeEnum.CardReplacement:
                    header = new string[] { "Data da Solicitação", "Usuário", "Tipo de Usuário", "Código do Grupo", "Nome do Grupo", "Código do Convênio", "Nome do Convênio", "CPF", "Nome", "E-mail", "Telefone" };
                    csv.AppendLine(String.Join(';', header));
                    foreach (var row in res)
                    {
                        var line = new string[]
                        {
                            row.RequestedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                            row.UserName,
                            row.UserType,
                            row.GroupId.ToString(),
                            row.GroupName,
                            row.ShopId.ToString(),
                            row.ShopName,
                            row.Cpf,
                            row.Name,
                            row.Email,
                            row.Phone
                        };
                        csv.AppendLine(String.Join(';', line));
                    }
                    break;
                case SolicitationTypeEnum.LimitChange:
                    header = new string[] { "Data da Solicitação", "Usuário", "Tipo de Usuário", "Código do Grupo", "Nome do Grupo", "Código do Convênio", "Nome do Convênio", "CPF", "Nome", "Limite Atual", "Novo Limite" };
                    csv.AppendLine(String.Join(';', header));
                    CultureInfo pt = new CultureInfo("pt-BR");
                    foreach (var row in res)
                    {
                        var line = new string[]
                        {
                            row.RequestedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                            row.UserName,
                            row.UserType,
                            row.GroupId.ToString(),
                            row.GroupName,
                            row.ShopId.ToString(),
                            row.ShopName,
                            row.Cpf,
                            row.Name,
                            row.PreviousLimit.ToString("N2",pt),
                            row.NewLimit.ToString("N2",pt)
                        };
                        csv.AppendLine(String.Join(';', line));
                    }
                    break;
                case SolicitationTypeEnum.CardRequest:
                    header = new string[] { "Data da Solicitação", "Usuário", "Tipo de Usuário", "Código do Grupo", "Nome do Grupo", "Código do Convênio", "Nome do Convênio", "CNPJ do Convênio", "CPF", "Nome", "E-mail", "Telefone", "RG","Data de Nascimento", "Data de Admissão", "CEP", "Endereço", "Número","Complemento","Bairro","Cidade","UF","Código Centro de Custo","Nome Centro de Custo","Código da Filial","Nome da Filial", "Limite Solicitado" };
                    csv.AppendLine(String.Join(';', header));
                    CultureInfo pt2 = new CultureInfo("pt-BR");
                    foreach (var row in res)
                    {
                        var line = new string[]
                        {
                            row.RequestedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                            row.UserName,
                            row.UserType,
                            row.GroupId.ToString(),
                            row.GroupName,
                            row.ShopId.ToString(),
                            row.ShopName,
                            row.ShopDocument,
                            row.Cpf,
                            row.Name,
                            row.Email,
                            row.Phone,
                            row.Rg,
                            row.BirthDate != null ? ((DateTime)row.BirthDate).ToString("dd/MM/yyyy") : "",
                            row.AdmissionDate != null ? ((DateTime)row.AdmissionDate).ToString("dd/MM/yyyy") : "",
                            row.ZipCode,
                            row.Street,
                            row.AddressNumber,
                            row.AddressComplement,
                            row.Neighborhood,
                            row.CityName,
                            row.State,
                            row.CostCenterId.ToString(),
                            row.CostCenterName,
                            row.BranchId.ToString(),
                            row.BranchName,
                            row.NewLimit.ToString("N2",pt2)
                        };
                        csv.AppendLine(String.Join(';', line));
                    }
                    break;
                case SolicitationTypeEnum.AccountUpdate:
                    header = new string[] { "Data da Solicitação", "Usuário", "Tipo de Usuário", "Código do Grupo", "Nome do Grupo", "Código do Convênio", "Nome do Convênio", "CNPJ do Convênio", "CPF", "Nome", "E-mail", "Telefone", "RG", "Data de Nascimento", "Data de Admissão", "CEP", "Endereço", "Número", "Complemento", "Bairro", "Cidade", "UF", "Código Centro de Custo", "Nome Centro de Custo", "Código da Filial", "Nome da Filial"};
                    csv.AppendLine(String.Join(';', header));
                    foreach (var row in res)
                    {
                        var line = new string[]
                        {
                            row.RequestedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                            row.UserName,
                            row.UserType,
                            row.GroupId.ToString(),
                            row.GroupName,
                            row.ShopId.ToString(),
                            row.ShopName,
                            row.ShopDocument,
                            row.Cpf,
                            row.Name,
                            row.Email,
                            row.Phone,
                            row.Rg,
                            row.BirthDate != null ? ((DateTime)row.BirthDate).ToString("dd/MM/yyyy") : "",
                            row.AdmissionDate != null ? ((DateTime)row.AdmissionDate).ToString("dd/MM/yyyy") : "",
                            row.ZipCode,
                            row.Street,
                            row.AddressNumber,
                            row.AddressComplement,
                            row.Neighborhood,
                            row.CityName,
                            row.State,
                            row.CostCenterId.ToString(),
                            row.CostCenterName,
                            row.BranchId.ToString(),
                            row.BranchName
                        };
                        csv.AppendLine(String.Join(';', line));
                    }
                    break;

                case SolicitationTypeEnum.GlobalLimitChange:
                    header = new string[] { "Data da Solicitação", "Usuário", "Tipo de Usuário", "Código do Grupo", "Nome do Grupo", "Código do Convênio", "Nome do Convênio", "CNPJ do Convênio", "Limite Solicitado" };
                    csv.AppendLine(String.Join(';', header));
                    CultureInfo pt3 = new CultureInfo("pt-BR");
                    foreach (var row in res)
                    {
                        var line = new string[]
                        {
                            row.RequestedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                            row.UserName,
                            row.UserType,
                            row.GroupId.ToString(),
                            row.GroupName,
                            row.ShopId.ToString(),
                            row.ShopName,
                            row.ShopDocument,
                            row.NewLimit.ToString("N2",pt3)
                        };
                        csv.AppendLine(String.Join(';', line));
                    }
                    break;
                    case SolicitationTypeEnum.Dismissal:
                    header = new string[] { "Data da Solicitação", "Usuário", "Tipo de Usuário", "Código do Grupo", "Nome do Grupo", "Código do Convênio", "Nome do Convênio", "CNPJ do Convênio", "CPF", "Nome", "E-mail", "Telefone" };
                    csv.AppendLine(String.Join(';', header));
                    foreach (var row in res)
                    {
                        var line = new string[]
                        {
                            row.RequestedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                            row.UserName,
                            row.UserType,
                            row.GroupId.ToString(),
                            row.GroupName,
                            row.ShopId.ToString(),
                            row.ShopName,
                            row.ShopDocument,
                            row.Cpf,
                            row.Name,
                            row.Email,
                            row.Phone
                        };
                        csv.AppendLine(String.Join(';', line));
                    }
                    break;

            }
            byte[] preamble = enc.GetPreamble();
            byte[] byteArray = enc.GetBytes(csv.ToString());
            MemoryStream stream = new MemoryStream(Combine(preamble, byteArray));
            stream.Position = 0;

            return File(stream, "application/octet-stream", "solicitacoes.csv");
        }
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }
        /// <summary>
        /// Endpoint responsável por obter os detalhes da solicitacao
        /// </summary>
        [HttpGet("{convenio_id}/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(SolicitationDto), 200)]
        public async Task<ActionResult<SolicitationDto>> Get(long convenio_id, long id)
        {
            var response = await _service.GetById(id);
            return Ok(response);
        }

        /// <summary>
        /// Endpoint responsável por obter os detalhes da solicitacao
        /// </summary>
        [HttpGet("types")]
        [Authorize]
        [ProducesResponseType(typeof(List<SolicitationTypeDto>), 200)]
        public async Task<ActionResult<List<SolicitationTypeDto>>> GetTypes()
        {
            return Ok(await _service.GetTypes());
        }


    }
}
