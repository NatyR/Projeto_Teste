using Accounts.API.Common.Annotations.Validations;
using Accounts.API.Common.Dto;
using Accounts.API.Common.Dto.Account;
using Accounts.API.Common.Dto.User;
using Accounts.API.Common.Middlewares.Exceptions;
using Accounts.API.Entities;
using Accounts.API.Integrations.As;
using Accounts.API.Integrations.AwsS3;
using Accounts.API.Interfaces.Repositories;
using Accounts.API.Interfaces.Services;
using Accounts.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;


namespace Cards.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _service;

        private readonly ISolicitationService _solicitationService;
        private readonly IHttpServiceAccounts _httpService;
        private readonly IAccessRepositoryAccounts _acessoRepository;

        public AccountsController(IAccountService service,
            ISolicitationService solicitationService,
            IHttpServiceAccounts httpService,
            IAccessRepositoryAccounts acessoRepository)
        {
            _service = service;
            _solicitationService = solicitationService;
            _httpService = httpService;
            _acessoRepository = acessoRepository;
        }

        /// <summary>
        /// Endpoint responsável por listar os colaboradores de um convenio
        /// </summary>
        [HttpPost("blocked-paged")]
        [Authorize]
        [ProducesResponseType(typeof(List<BlockedAccountDto>), 200)]
        public async Task<ActionResult<ResponseDto<BlockedAccountDto>>> GetBlockedPaged([FromQuery] int limit, [FromQuery] int skip, [FromQuery] string order, [FromBody] BlockedAccountFilterDto filter)
        {


            var response = await _service.GetBlockedPaged(limit, skip, order, filter);
            return Ok(response);
        }

        [HttpPost("blocked-csv")]
        [Authorize]
        public async Task<IActionResult> GetBlocked(BlockedAccountFilterDto filter)
        {
            var res = await _service.GetBlocked(filter);
            if (res == null)
            {
                return NotFound();
            }
            var csv = new StringBuilder();
            var enc = new UTF8Encoding(true);
            var header = @"Código do Grupo;Nome do Grupo;Código do Convênio;Nome do Convênio;Código da Filial;Nome da Filial;Código do Centro de Custo;Nome do Centro de Custo;CPF do Colaborador;Matrícula;Data de Cadastramento;Data de Bloqueio;Limite do Cartão;Número do Cartão";
            csv.AppendLine(header);
            CultureInfo pt = new CultureInfo("pt-BR");
            foreach (var row in res)
            {
                var line = new string[]
                {
                            row.GroupId.ToString(),
                            row.GroupName,
                            row.ShopId.ToString(),
                            row.ShopName,
                            row.BranchId.ToString(),
                            row.BranchName,
                            row.CostCenterId.ToString(),
                            row.CostCenterName,
                            row.Cpf,
                            row.RegistrationNumber,
                            row.CreatedAt != null ? ((DateTime)row.CreatedAt).ToString("dd/MM/yyyy HH:mm:ss") : "",
                            row.BlockedAt != null ? ((DateTime)row.BlockedAt).ToString("dd/MM/yyyy HH:mm:ss") : "",
                            row.CardLimit.ToString("N2",pt),
                            row.CardNumber.Substring(12, 4)
                };
                csv.AppendLine(String.Join(';', line));
            }
            byte[] preamble = enc.GetPreamble();
            byte[] byteArray = enc.GetBytes(csv.ToString());

            MemoryStream stream = new MemoryStream(Combine(preamble, byteArray));
            stream.Position = 0;

            return File(stream, "application/octet-stream", "blocked-accounts.csv");
        }

        /// <summary>
        /// Endpoint responsável por listar os colaboradores de um convenio
        /// </summary>
        [HttpGet("paged/{convenio_id}")]
        [Authorize]
        [ProducesResponseType(typeof(List<AccountDto>), 200)]
        public async Task<ActionResult<ResponseDto<AccountDto>>> GetAllPaged([FromRoute] long convenio_id, [FromQuery] int limit, [FromQuery] int skip, [FromQuery] string search, [FromQuery] string order, [FromQuery] string? accountStatus, [FromQuery] string? cardStatus, [FromQuery] string? cardsFilter)
        {
            var response = await _service.GetAllPaged(convenio_id, limit, skip, search, order, accountStatus, cardStatus, cardsFilter);
            return Ok(response);
        }

        [HttpGet("csv/{convenio_id}")]
        [Authorize]
        public async Task<IActionResult> GetAll(long convenio_id)
        {
            var res = await _service.GetAllByConvenio(convenio_id);
            if (res == null)
            {
                return NotFound();
            }
            var csv = new StringBuilder();
            var enc = new UTF8Encoding(true);
            var header = @"Nome;CPF;Número do Cartão;Limite do cartão;Última atualização";
            csv.AppendLine(header);
            foreach (var account in res)
            {
                var line = $"{account.Name};{account.Cpf};****{account.CardNumber.Substring(4, 8)}****;{account.CardLimit};{account.CardLimitUpdatedAt}";
                csv.AppendLine(line);
            }
            byte[] preamble = enc.GetPreamble();
            byte[] byteArray = enc.GetBytes(csv.ToString());

            MemoryStream stream = new MemoryStream(Combine(preamble, byteArray));
            stream.Position = 0;

            return File(stream, "application/octet-stream", "limites.csv");
        }
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }
        [HttpGet("{convenio_id}/{id}")]
        [Authorize]
        public async Task<IActionResult> Get(long id, long convenio_id)
        {
            var res = await _service.Get(id, convenio_id);
            if (res == null)
            {
                return NotFound();
            }
            res.TratarCampos();
            return Ok(res);
        }

        [HttpGet("validate-registration-number/{convenio_id}/{registrationNumber}")]
        [Authorize]
        public async Task<IActionResult> GetByRegistrationNumber(long convenio_id, string registrationNumber)
        {
            var res = await _service.GetByRegistrationNumber(registrationNumber, convenio_id);
            if (res == null)
            {
                return Ok();
            }
            else
            {
                return Ok(new { id = res.Id });
            }
        }

        [HttpPost("{convenio}")]
        [ValidForm]
        [Authorize]
        [ProducesResponseType(typeof(AccountDto), 200)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Create(AccountAddDto account, [FromRoute] int convenio)
        {
            try
            {
                account.Convenio = convenio;
                account.CurrentUser = this.GetCurrentUser();
                await _service.Create(account);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{convenio}/{id}")]
        [ValidForm]
        [Authorize]
        [ProducesResponseType(typeof(AccountDto), 200)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Update(AccountAddDto account, [FromRoute] int convenio)
        {
            account.Convenio = convenio;
            account.CurrentUser = this.GetCurrentUser();
            await _service.Update(account);
            return Ok();
        }

        [HttpPost("{convenio}/block")]
        [ValidForm]
        [Authorize]
        [ProducesResponseType(typeof(AccountDto), 200)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Block([FromForm] AccountBlockDto data, [FromRoute] long convenio)
        {
            data.Convenio = convenio;
            data.CurrentUser = this.GetCurrentUser();
            await _service.Block(data);
            return Ok();
        }
        [HttpPost("{convenio}/unblock")]
        [ValidForm]
        [Authorize]
        [ProducesResponseType(typeof(AccountDto), 200)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Unblock([FromForm] AccountUnblockDto data, [FromRoute] int convenio)
        {
            data.Convenio = convenio;
            data.CurrentUser = this.GetCurrentUser();
            await _service.Unblock(data);
            return Ok();
        }
        [HttpPost("{convenio}/reissue")]
        [ValidForm]
        [Authorize]
        [ProducesResponseType(typeof(AccountDto), 200)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Reissue(AccountReissueDto accounts, [FromRoute] int convenio)
        {
            accounts.Convenio = convenio;
            accounts.CurrentUser = this.GetCurrentUser();
            await _service.Reissue(accounts);
            return Ok();
        }
        [HttpPost("{convenio}/change-limit")]
        [ValidForm]
        [Authorize]
        [ProducesResponseType(typeof(AccountLimitDto), 200)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ChangeLimit(AccountLimitDto accounts, [FromRoute] long convenio)
        {
            accounts.Convenio = convenio;
            accounts.CurrentUser = this.GetCurrentUser();
            await _service.ChangeLimit(accounts);
            return Ok();
        }

        [HttpPost("global-limit/{convenio}")]
        [ValidForm]
        [Authorize]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GlobalLimitFileUploadForm([FromForm] GlobalLimitFileUploadDto model, [FromRoute] int convenio)
        {
            try
            {
                model.CurrentUser = this.GetCurrentUser();
                var anexos = await _service.GlobalLimit(model);

                try
                {
                    Int64.TryParse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value, out long userId);
                    if (userId > 0)
                    {
                        string body = "";
                        foreach (var anexo in anexos)
                        {
                            var link = AwsS3Integration.SignedUrl(anexo.key);
                            body += link + ";";
                        }

                        string ip = _httpService.GetRequestIP();
                        var url = "/api/accounts/globallimit/" + convenio;
                        var method = HttpContext.Request.Method;
                        var postData = body;
                        Acesso acesso = new Acesso(userId, ip, url, method, postData);
                        Task.Run(async () =>
                        {
                            await _acessoRepository.Add(acesso);
                        }).Wait();
                    }
                }
                catch (Exception)
                {

                }

                return Ok("Arquivo enviado com sucesso");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




        [HttpPost("upload/{convenio}")]
        [ValidForm]
        [Authorize]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> FileUploadForm([FromForm] FileUploadDto FileUpload, [FromRoute] int convenio)
        {
            try
            {
                var currentUser = GetCurrentUser();
                await _service.UploadFileNewCard(FileUpload, convenio, currentUser);
                await _service.RegisterImport(FileUpload, convenio, currentUser);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost("upload-validate/{convenio}")]
        [ValidForm]
        [Authorize]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> FileUploadFormValidate([FromForm] FileUploadDto FileUpload, [FromRoute] long convenio)
        {
            try
            {

                var validation = await _service.ValidateFile(FileUpload, convenio);
                return Ok(validation);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }
        private UserDtoAccounts GetCurrentUser()
        {
            return new UserDtoAccounts()
            {
                Id = Int32.Parse(User.Claims.First(c => c.Type == "id").Value),
                Email = User.Claims.First(c => c.Type == ClaimTypes.Email).Value,
                Name = User.Claims.First(c => c.Type == ClaimTypes.Name).Value,
                UserType = User.Claims.First(c => c.Type == "userType").Value,
            };
        }
        #region Desligamento
        [HttpPost("{convenio}/dismissal")]
        [ValidForm]
        [Authorize]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public ActionResult Dismissal([FromForm] AccountDismissalDto data, [FromRoute] int convenio)
        {
            try
            {
                data.Convenio = convenio;
                data.CurrentUser = this.GetCurrentUser();
                data.UserId = Int32.Parse(User.Claims.First(c => c.Type == "id").Value);
                var retorno = _service.Dismissal(data);
                
                if(retorno.Exception is null)
                    return Ok();
                else{
                    throw new BadRequestException(retorno.Exception.Message);
                }
            }
            
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }
        [HttpPost("upload-dismissal/{convenio}")]
        [ValidForm]
        [Authorize]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> FileUploadDismissalForm([FromForm] FileUploadDto FileUpload, [FromRoute] int convenio)
        {
            try
            {
                var user = this.GetCurrentUser();
                await _service.ImportFileDismissal(FileUpload, convenio, user);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }
        [HttpPost("upload-dismissal-validate/{convenio}")]
        [ValidForm]
        [Authorize]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> FileUploadDismissalFormValidate([FromForm] FileUploadDto FileUpload, [FromRoute] long convenio)
        {
            try
            {

                var validation = await _service.ValidateFileDismissal(FileUpload, convenio);
                return Ok(validation);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }

        [HttpPost("callback")]
        [ValidForm]
        [ProducesResponseType(typeof(AccountDto), 200)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Callback(BulllaEmpresaCallbackDto callback)
        {
            try
            {
                await _solicitationService.RegisterCallback(callback);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion



        [HttpPost("{convenio}/desbloqueio-seguranca/")]
        [ValidForm]
        [Authorize]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> DesbloqueioDeSeguranca([FromRoute] long convenio, [FromBody] AccountModel data)
        {
            var userId = Int32.Parse(User.Claims.First(c => c.Type == "id").Value);

            try
            {
                var retorno = await _service.SecurityUnblock(convenio, data, userId);

                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{convenio}/contas-desligadas")]
        [Authorize]
        [ProducesResponseType(typeof(JObject), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ContasDesligadas([FromRoute] long convenio, [FromQuery] string cpfColaborador, [FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim, [FromQuery] int? pagina, [FromQuery] int? tamanhoPagina, [FromQuery] int? statusRecisao)
        {
            var userId = Int32.Parse(User.Claims.First(c => c.Type == "id").Value);

            try
            {
                var retorno = await _service.ContasDesligadas(convenio, cpfColaborador, dataInicio, dataFim, pagina, tamanhoPagina, userId, statusRecisao);

                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{convenio}/faturas")]
        [ValidForm]
        [Authorize]
        [ProducesResponseType(typeof(JObject), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Faturas([FromRoute] long convenio, [FromQuery] int idConta)
        {
            var userId = Int32.Parse(User.Claims.First(c => c.Type == "id").Value);

            try
            {
                var retorno = await _service.Faturas(convenio, idConta, userId);

                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{convenio}/contas")]
        [Authorize]
        [ProducesResponseType(typeof(JObject), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Contas([FromRoute] long convenio, [FromQuery] string cpf, [FromQuery] string email, [FromQuery] string nome, [FromQuery] int? pagina, [FromQuery] int? tamanhoPagina, [FromQuery] string ascendente, [FromQuery] string ordem, [FromQuery] string status)
        {
            var userId = Int32.Parse(User.Claims.First(c => c.Type == "id").Value);

            try
            {
                var retorno = await _service.Contas(convenio, cpf, email, nome, pagina, tamanhoPagina, ascendente, ordem, status, userId);

                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{convenio}/abatimento-divida")]
        [ValidForm]
        [Authorize]
        [ProducesResponseType(typeof(JObject), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AbatimentoDivida([FromRoute] long convenio, [FromForm] AbatimentoDivida data)
        {
            var userId = Int32.Parse(User.Claims.First(c => c.Type == "id").Value);

            try
            {
                var retorno = await _service.AbatimentoDivida(convenio, data, userId);

                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{convenio}/faturas-residual")]
        [ValidForm]
        [Authorize]
        [ProducesResponseType(typeof(JObject), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> FaturasResidual([FromRoute] long convenio, [FromQuery] int idConta)
        {
            var userId = Int32.Parse(User.Claims.First(c => c.Type == "id").Value);

            try
            {
                var retorno = await _service.FaturasResidual(convenio, idConta, userId);

                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{convenio}/cargas")]
        [ValidForm]
        [Authorize]
        [ProducesResponseType(typeof(JObject), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Cargas([FromRoute] long convenio, [FromQuery] int idConta)
        {
            var userId = Int32.Parse(User.Claims.First(c => c.Type == "id").Value);

            try
            {
                var retorno = await _service.Cargas(convenio, idConta, userId);

                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{convenio}/cartoes")]
        [Authorize]
        [ProducesResponseType(typeof(JObject), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Cartoes([FromRoute] long convenio, [FromQuery] string cpf, [FromQuery] string email, [FromQuery] string nome, [FromQuery] int? pagina, [FromQuery] int? tamanhoPagina, [FromQuery] string ascendente, [FromQuery] string ordem, [FromQuery] string status)
        {
            var userId = Int32.Parse(User.Claims.First(c => c.Type == "id").Value);

            try
            {
                var retorno = await _service.Cartoes(convenio, cpf, email, nome, pagina, tamanhoPagina, ascendente, ordem, status, userId);

                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// Endpoint responsável por listar dados de rescisão de colaboradores
        /// </summary>
        [HttpGet("report-dismissal-discount")]
        [Authorize]
        [ProducesResponseType(typeof(JObject), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseAccounts), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ReportDismissalDiscount>>> ReportDismissalDiscount([FromQuery] long? shopId, [FromQuery] long? groupId,[FromQuery] string search, [FromQuery] DateTime? dtStart, [FromQuery] DateTime? dtEnd, [FromQuery] int? page, [FromQuery] int? pageSize)
        {

            try
            {
                var userId = Int32.Parse(User.Claims.First(c => c.Type == "id").Value);
                var userType = User.Claims.First(c => c.Type =="userType").Value;
                var userMail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
                long defaultValue = 0;
                long defaultShop = shopId ?? defaultValue;
                long defaultGroup = groupId ?? defaultValue;

                var retorno = await _service.ReportDismissalDiscount(shopId, groupId, dtStart, dtEnd, page, pageSize, userId, userMail, userType, search);

                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("report-dismissal/csv")]
        [Authorize]
        public async Task<IActionResult> ReportDismissalCSV([FromQuery] long? shopId, [FromQuery] long? groupId,[FromQuery] string search, [FromQuery] DateTime? dtStart, [FromQuery] DateTime? dtEnd)
        {
            try
            {
                var userId = Int32.Parse(User.Claims.First(c => c.Type == "id").Value);
                var userType = User.Claims.First(c => c.Type =="userType").Value;
                var userMail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
                long defaultValue = 0;
                long defaultShop = shopId ?? defaultValue;
                long defaultGroup = groupId ?? defaultValue;

                var retorno = await _service.ReportDismissalCSV(shopId, groupId, dtStart, dtEnd, userId, userMail, userType, search);
                return File(retorno, "application/octet-stream", "rel_rescisao_colaborador_"+ DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
     
        }

    }
}






