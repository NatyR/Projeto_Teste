using Accounts.API.Common.Dto;
using Accounts.API.Common.Dto.Account;
using Accounts.API.Common.Dto.As;
using Accounts.API.Common.Dto.Rabbit;
using Accounts.API.Common.Dto.User;
using Accounts.API.Common.Enum;
using Accounts.API.Entities;
using Accounts.API.Integrations.As;
using Accounts.API.Integrations.AwsS3;
using Accounts.API.Integrations.BulllaEmpresa;
using Accounts.API.Integrations.BulllaEmpresa.Interfaces;
using Accounts.API.Integrations.BulllaEmpresa.Request;
using Accounts.API.Integrations.Cep;
using Accounts.API.Interfaces.Repositories;
using Accounts.API.Interfaces.Services;
using Accounts.API.Repositories;
using AutoMapper;
using Castle.Core.Resource;
using CsvHelper;
using CsvHelper.Configuration;
using Maoli;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration.CommandLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Accounts.API.Common.Dto.Solicitation;
using Users.API.Interfaces.Services;
using Portal.API.Interfaces.Services;
using Portal.API.Dto.Notification;
using Accounts.API.Common;
using System.Threading;
using Users.API.Interfaces.Repositories;
using JsonSerializer = System.Text.Json.JsonSerializer;
using UserTypeEnum = Accounts.API.Common.Enum.UserTypeEnum;



namespace Accounts.API.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IShopRepository _shopRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly ICostCenterRepository _costCenterRepository;
        private readonly ILimitRequestRepository _limitRequestRepository;
        private readonly ISolicitationRepository _solicitationRepository;
        private readonly IBulllaEmpresaIntegration _bulllaEmpresaIntegration;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IAsIntegration _asIntegration;
        private RabbitService rabbitService;
        private readonly ILogger<BulllaEmpresaIntegration> _logger;
        private readonly EnvironmentsBase environmentsBase;

        public AccountService(
            IAccountRepository accountRepository,
            IShopRepository shopRepository,
            IBranchRepository branchRepository,
            ICostCenterRepository costCenterRepository,
            ILimitRequestRepository limitRequestRepository,
            ISolicitationRepository solicitationRepository,
            IBulllaEmpresaIntegration bulllaEmpresaIntegration,
            IActionContextAccessor actionContextAccessor,
            IUserService userService,
            IUserRepository userRepository,
            INotificationService notificationService,
            IAsIntegration asIntegration,
            IConfiguration config,
            IMapper mapper,
            ILogger<BulllaEmpresaIntegration> logger)
        {
            _accountRepository = accountRepository;
            _shopRepository = shopRepository;
            _branchRepository = branchRepository;
            _costCenterRepository = costCenterRepository;
            _limitRequestRepository = limitRequestRepository;
            _solicitationRepository = solicitationRepository;
            _bulllaEmpresaIntegration = bulllaEmpresaIntegration;
            _actionContextAccessor = actionContextAccessor;
            _userService = userService;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _asIntegration = asIntegration;
            environmentsBase = new EnvironmentsBase(config);

            _mapper = mapper;
            _config = config;
            _logger = logger;

            rabbitService = new RabbitService(config);

            _logger = logger;
        }
        public async Task<ResponseDto<BlockedAccountDto>> GetBlockedPaged(int limit, int skip, string order, BlockedAccountFilterDto filter)
        {
            var entities = await _accountRepository.GetBlockedPaged(limit, skip, order, filter);
            var dtos = _mapper.Map<ResponseDto<BlockedAccountDto>>(entities);
            return dtos;
        }
        public async Task<List<BlockedAccountDto>> GetBlocked(BlockedAccountFilterDto filter)
        {
            var accounts = await _accountRepository.GetBlocked(filter);
            return _mapper.Map<List<BlockedAccountDto>>(accounts);
        }
        public async Task<ResponseDto<AccountDto>> GetAllPaged(long convenio_id, int limit, int skip, string search, string order, string? accountStatus, string? cardStatus, string? cardsFilter)
        {
            var entities = await _accountRepository.GetAllPaged(convenio_id, limit, skip, search, order, accountStatus, cardStatus, cardsFilter);
            var dtos = _mapper.Map<ResponseDto<AccountDto>>(entities);
            return dtos;
        }
        public async Task Create(AccountAddDto account)
        {
            var convenio = await _shopRepository.GetById(account.Convenio);
            var existing = await this.GetByRegistrationNumber(account.RegistrationNumber, account.Convenio);
            if (existing != null)
            {
                //throw new Exception("Já existe colaborador cadastrado com essa matrícula!");
                _actionContextAccessor.ActionContext.ModelState.AddModelError(nameof(account.RegistrationNumber), "Já existe colaborador cadastrado com essa matrícula!");
                return;
            }
            existing = await this.GetByCpfAndConvenio(account.Cpf, account.Convenio);
            if (existing != null)
            {
                //throw new Exception("Já existe colaborador cadastrado com esse CPF!");
                _actionContextAccessor.ActionContext.ModelState.AddModelError(nameof(account.Cpf), "Já existe colaborador cadastrado com esse CPF!");
                return;
            }
            var costCenters = await _costCenterRepository.GetAllByConvenio(account.Convenio);
            var branches = await _branchRepository.GetAllByConvenio(account.Convenio);

            var costCenterCode = account.CostCenter > 0 ? costCenters.FirstOrDefault(x => x.Id == account.CostCenter).InternalCode : "";
            var branchCode = account.Branch > 0 ? branches.FirstOrDefault(x => x.Id == account.Branch).Code : "";

            var csv = new StringBuilder();
            var enc = new UTF8Encoding(true);
            var header = @"Matricula;Nome;CPF;RG;Orgao;Data Nascimento;Data Admissao;Nome Mae;Nome Pai;Celular;Email;CNPJ;Codigo Filial;Nome Filial;Codigo Centro de Custo;Nome Centro de Custo;Unidade Entrega;Entrega Colaborador;Sexo;Nacionalidade;Cargo;CEP;Endereco;Numero;Complemento;Bairro;Cidade;UF;Nome Cartao;Limite;Codigo Usuario;Nome Usuario;Email Usuario";
            csv.AppendLine(header);
            var birthDate = account.BirthDate.ToString("dd/MM/yyyy");
            var admissionDate = account.AdmissionDate != null ? ((DateTime)account.AdmissionDate).ToString("dd/MM/yyyy") : "";
            var cardLimit = account.CardLimit.ToString();
            var line = $"{account.RegistrationNumber};{account.Name};{account.Cpf};;;{birthDate};{admissionDate};{account.MothersName};;{account.PhoneNumber};{account.Email};;{branchCode};;{costCenterCode};;;N;;;;{account.ZipCode};{account.Street};{account.AddressNumber};{account.AddressComplement};{account.Neighborhood};{account.CityName};{account.State};{account.CardName};{cardLimit};{account.CurrentUser.Id};{account.CurrentUser.Name};{account.CurrentUser.Email}";
            csv.AppendLine(line);
            byte[] preamble = enc.GetPreamble();
            byte[] byteArray = enc.GetBytes(csv.ToString());

            MemoryStream stream = new MemoryStream(Combine(preamble, byteArray));
            await AwsS3Integration.UploadFileAsync(stream, "accounts/import/" + account.Convenio.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_INDIVIDUAL.csv");

            var solicitation = new Solicitation()
            {
                SolicitationType = SolicitationTypeEnum.CardRequest,
                UserId = account.CurrentUser.Id,
                ShopId = account.Convenio,
                ShopName = convenio.Name,
                ShopDocument = convenio.Cnpj,
                GroupId = convenio.IdGroup,
                GroupName = convenio.GroupName,
                ClientId = null,
                Name = account.Name,
                Email = account.Email,
                Phone = account.PhoneNumber,
                Cpf = account.Cpf,
                PreviousLimit = 0,
                NewLimit = account.CardLimit,
                AccountStatus = "",
                CardStatus = "",
                Rg = account.Rg,
                BirthDate = account.BirthDate,
                AdmissionDate = account.AdmissionDate,
                ZipCode = account.ZipCode,
                Street = account.Street,
                AddressNumber = account.AddressNumber,
                AddressComplement = account.AddressComplement,
                Neighborhood = account.Neighborhood,
                CityName = account.CityName,
                State = account.State,
                CostCenterId = account.CostCenter,
                CostCenterName = account.CostCenter > 0 ? costCenters.FirstOrDefault(x => x.Id == account.CostCenter).Description : "",
                BranchId = account.Branch,
                BranchName = account.Branch > 0 ? branches.FirstOrDefault(x => x.Id == account.Branch).Description : "",
                Observation = "A solicitação foi recebida com sucesso! \nSeu pedido será processado em até 5 dias úteis.\nO prazo de entrega, no local indicado em seu pedido, é de até 7 dias úteis para as capitais e regiões metropolitanas e de até 15 dias úteis para as demais regiões."
            };
            await _solicitationRepository.Create(solicitation);
        }
        public async Task Update(AccountAddDto account)
        {
            var convenio = await _shopRepository.GetById(account.Convenio);
            var costCenters = await _costCenterRepository.GetAllByConvenio(account.Convenio);
            var branches = await _branchRepository.GetAllByConvenio(account.Convenio);
            var customer = await _accountRepository.GetByCpfAndConvenio(account.Cpf.Replace(".", "").Replace("-", ""), account.Convenio);
            var csv = new StringBuilder();
            var enc = new UTF8Encoding(true);
            var header = @"Matricula;Nome;CPF;RG;Orgao;Data Nascimento;Data Admissao;Nome Mae;Nome Pai;Celular;Email;CNPJ;Codigo Filial;Nome Filial;Codigo Centro de Custo;Nome Centro de Custo;Unidade Entrega;Entrega Colaborador;Sexo;Nacionalidade;Cargo;CEP;Endereco;Numero;Complemento;Bairro;Cidade;UF;Nome Cartao;Limite;Codigo Usuario;Nome Usuario;Email Usuario";
            csv.AppendLine(header);
            var birthDate = account.BirthDate.ToString("dd/MM/yyyy");
            var admissionDate = account.AdmissionDate != null ? ((DateTime)account.AdmissionDate).ToString("dd/MM/yyyy") : "";
            var cardLimit = account.CardLimit.ToString();
            var line = $"{account.RegistrationNumber};{account.Name};{account.Cpf};;;{birthDate};{admissionDate};{account.MothersName};;{account.PhoneNumber};{account.Email};;;;;;;N;;;;{account.ZipCode};{account.Street};{account.AddressNumber};{account.AddressComplement};{account.Neighborhood};{account.CityName};{account.State};{account.CardName};{cardLimit};{account.CurrentUser.Id};{account.CurrentUser.Name};{account.CurrentUser.Email}";
            csv.AppendLine(line);
            byte[] preamble = enc.GetPreamble();
            byte[] byteArray = enc.GetBytes(csv.ToString());

            MemoryStream stream = new MemoryStream(Combine(preamble, byteArray));
            await AwsS3Integration.UploadFileAsync(stream, "accounts/import/" + account.Convenio.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_ALTERACAO.csv");

            var branch  = branches.FirstOrDefault(x => x.Id == account.Branch);
            var costCenter = costCenters.FirstOrDefault(x => x.Id == account.CostCenter);

            var solicitation = new Solicitation()
            {
                SolicitationType = SolicitationTypeEnum.AccountUpdate,
                UserId = account.CurrentUser.Id,
                ShopId = account.Convenio,
                ShopName = convenio.Name,
                ShopDocument = convenio.Cnpj,
                GroupId = convenio.IdGroup,
                GroupName = convenio.GroupName,
                ClientId = customer.Id,
                Name = account.Name,
                Email = account.Email,
                Phone = account.PhoneNumber,
                Cpf = account.Cpf,
                PreviousLimit = customer.CardLimit,
                NewLimit = customer.CardLimit,
                AccountStatus = customer.Status,
                CardStatus = customer.CardStatus,
                Rg = customer.Rg,
                BirthDate = account.BirthDate,
                AdmissionDate = account.AdmissionDate,
                ZipCode = account.ZipCode,
                Street = account.Street,
                AddressNumber = account.AddressNumber,
                AddressComplement = account.AddressComplement,
                Neighborhood = account.Neighborhood,
                CityName = account.CityName,
                State = account.State,
                CostCenterId = account.CostCenter,
                CostCenterName = account.CostCenter > 0 ? (costCenter ?? new CostCenter()).Description : "",
                BranchId = account.Branch,
                BranchName = account.Branch > 0 ? (branch ?? new Branch()).Description : "",
                Observation = "A solicitação foi recebida com sucesso! \nAguarde até 48h úteis para concluirmos sua alteração."

            };
            await _solicitationRepository.Create(solicitation);
        }
        public async Task Block(AccountBlockDto accounts)
        {
            var convenio = await _shopRepository.GetById(accounts.Convenio);
            var outrosBloqueios = accounts.Accounts.Where(a => a.Reason != "2").ToList();
            if (outrosBloqueios.Count > 0)
            {
                var csv = new StringBuilder();
                var enc = new UTF8Encoding(true);
                var header = @"Matricula;Nome;CPF;RG;Orgao;Data Nascimento;Data Admissao;Nome Mae;Nome Pai;Celular;Email;CNPJ;Codigo Filial;Nome Filial;Codigo Centro de Custo;Nome Centro de Custo;Unidade Entrega;Entrega Colaborador;Sexo;Nacionalidade;Cargo;CEP;Endereco;Numero;Complemento;Bairro;Cidade;UF;Nome Cartao;Limite;Movimento;Codigo Usuario;Nome Usuario;Email Usuario;Arquivo";
                csv.AppendLine(header);
                foreach (var account in outrosBloqueios)
                {
                    var link = "";
                    if (account.File != null)
                    {
                        var ext = Path.GetExtension(account.File.FileName);
                        var attachmentName = accounts.Convenio.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + account.Cpf + ext;
                        var attachmentKey = "accounts/anexo-bloqueio/" + attachmentName;
                        using var memoryStream = new MemoryStream();
                        account.File.CopyTo(memoryStream);
                        await AwsS3Integration.UploadFileAsync(memoryStream, attachmentKey);
                        link = AwsS3Integration.SignedUrl(attachmentKey);
                    }
                    var line = $"{account.RegistrationNumber};{account.Name};{account.Cpf};;;;;;;{account.PhoneNumber};{account.Email};;;;;;;N;;;;;;;;;;;;;7;{accounts.CurrentUser.Id};{accounts.CurrentUser.Name};{accounts.CurrentUser.Email};{link}";
                    csv.AppendLine(line);
                }
                byte[] preamble = enc.GetPreamble();
                byte[] byteArray = enc.GetBytes(csv.ToString());

                MemoryStream stream = new MemoryStream(Combine(preamble, byteArray));
                await AwsS3Integration.UploadFileAsync(stream, "accounts/import/" + accounts.Convenio.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_BLOQUEIO.csv");
            }
            var bloqueioInss = accounts.Accounts.Where(a => a.Reason == "2").ToList();
            if (bloqueioInss.Count > 0)
            {
                var csv = new StringBuilder();
                var enc = new UTF8Encoding(true);
                var header = @"Convenio;Matricula;Nome;CPF;Celular;Email;Motivo;Codigo Usuario;Nome Usuario;Email Usuario;Arquivo";
                csv.AppendLine(header);
                foreach (var account in bloqueioInss)
                {
                    var link = "";
                    if (account.File != null)
                    {
                        var ext = Path.GetExtension(account.File.FileName);
                        var attachmentName = accounts.Convenio.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + account.Cpf + ext;
                        var attachmentKey = "accounts/anexo-bloqueio/" + attachmentName;
                        using var memoryStream = new MemoryStream();
                        account.File.CopyTo(memoryStream);
                        await AwsS3Integration.UploadFileAsync(memoryStream, attachmentKey);
                        link = AwsS3Integration.SignedUrl(attachmentKey);
                    }
                    var line = $"{accounts.Convenio};{account.RegistrationNumber};{account.Name};{account.Cpf};{account.PhoneNumber};{account.Email};INSS;{accounts.CurrentUser.Id};{accounts.CurrentUser.Name};{accounts.CurrentUser.Email};{link}";
                    csv.AppendLine(line);
                }
                byte[] preamble = enc.GetPreamble();
                byte[] byteArray = enc.GetBytes(csv.ToString());

                MemoryStream stream = new MemoryStream(Combine(preamble, byteArray));
                var filename = accounts.Convenio.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_BLOQUEIO_INSS.csv";
                var key = "accounts/bloqueio/" + filename;
                await AwsS3Integration.UploadFileAsync(stream, key);

                string to = _config.GetValue<string>("emails:limitEmissaoCartaoIndividual");
                var message = new SendEmailMessageDto()
                {
                    from = "atendimento@bullla.com.br",
                    to = to,
                    subject = "Bloqueio INSS",
                    html = @$"<p>Segue em anexo arquivo para bloqueio dos cartões do convênio {accounts.Convenio.ToString()}.</p>",
                    text = @$"Segue em anexo arquivo para bloqueio dos cartões do convênio {accounts.Convenio.ToString()}."
                };
                message.attachments.Add(new MessageAttachmentDto()
                {
                    fileName = filename,
                    key = key
                });
                await Task.Run(() => rabbitService.Publish("send-email", JsonConvert.SerializeObject(message)));
            }
            foreach (var account in accounts.Accounts)
            {
                var customer = await _accountRepository.GetByCpfAndConvenio(account.Cpf, accounts.Convenio);
                if (customer != null)
                {
                    var solicitation = new Solicitation()
                    {
                        SolicitationType = SolicitationTypeEnum.CardBlock,
                        UserId = accounts.CurrentUser.Id,
                        ShopId = accounts.Convenio,
                        ShopName = convenio.Name,
                        ShopDocument = convenio.Cnpj,
                        GroupId = convenio.IdGroup,
                        GroupName = convenio.GroupName,
                        ClientId = customer.Id,
                        Name = customer.Name,
                        Email = customer.Email,
                        Phone = customer.PhoneNumber,
                        Cpf = customer.Cpf,
                        PreviousLimit = customer.CardLimit,
                        NewLimit = customer.CardLimit,
                        AccountStatus = customer.Status,
                        CardStatus = customer.CardStatus,
                        BlockType = account.Reason,
                        Observation = ""
                    };
                    await _solicitationRepository.Create(solicitation);
                }
            }
            //return _mapper.Map<AccountDto>(await _accountRepository.Create(_mapper.Map<Account>(account)));
        }
        public async Task<List<MessageAttachmentDto>> GlobalLimit(GlobalLimitFileUploadDto model)
        {
            try
            {
                var convenio = await _shopRepository.GetById(model.shopId);

                string to = _config.GetValue<string>("emails:globalLimit");

                Dictionary<string, string> anexos = new Dictionary<string, string>();

                foreach (var file in model.files)
                {
                    if (file.file != null)
                    {
                        var ext = Path.GetExtension(file.file.FileName);
                        var filename = model.shopId.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + file.description + ext;
                        var key = "accounts/global-limit/" + filename;
                        using var stream = new MemoryStream();
                        file.file.CopyTo(stream);
                        await AwsS3Integration.UploadFileAsync(stream, key);
                        anexos.Add(filename, key);
                    }
                }

                var html = @$"<p>Nova solicitação de aumento de limite global.</p><p>Convênio: {model.shopId.ToString()} - {model.shopName}<br>Grupo: {model.groupId.ToString()} - {model.groupName}<br>Número de Funcionários: {model.numberofstaff}</p>";
                var text = @$"Nova solicitação de aumento de limite global.\nConvênio: {model.shopId.ToString()} - {model.shopName}\nGrupo: {model.groupId.ToString()} - {model.groupName}\nNúmero de Funcionários: {model.numberofstaff}";

                foreach (var anexo in anexos)
                {
                    var url = AwsS3Integration.SignedUrl(anexo.Value);

                    html += "<br />" + anexo.Key + @" - <a href=""" + url + "\" target=\"_blank\">Download</a>";
                    text += @"\n" + anexo.Key + " - " + url;
                }

                var message = new SendEmailMessageDto()
                {
                    ////TODO: MUDAR PARA PARAMETRO NO APP.SETTINGS
                    from = "atendimento@bullla.com.br",
                    to = to,
                    subject = "Aumento de Limite Global",
                    html = html,
                    text = text
                };

                await Task.Run(() => rabbitService.Publish("send-email", JsonConvert.SerializeObject(message)));

                //var solicitation = new Solicitation(SolicitationTypeEnum.GlobalLimitChange, model.CurrentUser.Id, model.shopId, model.groupName, convenio.Limit, model.globalLimit);
                var solicitation = new Solicitation()
                {
                    SolicitationType = SolicitationTypeEnum.GlobalLimitChange,
                    UserId = model.CurrentUser.Id,
                    ShopId = model.shopId,
                    ShopName = convenio.Name,
                    ShopDocument = convenio.Cnpj,
                    GroupId = convenio.IdGroup,
                    GroupName = convenio.GroupName,
                    PreviousLimit = convenio.Limit,
                    Observation = "A solicitação foi recebida com sucesso!\nSeu pedido será analisado em até 7 dias úteis."
                    //TODO: colocar number of staff (numero de funcionarios)

                };
                await _solicitationRepository.Create(solicitation);


                foreach (var anexo in anexos)
                {
                    message.attachments.Add(new MessageAttachmentDto()
                    {
                        fileName = anexo.Key,
                        key = anexo.Value
                    });
                }

                _logger.LogInformation("from - " + message.from);
                _logger.LogInformation("to - " + message.to);
                _logger.LogInformation("subject - " + message.subject);
                _logger.LogInformation("html - " + message.html);
                _logger.LogInformation("text - " + message.text);
                _logger.LogInformation("Enviado email com sucesso.");

                return message.attachments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "erro ao enviar email", null);

                throw ex;
            }
        }

        public async Task Unblock(AccountUnblockDto accounts)
        {
            var convenio = await _shopRepository.GetById(accounts.Convenio);
            var outrosBloqueios = accounts.Accounts.Where(a => a.Reason != "2").ToList();
            if (outrosBloqueios.Count > 0)
            {
                var csv = new StringBuilder();
                var enc = new UTF8Encoding(true);
                var header = @"Matricula;Nome;CPF;RG;Orgao;Data Nascimento;Data Admissao;Nome Mae;Nome Pai;Celular;Email;CNPJ;Codigo Filial;Nome Filial;Codigo Centro de Custo;Nome Centro de Custo;Unidade Entrega;Entrega Colaborador;Sexo;Nacionalidade;Cargo;CEP;Endereco;Numero;Complemento;Bairro;Cidade;UF;Nome Cartao;Limite;Movimento;Codigo Usuario;Nome Usuario;Email Usuario;Arquivo";
                csv.AppendLine(header);
                foreach (var account in outrosBloqueios)
                {
                    var link = "";
                    if (account.File != null)
                    {
                        var ext = Path.GetExtension(account.File.FileName);
                        var attachmentName = accounts.Convenio.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + account.Cpf + ext;
                        var attachmentKey = "accounts/anexo-desbloqueio/" + attachmentName;
                        using var memoryStream = new MemoryStream();
                        account.File.CopyTo(memoryStream);
                        await AwsS3Integration.UploadFileAsync(memoryStream, attachmentKey);
                        link = AwsS3Integration.SignedUrl(attachmentKey);
                    }
                    var line = $"{account.RegistrationNumber};{account.Name};{account.Cpf};;;;;;;{account.PhoneNumber};{account.Email};;;;;;;N;;;;;;;;;;;;;8;{accounts.CurrentUser.Id};{accounts.CurrentUser.Name};{accounts.CurrentUser.Email};{link}";
                    csv.AppendLine(line);
                }
                byte[] preamble = enc.GetPreamble();
                byte[] byteArray = enc.GetBytes(csv.ToString());

                MemoryStream stream = new MemoryStream(Combine(preamble, byteArray));
                await AwsS3Integration.UploadFileAsync(stream, "accounts/import/" + accounts.Convenio.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_DESBLOQUEIO.csv");
            }
            var bloqueioInss = accounts.Accounts.Where(a => a.Reason == "2").ToList();
            if (bloqueioInss.Count > 0)
            {
                var csv = new StringBuilder();
                var enc = new UTF8Encoding(true);
                var header = @"Convenio;Matricula;Nome;CPF;Celular;Email;Motivo;Codigo Usuario;Nome Usuario;Email Usuario;Arquivo";
                csv.AppendLine(header);
                foreach (var account in bloqueioInss)
                {
                    var link = "";
                    if (account.File != null)
                    {
                        var ext = Path.GetExtension(account.File.FileName);
                        var attachmentName = accounts.Convenio.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + account.Cpf + ext;
                        var attachmentKey = "accounts/anexo-desbloqueio/" + attachmentName;
                        using var memoryStream = new MemoryStream();
                        account.File.CopyTo(memoryStream);
                        await AwsS3Integration.UploadFileAsync(memoryStream, attachmentKey);
                        link = AwsS3Integration.SignedUrl(attachmentKey);
                    }
                    var line = $"{accounts.Convenio};{account.RegistrationNumber};{account.Name};{account.Cpf};{account.PhoneNumber};{account.Email};INSS;{accounts.CurrentUser.Id};{accounts.CurrentUser.Name};{accounts.CurrentUser.Email};{link}";
                    csv.AppendLine(line);
                }
                byte[] preamble = enc.GetPreamble();
                byte[] byteArray = enc.GetBytes(csv.ToString());

                MemoryStream stream = new MemoryStream(Combine(preamble, byteArray));
                var filename = accounts.Convenio.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_DESBLOQUEIO_INSS.csv";
                var key = "accounts/desbloqueio/" + filename;
                await AwsS3Integration.UploadFileAsync(stream, key);
                string to = _config.GetValue<string>("emails:limitEmissaoCartaoIndividual");
                var message = new SendEmailMessageDto()
                {
                    from = "atendimento@bullla.com.br",
                    to = to,
                    subject = "Desbloqueio INSS",
                    html = @$"<p>Segue em anexo arquivo para desbloqueio dos cartões do convênio {accounts.Convenio.ToString()}.</p>",
                    text = @$"Segue em anexo arquivo para desbloqueio dos cartões do convênio {accounts.Convenio.ToString()}."
                };
                message.attachments.Add(new MessageAttachmentDto()
                {
                    fileName = filename,
                    key = key
                });
                await Task.Run(() => rabbitService.Publish("send-email", JsonConvert.SerializeObject(message)));
            }
            foreach (var account in accounts.Accounts)
            {
                var customer = await _accountRepository.GetByCpfAndConvenio(account.Cpf, accounts.Convenio);
                if (customer != null)
                {
                    var solicitation = new Solicitation()
                    {
                        SolicitationType = SolicitationTypeEnum.CardUnblock,
                        UserId = accounts.CurrentUser.Id,
                        ShopId = accounts.Convenio,
                        ShopName = convenio.Name,
                        ShopDocument = convenio.Cnpj,
                        GroupId = convenio.IdGroup,
                        GroupName = convenio.GroupName,
                        ClientId = customer.Id,
                        Name = customer.Name,
                        Email = customer.Email,
                        Phone = customer.PhoneNumber,
                        Cpf = customer.Cpf,
                        PreviousLimit = customer.CardLimit,
                        NewLimit = customer.CardLimit,
                        AccountStatus = customer.Status,
                        CardStatus = customer.CardStatus,
                        BlockType = account.Reason,
                        Observation = ""
                    };
                    await _solicitationRepository.Create(solicitation);
                }
            }
            //return _mapper.Map<AccountDto>(await _accountRepository.Create(_mapper.Map<Account>(account)));
        }

        public async Task Reissue(AccountReissueDto accounts)
        {
            var convenio = await _shopRepository.GetById(accounts.Convenio);
            var csv = new StringBuilder();
            var enc = new UTF8Encoding(true);
            var header = @"Matricula;Nome;CPF;RG;Orgao;Data Nascimento;Data Admissao;Nome Mae;Nome Pai;Celular;Email;CNPJ;Codigo Filial;Nome Filial;Codigo Centro de Custo;Nome Centro de Custo;Unidade Entrega;Entrega Colaborador;Sexo;Nacionalidade;Cargo;CEP;Endereco;Numero;Complemento;Bairro;Cidade;UF;Nome Cartao;Limite;Movimento;Codigo Usuario;Nome Usuario;Email Usuario";
            csv.AppendLine(header);
            foreach (var account in accounts.Accounts)
            {
                var line = $"{account.RegistrationNumber};{account.Name};{account.Cpf};;;;;;;{account.PhoneNumber};{account.Email};;;;;;;N;;;;;;;;;;;;;5;{accounts.CurrentUser.Id};{accounts.CurrentUser.Name};{accounts.CurrentUser.Email}";
                csv.AppendLine(line);
            }
            byte[] preamble = enc.GetPreamble();
            byte[] byteArray = enc.GetBytes(csv.ToString());

            MemoryStream stream = new MemoryStream(Combine(preamble, byteArray));
            await AwsS3Integration.UploadFileAsync(stream, "accounts/import/" + accounts.Convenio.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_SEGUNDAVIA.csv");
            foreach (var account in accounts.Accounts)
            {
                var customer = await _accountRepository.GetByCpfAndConvenio(account.Cpf, accounts.Convenio);
                if (customer != null)
                {
                    var solicitation = new Solicitation()
                    {
                        SolicitationType = SolicitationTypeEnum.CardReplacement,
                        UserId = accounts.CurrentUser.Id,
                        ShopId = accounts.Convenio,
                        ShopName = convenio.Name,
                        ShopDocument = convenio.Cnpj,
                        GroupId = convenio.IdGroup,
                        GroupName = convenio.GroupName,
                        ClientId = customer.Id,
                        Name = customer.Name,
                        Email = customer.Email,
                        Phone = customer.PhoneNumber,
                        Cpf = customer.Cpf,
                        PreviousLimit = customer.CardLimit,
                        NewLimit = customer.CardLimit,
                        AccountStatus = customer.Status,
                        CardStatus = customer.CardStatus,
                        Observation = "A solicitação foi recebida com sucesso!\nSeu pedido será processado em até 5 dias úteis.\nO prazo para entrega é de até 7 dias úteis no local indicado em contrato."
                    };
                    await _solicitationRepository.Create(solicitation);
                }
            }
            //return _mapper.Map<AccountDto>(await _accountRepository.Create(_mapper.Map<Account>(account)));
        }
        public async Task ChangeLimit(AccountLimitDto accounts)
        {
            var convenio = await _shopRepository.GetById(accounts.Convenio);
            if (accounts.CurrentUser.UserType.ToLower() != "master")
            {
                foreach (var account in accounts.Accounts)
                {
                    var limitRequest = new LimitRequest()
                    {
                        ShopId = accounts.Convenio,
                        Cpf = account.Cpf,
                        Name = account.Name,
                        RegistrationNumber = account.RegistrationNumber,
                        Status = "PENDENTE",
                        UserId = accounts.CurrentUser.Id,
                        PreviousLimit = (decimal)account.CardLimit,
                        NewLimit = (decimal)account.NewCardLimit

                    };
                    var res = await _limitRequestRepository.Create(limitRequest);
                    var customer = await _accountRepository.GetByCpfAndConvenio(account.Cpf, accounts.Convenio);
                    if (customer != null)
                    {
                        var solicitation = new Solicitation()
                        {
                            SolicitationType = SolicitationTypeEnum.LimitChange,
                            UserId = accounts.CurrentUser.Id,
                            ShopId = accounts.Convenio,
                            ShopName = convenio.Name,
                            ShopDocument = convenio.Cnpj,
                            GroupId = convenio.IdGroup,
                            GroupName = convenio.GroupName,
                            ClientId = customer.Id,
                            Name = customer.Name,
                            Email = customer.Email,
                            Phone = customer.PhoneNumber,
                            Cpf = customer.Cpf,
                            PreviousLimit = customer.CardLimit,
                            NewLimit = (decimal)account.NewCardLimit,
                            AccountStatus = customer.Status,
                            CardStatus = customer.CardStatus,
                            Observation = "A solicitação foi realizada com sucesso!\nEm até 48h o limite estará disponível."
                        };
                        await _solicitationRepository.Create(solicitation);
                    }
                }

            }
            else
            {


                var csv = new StringBuilder();
                var enc = new UTF8Encoding(true);
                var header = @"Matricula;Nome;CPF;RG;Orgao;Data Nascimento;Data Admissao;Nome Mae;Nome Pai;Celular;Email;CNPJ;Codigo Filial;Nome Filial;Codigo Centro de Custo;Nome Centro de Custo;Unidade Entrega;Entrega Colaborador;Sexo;Nacionalidade;Cargo;CEP;Endereco;Numero;Complemento;Bairro;Cidade;UF;Nome Cartao;Limite;Movimento;Codigo Usuario;Nome Usuario;Email Usuario";
                csv.AppendLine(header);
                foreach (var account in accounts.Accounts)
                {
                    var line = $"{account.RegistrationNumber};{account.Name};{account.Cpf};;;;;;;;;;;;;;;N;;;;;;;;;;;;{account.NewCardLimit};9;{accounts.CurrentUser.Id};{accounts.CurrentUser.Name};{accounts.CurrentUser.Email}";
                    csv.AppendLine(line);
                    var customer = await _accountRepository.GetByCpfAndConvenio(account.Cpf, accounts.Convenio);
                    if (customer != null)
                    {
                        var solicitation = new Solicitation()
                        {
                            SolicitationType = SolicitationTypeEnum.LimitChange,
                            UserId = accounts.CurrentUser.Id,
                            ShopId = accounts.Convenio,
                            ShopName = convenio.Name,
                            ShopDocument = convenio.Cnpj,
                            GroupId = convenio.IdGroup,
                            GroupName = convenio.GroupName,
                            ClientId = customer.Id,
                            Name = customer.Name,
                            Email = customer.Email,
                            Phone = customer.PhoneNumber,
                            Cpf = customer.Cpf,
                            PreviousLimit = customer.CardLimit,
                            NewLimit = (decimal)account.NewCardLimit,
                            AccountStatus = customer.Status,
                            CardStatus = customer.CardStatus,
                            Observation = "A solicitação foi realizada com sucesso!\nEm até 48h o limite estará disponível."
                        };
                        await _solicitationRepository.Create(solicitation);
                    }
                }
                byte[] preamble = enc.GetPreamble();
                byte[] byteArray = enc.GetBytes(csv.ToString());

                MemoryStream stream = new MemoryStream(Combine(preamble, byteArray));
                await AwsS3Integration.UploadFileAsync(stream, "accounts/import/" + accounts.Convenio.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_LIMITE.csv");

            }
            //Dados:
            //Limite atual
            //    Novo limite
            //    cpf 
            //    final do cartao
            //    convenio
            //var header = @"Nome;CPF;Limite Atual;Novo Limite";
            //csv.AppendLine(header);
            //foreach (var account in accounts.Accounts)
            //{
            //    var line = $"{account.Name};{account.Cpf};{account.CardLimit};{account.NewCardLimit}";
            //    csv.AppendLine(line);
            //}
            //byte[] preamble = enc.GetPreamble();
            //byte[] byteArray = enc.GetBytes(csv.ToString());

            //MemoryStream stream = new MemoryStream(Combine(preamble, byteArray));
            //var filename = accounts.Convenio.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_LIMITE.csv";
            //var key = "accounts/limit/" + filename;
            //await AwsS3Integration.UploadFileAsync(stream, key);
            //var message = new SendEmailMessageDto()
            //{
            //    from = "atendimento@bullla.com.br",
            //    to = "demandaportal@bullla.com.br",
            //    subject = "Alteração de Limite",
            //    html = @$"<p>Segue em anexo arquivo para alteração de limite dos cartões do convênio {accounts.Convenio.ToString()}.</p>",
            //    text = @$"Segue em anexo arquivo para alteração de limite dos cartões do convênio {accounts.Convenio.ToString()}."
            //};
            //message.attachments.Add(new MessageAttachmentDto()
            //{
            //    fileName = filename,
            //    key = key
            //});
            //await Task.Run(() => RabbitService.Publish("send-email", JsonConvert.SerializeObject(message)));
            //return _mapper.Map<AccountDto>(await _accountRepository.Create(_mapper.Map<Account>(account)));
        }
        public async Task<List<AccountDto>> GetAllByConvenio(long convenio)
        {
            var accounts = await _accountRepository.GetAllByConvenio(convenio);
            return _mapper.Map<List<AccountDto>>(accounts);
        }
        public async Task<AccountDto> GetByRegistrationNumber(string registration, long convenio)
        {
            var account = await _accountRepository.GetByRegistrationNumber(registration, convenio);
            return _mapper.Map<AccountDto>(account);
        }
        public async Task<AccountDto> GetByCpfAndConvenio(string cpf, long convenio)
        {
            var account = await _accountRepository.GetByCpfAndConvenio(cpf, convenio);
            return _mapper.Map<AccountDto>(account);
        }
        public async Task<AccountDto> GetByCpfAndGrupo(string cpf, long grupo)
        {
            var account = await _accountRepository.GetByCpfAndGrupo(cpf, grupo);
            return _mapper.Map<AccountDto>(account);
        }

        public async Task<AccountDto> Get(long id, long convenio)
        {
            var account = await _accountRepository.Get(id, convenio);
            return _mapper.Map<AccountDto>(account);
        }
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }
        public async Task RegisterImport(FileUploadDto fileUpload, long convenioId, UserDtoAccounts currentUser)
        {
            var convenio = await _shopRepository.GetById(convenioId);
            List<AccountCsvDto> result;
            using (var reader = new StreamReader(fileUpload.FormFile.OpenReadStream()))
            {
                var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
                config.HasHeaderRecord = true;
                config.Delimiter = ";";
                config.BadDataFound = null;
                config.IgnoreBlankLines = true;
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Context.RegisterClassMap<CsvMapperAccountDto>();
                    result = csv.GetRecords<AccountCsvDto>().ToList();
                }
            }
            foreach (var account in result)
            {

                
                var NewLimit = 0M;
                if (!string.IsNullOrEmpty(account.CardLimit) && !string.IsNullOrWhiteSpace(account.CardLimit))
                {
                    var newlimiteString = account.CardLimit.Replace(".", "").Replace(",", ".");
                    Decimal.TryParse(newlimiteString, out NewLimit);
                }

                long CostCenterId = 0;
                if (!string.IsNullOrEmpty(account.CostCenterCode) && !string.IsNullOrWhiteSpace(account.CostCenterCode))
                {
                    Int64.TryParse(account.CostCenterCode, out CostCenterId);
                }

                long BranchCode = 0;
                if (!string.IsNullOrEmpty(account.BranchCode) && !string.IsNullOrWhiteSpace(account.BranchCode))
                {
                    Int64.TryParse(account.BranchCode, out BranchCode);
                }

                var solicitation = new Solicitation()
                {
                    SolicitationType = SolicitationTypeEnum.CardRequest,
                    UserId = currentUser.Id,
                    ShopId = convenioId,
                    ShopName = convenio.Name,
                    ShopDocument = convenio.Cnpj,
                    GroupId = convenio.IdGroup,
                    GroupName = convenio.GroupName,
                    ClientId = null,
                    Name = account.Name,
                    Email = account.Email,
                    Phone = account.PhoneNumber,
                    Cpf = account.Cpf,
                    PreviousLimit = 0,
                    NewLimit = NewLimit,
                    AccountStatus = "",
                    CardStatus = "",
                    Rg = "",
                    ZipCode = account.ZipCode,
                    Street = account.Street,
                    AddressNumber = account.AddressNumber,
                    AddressComplement = account.AddressComplement,
                    Neighborhood = account.Neighborhood,
                    CityName = account.CityName,
                    State = account.State,
                    CostCenterId = CostCenterId,
                    CostCenterName = account.CostCenterName,
                    BranchId = BranchCode,
                    BranchName = account.BranchName,
                    Observation = "A solicitação foi recebida com sucesso! \nSeu pedido será processado em até 5 dias úteis.\nO prazo de entrega, no local indicado em seu pedido, é de até 7 dias úteis para as capitais e regiões metropolitanas e de até 15 dias úteis para as demais regiões."

                };
                if (!String.IsNullOrEmpty(account.BirthDate) && DateTime.TryParseExact(account.BirthDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                {
                    solicitation.BirthDate = dt;
                }
                if (!String.IsNullOrEmpty(account.AdmissionDate) && DateTime.TryParseExact(account.AdmissionDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt2))
                {
                    solicitation.AdmissionDate = dt2;
                }
                await _solicitationRepository.Create(solicitation);

            }
        }

        public async Task UploadFileNewCard(FileUploadDto fileUpload, long convenio, UserDtoAccounts currentUser)
        {
            int maxLines = 1000;
            Regex cpfRegex = new Regex(@"(\d{3})\.(\d{3})\.(\d{3})-(\d{2})"); // Match CPF number
            Regex cnpjRegex = new Regex(@"(\d{2})\.(\d{3})\.(\d{3})\/(\d{4})-(\d{2})"); // Match CNPJ number

            using (var stream = new StreamReader(fileUpload.FormFile.OpenReadStream()))
            {
                int lineCount = 0;
                while (stream.ReadLine() != null)
                {
                    lineCount++;
                }

                int fileCount = (int)Math.Ceiling((double)lineCount / maxLines);

                using (var inputStream = new StreamReader(fileUpload.FormFile.OpenReadStream()))
                {
                    string header = inputStream.ReadLine();
                    header += ";Codigo Usuario;Nome Usuario;Email Usuario";

                    for (int i = 0; i < fileCount; i++)
                    {
                        int startIndex = i * maxLines + 1;
                        int endIndex = Math.Min(((i + 1) * maxLines) + 1, lineCount) - 1;

                        string[] currentLines = new string[endIndex - startIndex + 2];
                        currentLines[0] = header;

                        int currentLineIndex = 1;
                        while (!inputStream.EndOfStream && (currentLineIndex + startIndex - 1) <= endIndex)
                        {
                            var line  = inputStream.ReadLine();
                            string cpf = cpfRegex.Match(line).Value; 
                            string cnpj = cnpjRegex.Match(line).Value;

                            string newCpf = cpfRegex.Replace(cpf, "$1$2$3$4"); 
                            string newCnpj = cnpjRegex.Replace(cnpj, "$1$2$3$4$5");

                            currentLines[currentLineIndex++] = line.Replace(cpf, newCpf).Replace(cnpj, newCnpj).ToUpper() + $";{currentUser.Id};{currentUser.Name};{currentUser.Email}";
                        }
                        using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, currentLines))))
                        {
                            await AwsS3Integration.UploadFileAsync(memoryStream, "accounts/import/" + convenio.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + $"{i + 1}_REMESSA.csv");
                        }
                    }
                }
            }
        }

        public async Task<AccountValidationDto> ValidateFile(FileUploadDto fileUpload, long convenio)
        {
            var accountValidation = new AccountValidationDto();
            List<AccountCsvDto> result;
            using (var reader = new StreamReader(fileUpload.FormFile.OpenReadStream()))
            {
                var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
                config.HasHeaderRecord = true;
                config.Delimiter = ";";
                config.BadDataFound = null;
                config.IgnoreBlankLines = true;
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Context.RegisterClassMap<CsvMapperAccountDto>();
                    result = csv.GetRecords<AccountCsvDto>().ToList();
                }
            }

            var accounts = new List<AccountDto>();
            //validate each record
            var lineNumber = 0;
            foreach (var res in result)
            {

                lineNumber++;
                //add occurrence to Accountvalidation
                if (res.RegistrationNumber == null || res.RegistrationNumber == "")
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "Matricula",
                        Message = "Matricula não pode ser vazia"
                    });
                }
                else if (!res.RegistrationNumber.Any(c => char.IsDigit(c)))
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "Matricula",
                        Message = "Matricula deve conter pelo menos um número"
                    });
                }
                else
                {
                    var existing = await this.GetByRegistrationNumber(res.RegistrationNumber, convenio);
                    if (existing != null)
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Matricula",
                            Message = "Matricula já existe"
                        });
                    }
                }
                //Validação Nome
                if (String.IsNullOrEmpty(res.Name))
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "Nome",
                        Message = "Nome não pode ser vazio"
                    });
                }
                else if (res.Name.Any(c => !char.IsLetter(c) && !char.IsWhiteSpace(c)))
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "Nome",
                        Message = "Nome deve conter apenas letras"
                    });
                }
                else if (res.Name.Length < 3)
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "Nome",
                        Message = "Nome deve conter pelo menos 3 caracteres"
                    });
                }
                //Validação CPF
                if (String.IsNullOrEmpty(res.Cpf))
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "CPF",
                        Message = "CPF não pode ser vazio"
                    });
                }
                else if (res.Cpf.Length != 14 || res.Cpf.Replace(".", "").Replace("-", "").Length != 11)
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "CPF",
                        Message = "CPF deve conter 14 caracteres no formato 000.000.000-00"
                    });
                }
                else if (!Cpf.Validate(res.Cpf))
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "CPF",
                        Message = "CPF inválido"
                    });
                }
                else
                {
                    res.Cpf = res.Cpf.Replace(".", "").Replace("-", "");
                    var existing = await this.GetByCpfAndConvenio(res.Cpf, convenio);
                    if (existing != null)
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "CPF",
                            Message = "CPF já existe"
                        });
                    }

                }
                //Validação Data de Nascimento
                if (String.IsNullOrEmpty(res.BirthDate))
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "Data de Nascimento",
                        Message = "Data de Nascimento não pode ser vazia"
                    });
                }
                else if (res.BirthDate.Length != 10)
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "Data de Nascimento",
                        Message = "Data de Nascimento deve conter 10 dígitos no formato DD/MM/AAAA"
                    });
                }
                else if (!DateTime.TryParseExact(res.BirthDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "Data de Nascimento",
                        Message = "Data de Nascimento inválida"
                    });
                }
                else
                {
                    //DateTime.TryParseExact(res.BirthDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt);
                    //check if less than 18 years old
                    if (dt.AddYears(18) > DateTime.Now)
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Data de Nascimento",
                            Message = "Idade deve ser maior que 18 anos"
                        });
                    }
                    else if (dt.AddYears(65) < DateTime.Now)
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Data de Nascimento",
                            Message = "Idade deve ser menor que 65 anos"
                        });
                    }
                }
                //Validação Data de Admissão
                if (String.IsNullOrEmpty(res.AdmissionDate))
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "Data de Admissão",
                        Message = "Data de Admissão não pode ser vazia"
                    });
                } 
                else 
                if (!String.IsNullOrEmpty(res.AdmissionDate))
                {
                    if (res.AdmissionDate.Length != 10)
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Data de Admissão",
                            Message = "Data de Admissão deve conter 10 dígitos no formato DD/MM/AAAA"
                        });
                    }
                    else if (!DateTime.TryParseExact(res.AdmissionDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Data de Admissão",
                            Message = "Data de Admissão inválida"
                        });
                    }
                }

                //Validação Nome da Mãe
                if (String.IsNullOrEmpty(res.MothersName))
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "Nome da mãe",
                        Message = "Nome da mãe não pode ser vazio"
                    });
                }
                else if (res.MothersName.Any(c => !char.IsLetter(c) && !char.IsWhiteSpace(c)))
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "Nome da mãe",
                        Message = "Nome da mãe deve conter apenas letras"
                    });
                }
                else if (res.MothersName.Length < 3)
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "Nome da mãe",
                        Message = "Nome da mãe deve conter pelo menos 3 caracteres"
                    });
                }
                //Validação Celular
                if (!String.IsNullOrEmpty(res.PhoneNumber))
                {
                    if (res.PhoneNumber.Any(c => !char.IsDigit(c)))
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Celular",
                            Message = "Celular deve conter apenas dígitos"
                        });
                    }
                    else if (res.PhoneNumber.Length != 11)
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Celular",
                            Message = "Celular deve conter 11 dígitos"
                        });
                    }
                }
                //Validação Email
                if (!String.IsNullOrEmpty(res.Email))
                {
                    if (!res.Email.Contains("@") || !res.Email.Contains(".") || res.Email.Length < 5 || res.Email.Any(c => !char.IsLetter(c) && !char.IsDigit(c) && c != '@' && c != '.' && c != '_' && c != '-'))
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Email",
                            Message = "Email inválido"
                        });
                    }
                }
                //Validação CNPJ
                if (String.IsNullOrEmpty(res.Cnpj))
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "CNPJ",
                        Message = "CNPJ não pode ser vazio"
                    });
                }
                else if (res.Cnpj.Length != 18 || res.Cnpj.Replace(".", "").Replace("/", "").Replace("-", "").Length != 14)
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "CNPJ",
                        Message = "CNPJ deve conter 18 caracteres no formato 00.000.000/0000-00"
                    });
                }
                else if (!Cnpj.Validate(res.Cnpj))
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "CNPJ",
                        Message = "CNPJ inválido"
                    });
                }
                else
                {
                    res.Cnpj = res.Cnpj.Replace(".", "").Replace("/", "").Replace("-", "");
                    var existing = await _shopRepository.GetById(convenio);
                    if (existing == null || existing.Cnpj != res.Cnpj)
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "CNPJ",
                            Message = "CNPJ do convênio não confere"
                        });
                    }

                }
                Branch branch = null;
                var branches = await _branchRepository.GetAllByConvenio(convenio);
                if (branches.Count > 0)
                {
                    //Validação Codigo Filial
                    if (String.IsNullOrEmpty(res.BranchCode))
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Codigo Filial",
                            Message = "Codigo Filial deve ser informado"
                        });
                    }
                    else if (!res.BranchCode.Any(c => char.IsDigit(c)))
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Codigo Filial",
                            Message = "Codigo Filial deve conter pelo menos um número"
                        });
                    }
                    else if (res.BranchCode.Length < 1 || res.BranchCode.Length > 16)
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Codigo Filial",
                            Message = "Codigo Filial deve ter entre 1 e 16 caracteres"
                        });
                    }
                    else
                    {
                        var existing = branches.Any(b => b.Code == res.BranchCode);
                        if (!existing)
                        {
                            accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                            {
                                LineNumber = lineNumber,
                                Fatal = true,
                                Field = "Codigo Filial",
                                Message = "Codigo Filial inexistente no convênio"
                            });
                        }
                        else
                        {
                            branch = branches.Where(b => b.Code == res.BranchCode).FirstOrDefault();
                        }
                    }
                    //Validacao Nome Filial
                    if (!String.IsNullOrEmpty(res.BranchName))
                    {
                        if (res.BranchName.Any(c => !char.IsLetter(c) && !char.IsWhiteSpace(c) && !char.IsDigit(c)))
                        {
                            accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                            {
                                LineNumber = lineNumber,
                                Fatal = true,
                                Field = "Nome Filial",
                                Message = "Nome Filial deve conter apenas letras e números"
                            });
                        }
                        else if (res.BranchName.Length < 1 || res.BranchName.Length > 65)
                        {
                            accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                            {
                                LineNumber = lineNumber,
                                Fatal = true,
                                Field = "Nome Filial",
                                Message = "Nome Filial deve ter entre 1 e 65 caracteres"
                            });
                        }
                    }
                    var AllCostCenters = await _costCenterRepository.GetAllByConvenio(convenio);
                    if (AllCostCenters.Count > 0 && !String.IsNullOrEmpty(res.BranchCode) && branch != null)
                    {
                        var costCenters = AllCostCenters.FindAll(c => c.BranchId == branch.Id);
                        if (costCenters.Count > 0)
                        {
                            //Validação Codigo Centro de Custo
                            if (String.IsNullOrEmpty(res.CostCenterCode))
                            {
                                accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                                {
                                    LineNumber = lineNumber,
                                    Fatal = true,
                                    Field = "Codigo Centro de Custo",
                                    Message = "Codigo Centro de Custo deve ser informado"
                                });
                            }
                            else if (!res.CostCenterCode.Any(c => char.IsDigit(c)))
                            {
                                accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                                {
                                    LineNumber = lineNumber,
                                    Fatal = true,
                                    Field = "Codigo Centro de Custo",
                                    Message = "Codigo Centro de Custo deve conter pelo menos um número"
                                });
                            }
                            else if (res.CostCenterCode.Length < 1 || res.CostCenterCode.Length > 16)
                            {
                                accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                                {
                                    LineNumber = lineNumber,
                                    Fatal = true,
                                    Field = "Codigo Centro de Custo",
                                    Message = "Codigo Centro de Custo deve ter entre 1 e 16 caracteres"
                                });
                            }
                            else
                            {
                                var existing = costCenters.Any(c => c.InternalCode == res.CostCenterCode);
                                if (!existing)
                                {
                                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                                    {
                                        LineNumber = lineNumber,
                                        Fatal = true,
                                        Field = "Codigo Centro de Custo",
                                        Message = "Codigo Centro de Custo inexistente no convênio"
                                    });
                                }
                            }
                            //Validação Nome Centro de Custo
                            if (!String.IsNullOrEmpty(res.CostCenterName))
                            {
                                if (res.CostCenterName.Any(c => !char.IsLetter(c) && !char.IsWhiteSpace(c) && !char.IsDigit(c)))
                                {
                                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                                    {
                                        LineNumber = lineNumber,
                                        Fatal = true,
                                        Field = "Nome Centro de Custo",
                                        Message = "Nome Centro de Custo deve conter apenas letras e números"
                                    });
                                }
                                else if (res.CostCenterName.Length < 1 || res.CostCenterName.Length > 40)
                                {
                                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                                    {
                                        LineNumber = lineNumber,
                                        Fatal = true,
                                        Field = "Nome Centro de Custo",
                                        Message = "Nome Centro de Custo deve ter entre 1 e 40 caracteres"
                                    });
                                }
                            }
                        }
                    }
                }
                //Validação CEP
                //if (String.IsNullOrEmpty(res.ZipCode))
                //{
                //    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                //    {
                //        LineNumber = lineNumber,
                //        Fatal = true,
                //        Field = "CEP",
                //        Message = "CEP deve ser informado"
                //    });
                //}
                if (!String.IsNullOrEmpty(res.ZipCode))
                {
                    if (res.ZipCode.Any(c => !char.IsDigit(c) && c != '-'))
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "CEP",
                            Message = "CEP deve conter apenas números e hífen"
                        });
                    }
                    else if (res.ZipCode.Length < 8 || res.ZipCode.Length > 9)
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "CEP",
                            Message = "CEP deve ter 8 caracteres (apenas números) ou 9 (com hífen)"
                        });
                    }
                    else
                    {
                        var cep = await CepIntegration.ConsultaCep(res.ZipCode);
                        if (cep.result.resultado == "0")
                        {
                            accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                            {
                                LineNumber = lineNumber,
                                Fatal = true,
                                Field = "CEP",
                                Message = "CEP não encontrado"
                            });
                        }
                        else if (cep.result.uf != res.State)
                        {
                            accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                            {
                                LineNumber = lineNumber,
                                Fatal = true,
                                Field = "CEP",
                                Message = "CEP não pertence ao estado informado"
                            });
                        }
                    }
                }
                //Validação Logradouro
                //if (String.IsNullOrEmpty(res.Street))
                //{
                //    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                //    {
                //        LineNumber = lineNumber,
                //        Fatal = true,
                //        Field = "Endereco",
                //        Message = "Endereco deve ser informado"
                //    });
                //}
                if (!String.IsNullOrEmpty(res.Street))
                {
                    if (res.Street.Any(c => !char.IsLetter(c) && !char.IsWhiteSpace(c) && !char.IsDigit(c) && c != '.' && c != ','))
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Endereco",
                            Message = "Endereco deve conter apenas letras, números e espaços"
                        });
                    }
                    else if (res.Street.Length < 1 || res.Street.Length > 80)
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Endereco",
                            Message = "Endereco deve ter entre 1 e 80 caracteres"
                        });
                    }
                }
                //Validação Numero
                //if (String.IsNullOrEmpty(res.AddressNumber))
                //{
                //    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                //    {
                //        LineNumber = lineNumber,
                //        Fatal = true,
                //        Field = "Numero",
                //        Message = "Numero deve ser informado"
                //    });
                //}
                if (!String.IsNullOrEmpty(res.AddressNumber))
                {
                    if (res.AddressNumber.Any(c => !char.IsDigit(c) && !char.IsWhiteSpace(c) && !char.IsLetter(c)))
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Numero",
                            Message = "Numero deve conter apenas letras e números"
                        });
                    }
                    else if (res.AddressNumber.Length < 1 || res.AddressNumber.Length > 10)
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Numero",
                            Message = "Numero deve ter entre 1 e 10 caracteres"
                        });
                    }
                }
                //Validação Complemento
                if (!String.IsNullOrEmpty(res.AddressComplement))
                {
                    if (res.AddressComplement.Any(c => !char.IsLetter(c) && !char.IsWhiteSpace(c) && !char.IsDigit(c) && c != '.' && c != ','))
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Complemento",
                            Message = "Complemento deve conter apenas letras, números e espaços"
                        });
                    }
                    else if (res.AddressComplement.Length < 1 || res.AddressComplement.Length > 24)
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Complemento",
                            Message = "Complemento deve ter entre 1 e 24 caracteres"
                        });
                    }
                }
                //Validação Bairro
                //if (String.IsNullOrEmpty(res.Neighborhood))
                //{
                //    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                //    {
                //        LineNumber = lineNumber,
                //        Fatal = true,
                //        Field = "Bairro",
                //        Message = "Bairro deve ser informado"
                //    });
                //}
                if (!String.IsNullOrEmpty(res.Neighborhood))
                {
                    if (res.Neighborhood.Any(c => !char.IsLetter(c) && !char.IsWhiteSpace(c) && !char.IsDigit(c) && c != '.' && c != ','))
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Bairro",
                            Message = "Bairro deve conter apenas letras, números e espaços"
                        });
                    }
                    else if (!res.Neighborhood.Any(c => char.IsLetter(c)))
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Bairro",
                            Message = "Bairro deve conter ao menos uma letra"
                        });
                    }
                    else if (res.Neighborhood.Length < 1 || res.Neighborhood.Length > 80)
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Bairro",
                            Message = "Bairro deve ter entre 1 e 80 caracteres"
                        });
                    }
                }
                //Validação Cidade
                //if (String.IsNullOrEmpty(res.CityName))
                //{
                //    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                //    {
                //        LineNumber = lineNumber,
                //        Fatal = true,
                //        Field = "Cidade",
                //        Message = "Cidade deve ser informada"
                //    });
                //}
                if (!String.IsNullOrEmpty(res.CityName))
                {
                    if (res.CityName.Any(c => !char.IsLetter(c) && !char.IsWhiteSpace(c)))
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Cidade",
                            Message = "Cidade deve conter apenas letras e espaços"
                        });
                    }
                    else if (res.CityName.Length < 1 || res.CityName.Length > 60)
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Cidade",
                            Message = "Cidade deve ter entre 1 e 60 caracteres"
                        });
                    }
                }
                //VAlidação UF
                var states = new List<string>() { "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO" };
                //if (String.IsNullOrEmpty(res.State))
                //{
                //    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                //    {
                //        LineNumber = lineNumber,
                //        Fatal = true,
                //        Field = "UF",
                //        Message = "UF deve ser informado"
                //    });
                //}
                if (!String.IsNullOrEmpty(res.State))
                {
                    if (res.State.Length != 2)
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "UF",
                            Message = "Aceita apenas 2 letras"
                        });
                    }
                    else if (res.State.Any(c => !char.IsLetter(c)))
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "UF",
                            Message = "Aceita apenas 2 letras"
                        });
                    } //Verificar se o estado é válido
                    else if (!states.Contains(res.State.ToUpper()))
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "UF",
                            Message = "Aceita apenas 2 letras"
                        });
                    }
                }
                //Validação Limite
                //if (String.IsNullOrEmpty(res.CardLimit))
                //{
                //    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                //    {
                //        LineNumber = lineNumber,
                //        Fatal = true,
                //        Field = "Limite",
                //        Message = "Limite deve ser informado"
                //    });
                //}
                //Verificar se CardLimit é menor que 0,01 ou maior que 10000,00
                if (!String.IsNullOrEmpty(res.CardLimit))
                {
                    if (!Double.TryParse(res.CardLimit, NumberStyles.Number, new CultureInfo("pt-BR"), out double vl))
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Limite",
                            Message = "Limite deve ser informado no formato 9.999,99"

                        });

                    }
                    else if (vl < 0.01 || vl > 10000)
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Limite",
                            Message = "Limite deve ser entre R$ 0,01 e R$ 10.000,00"
                        });

                    }
                }
                accountValidation.Success = !accountValidation.Occurrences.Any(o => o.Fatal);

                //else if (res.Name == null || res.Name == "")
                //{
                //    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                //    {
                //        RegistrationNumber = res.RegistrationNumber,
                //        Name = "Nome não pode ser vazio",
                //        Cpf = res.Cpf,
                //        Email = res.Email,
                //        PhoneNumber = res.PhoneNumber
                //    });
                //}
                //else if (res.Cpf == null || res.Cpf == "")
                //{
                //    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                //    {
                //        RegistrationNumber = res.RegistrationNumber,
                //        Name = res.Name,
                //        Cpf = "CPF não pode ser vazio",
                //        Email = res.Email,
                //        PhoneNumber = res.PhoneNumber
                //    });
                //}
                //else if (res.Email == null || res.Email == "")
                //{
                //    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                //    {
                //        RegistrationNumber = res.RegistrationNumber,
                //        Name = res.Name,
                //        Cpf = res.Cpf,
                //        Email = "Email não pode ser vazio",
                //        PhoneNumber = res.PhoneNumber
                //    });
                //}
                //else if (res.PhoneNumber == null || res.PhoneNumber == "")
                //{
                //    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                //    {
                //        RegistrationNumber = res.RegistrationNumber,
                //        Name = res.Name,
                //        Cpf = res.Cpf,
                //        Email = res.Email,
                //        PhoneNumber = "Telefone
                //    });
                //}
            }
            //return Accountvalidation
            return accountValidation;

        }

        public async Task Dismissal(AccountDismissalDto accounts)
        {
            var convenio = await _shopRepository.GetById(accounts.Convenio);
            var contas = new List<BulllaEmpresaCancelamentoContaRequest>();
            var filter = new List<AccountDismissal>();

            foreach (var account in accounts.Accounts)
            {
                var customer = await _accountRepository.GetByCpfAndConvenio(account.Cpf, accounts.Convenio);
                if (customer != null)
                {
                    var solicitation = new Solicitation()
                    {
                        SolicitationType = SolicitationTypeEnum.Dismissal,
                        UserId = accounts.CurrentUser.Id,
                        ShopId = accounts.Convenio,
                        ShopName = convenio.Name,
                        ShopDocument = convenio.Cnpj,
                        GroupId = convenio.IdGroup,
                        GroupName = convenio.GroupName,
                        ClientId = customer.Id,
                        Name = customer.Name,
                        Email = customer.Email,
                        Phone = customer.PhoneNumber,
                        Cpf = customer.Cpf,
                        PreviousLimit = customer.CardLimit,
                        NewLimit = customer.CardLimit,
                        AccountStatus = customer.Status,
                        CardStatus = customer.CardStatus,
                        BlockType = account.Reason,
                        Observation = "A solicitação foi realizada com sucesso!\nCancelamento será realizado em até 24 horas.\nInforme valor para fins rescisórios até 1 dia antes da data de corte e evite cobranças desnecessárias."
                    };
                    await _solicitationRepository.Create(solicitation);
                    if (solicitation.Id > 0)
                    {
                        contas.Add(new BulllaEmpresaCancelamentoContaRequest()
                        {
                            idConta = customer.Id,
                            idRegistro = solicitation.Id,
                            idTipoBloqueio = account.Reason,
                        });

                        filter.Add(new AccountDismissal()
                        {
                            Convenio = accounts.Convenio,
                            Cpf = account.Cpf,
                            UserId = accounts.UserId.Value,
                            SolicitationId = solicitation.Id
                        });
                    }
                }
            }

            _bulllaEmpresaIntegration.CancelamentoConta(accounts.Convenio, accounts.CurrentUser.Email, contas);
            
            //Timer para aguardar a integração do registro de bloqueio na tabela T_BLOQUEIOCLIENTE
            Thread.Sleep(3000);

            foreach (var row in filter)
            {
                var status = await _accountRepository.GetStatusDismissal(row.Cpf.ToString());
                if(status != null)
                {
                    var solicitation = new Solicitation()
                    {
                        Id = row.SolicitationId,
                        Observation = status.Observation,
                        BlockTypeId = status.IdTypeBlock,
                        CardStatus = status.CardStatus
                    };
                    
                    _ = _solicitationRepository.RegisterSuccess(row.SolicitationId, solicitation);
                }
            }

            await NotificationRescision (filter);
            await EmailRescision (filter);        

    }

        public async Task<string> NotificationRescision(List<AccountDismissal> filter)
        {
            int i = 0;
            bool valid = false;

            while(valid is false)
            {
                var searchSolicitation = new List<SolicitationFilterDto>
                {
                    new SolicitationFilterDto()
                    {
                        SearchTerm = filter[i].Cpf,
                        SolicitationType = 8,
                        SolicitationStatus = "",
                        SolicitationStartDate = null,
                        SolicitationEndDate = null
                    }
                };
                
                var solicitation = _solicitationRepository.GetAllPaged(searchSolicitation[0], 5, 0, "id%5desc");
                 
                if(solicitation.Result.Data != null)
                {   
                    for(int y = 0; y < solicitation.Result.Data.Count(); y++) 
                    {                
                        if(solicitation.Result.Data[y].Observation.Contains("[CLIENTE BLOQUEADO] - Cliente cancelado por Desligamento/Encerramento"))
                        {
                            if(valid is true)
                                break;
                                
                            dynamic returnAccounts = await ContasDesligadas(filter[i].Convenio, filter[i].Cpf, null,null, null, null, filter[i].UserId, null);
                            string convert = System.Text.Json.JsonSerializer.Serialize(returnAccounts);
                            
                            if(convert.Contains(filter[i].Cpf))
                            {
                                valid = true;

                                await SendNotification (filter);
                                await EmailRescisionD5(filter); 
                            }    
                        }                       
                    }
                }

                i++;

                if(i > filter.Count()-1)  
                    break;
            }

            return "Process Finish";
        }

        public async Task<string> SendNotification(List<AccountDismissal> filter )
        {
            var menu = "Rescisão do colaborador";
            var users = await _userService.GetAllbyMenu(menu);
            var group = await _shopRepository.GetAllByShop(filter[0].Convenio);
            var userNotification = users.Where(str => str.GroupId == group[0].IdGroup).ToList();

            for(int i = 0; i < userNotification.Count(); i++)
            {
                var notification = new NotificationAddDto()
                {
                    Title = "Rescisões Pendentes",
                    Description = "Evite cobranças desnecessárias, informe agora o valor Bullla descontado na rescisão do(s) colaborador(es) demitido(s).\n Acesse o menu Rescisão do Colaborador e consulte a lista de contas canceladas.",
                    Destination = "user",
                    GroupId = 0,
                    ShopId = 0,
                    UserId = userNotification[i].Id,
                    NotificationType = Portal.API.Enum.NotificationTypeEnum.RescisaoColaborador
                };

                    await _notificationService.Create(notification);
            }

            return "Process Finish";
        }

        public async Task<string> EmailRescision(List<AccountDismissal> filter, bool immediateTemplateD5 = false, bool jobSender = false)
        {
           var lotSuccess = new List<DismissalStatus>(); 
           var lotError = new List<DismissalStatus>(); 
           var urlBase = $"{environmentsBase.BaseUrl}";

            var menu = "Rescisão do colaborador";
            var users = await _userService.GetAllbyMenu(menu);
            var group = await _shopRepository.GetAllByShop(filter[0].Convenio);
            var userNotification = users.Where(str => str.GroupId == group[0].IdGroup).ToList();

            if(immediateTemplateD5 is false)
            {
                for(int i = 0; i < filter.Count(); i++)
                {
                    var searchSolicitation = new List<SolicitationFilterDto>
                    {
                        new SolicitationFilterDto()
                        {
                            SearchTerm = filter[i].Cpf,
                            SolicitationType = 8,
                            SolicitationStatus = "",
                            SolicitationStartDate = null,
                            SolicitationEndDate = null
                        }
                    };
                        
                    var solicitation = _solicitationRepository.GetAllPaged(searchSolicitation[0], 5, 0, "id%5desc");

                    if(solicitation.Result.Data != null)
                    {
                        for(int y = 0; y < solicitation.Result.Data.Count(); y++) 
                        {                
                            if(solicitation.Result.Data[y].Observation.Contains("[CLIENTE BLOQUEADO] - Cliente cancelado por Desligamento/Encerramento"))
                            {        
                                dynamic returnAccounts = ContasDesligadas(filter[i].Convenio, filter[i].Cpf, null,null, null, null, filter[i].UserId, null);
                                string convert = System.Text.Json.JsonSerializer.Serialize(returnAccounts);
                                
                                if(convert.Contains(filter[i].Cpf))
                                {
                                    lotSuccess.Add(new DismissalStatus()
                                    {
                                        Convenio = filter[i].Convenio,
                                        Cpf = filter[i].Cpf,
                                        UserId = filter[i].UserId
                                    });
                                }                               
                            }
                            else
                            {
                                lotError.Add(new DismissalStatus()
                                {
                                    Convenio = filter[i].Convenio,
                                    Cpf = filter[i].Cpf,
                                    UserId = filter[i].UserId
                                });
                            }
                        }

                    }
                }
            }
            else
            {
                var subject = "Atenção: Rescisão de Colaborador";

                var html = $@"<div align=""middle""><img src=""https://cdn-icons-png.flaticon.com/128/1680/1680012.png"" height=""62""
                    width=""62""><h2 style=""color: #868686"">Atenção!</h2></div></br><p style=""color: #7D7D7D"">Prezado cliente,</p>
                    <p style=""color: #7D7D7D"">Evite cobranças desnecessários para o seu convênio. Informe agora mesmo o valor Bullla 
                    descontado na rescisão do(s) colaborador(es) demitido(s).</p><p style=""color: #7D7D7D"">Caso o valor descontado não 
                    seja informado em até dois dias antes do corte de faturamento, 100% da dívida remanescente do colaborador será repassada 
                    ao convênio.</p><p style=""color: #7D7D7D"">Em caso de dúvidas entre em contato com nosso <b>suporte ao cliente</b>. 
                    <div align=""middle""><a href=""{urlBase}/?origin=/dismissal/simulation"" style=""color: #0786c1""><h3>Informar Valor 
                    Descontado em Rescisão</h3></a><br><h5 style=""color: #7D7D7D"">Caso já tenha informado o valor do desconto, favor 
                    desconsidere essa mensagem.</h5></div>";
                    
                var text = $@"Atenção! Prezado cliente, Evite cobranças desnecessários para o seu convênio. informe agora mesmo o valor Bullla 
                    descontado na rescisão do(s) colaborador(es) demitido(s). Caso o valor descontado não 
                    seja informado em até dois dias antes do corte de faturamento, 100% da dívida remanescente do colaborador será repassada 
                    ao convênio.Em caso de dúvidas entre em contato com nosso suporte ao cliente. Informar Valor 
                    Descontado em Rescisão Caso já tenha informado o valor do desconto, favor desconsidere essa mensagem.";

             
                if(jobSender)
                    await SendEmail (subject, html, text, filter[0].Convenio, jobSender);


                await SendEmail (subject, html, text, filter[0].Convenio);


            }

            if(lotSuccess.Count() > 0 && lotError.Count() == 0)
            {
                var qndtDismissal = "";
                if(filter.Count() > 1)
                {
                    qndtDismissal = "Quantidade de desligamentos efetivados: " + lotSuccess.Count();
                }
                var subject = "Atenção: Conta e cartão Bullla Cancelado(s)";

                var html = $@"<div align=""middle""><img src=""https://cdn-icons-png.flaticon.com/128/190/190411.png"" height=""62"" 
                    width=""62""><h2 style=""color: #868686"">Cancelamento de conta(s) realizado com sucesso!</h2></div>  <h4 style=""color: #868686"">Data da solicitação: {DateTime.Now.ToString("dd/MM/yyyy")}</br>
                    Código convênio: {group[0].Id}</br>Nome do convênio: {group[0].Name}</br>{ qndtDismissal }</h4><p style=""color: #7D7D7D"">Evite cobranças desnecessários para o seu convênio. 
                    Acesse o portal e informe agora mesmo o valor Bullla a ser descontado na rescisão do(s) colaborador(es) demitido(s).</p>
                    <p style=""color: #7D7D7D"">Caso o valor descontado não seja informado em até dois dias antes do corte de faturamento, 100% da dívida remanescente
                    do colaborador será repassada ao convênio.</p><p style=""color: #7D7D7D"">Em caso de dúvidas entre em contato com nosso <b>suporte ao cliente</b>.</p></br>
                    <div align=""middle""> <a href=""{urlBase}/?origin=/dismissal/simulation"" style=""color: #007eb6""><h3 style=""color: #0786c1"">Informar Valor Descontado em Rescisão</h3></a></br><h5 style=""color: #7D7D7D"">Caso já tenha informado o valor do desconto, favor desconsidere essa mensagem.</h5></div>";
                    
                var text = $@"Cancelamento de conta(s) realizado com sucesso! Data da solicitação: {DateTime.Now}
                    Código convênio: {group[0].Id} Nome do convênio: {group[0].Name} Evite cobranças desnecessários para o seu convênio. 
                    Acesse o portal e informe agora mesmo o valor Bullla a ser descontado na rescisão do(s) colaborador(es) demitido(s).
                    Caso o valor descontado não seja informado em até dois dias antes do corte de faturamento, 100% da dívida remanescente
                    do colaborador será repassada ao convênio. Em caso de dúvidas entre em contato com nosso suporte ao cliente.
                    Caso já tenha informado o valor do desconto, favor desconsidere essa mensagem.";
             
                await SendEmail (subject, html, text, lotSuccess[0].Convenio);

            }
            else if(lotError.Count() > 0 && filter.Count() > 1)
            {
                var subject = "Não foi possível cancelar a conta(as) e cartão(ões)";

                var html = $@"<div align=""middle""><img src=""https://cdn-icons-png.flaticon.com/128/1680/1680012.png"" height=""62""
                    width=""62""><h2 style=""color: #868686"">Atenção!</h2></div></br><h4 style=""color: #868686"">Data da solicitação: {DateTime.Now.ToString("dd/MM/yyyy")}</br>Código convênio: {group[0].Id}</br>Nome do convênio: {group[0].Name}
                    </br>Quantidade de desligamentos efetivados: {lotSuccess.Count()}</h4><p style=""color: #7D7D7D"">Não foi possível completar a requisição de alguns desligamentos,
                    acesse o <a href=""{urlBase}/?origin=/solicitations/list"" style=""color: #0786c1""><b  style=""color: #0786c1"">Histórico de Solicitações</b> </a> para analisar as ocorrências.</p><p style=""color: #7D7D7D"">Evite cobranças desnecessários para o seu 
                    convênio. Acesse o portal e informe agora mesmo o valor Bullla a ser descontado na rescisão do(s) colaborador(es) demitido(s).
                    </p><p style=""color: #7D7D7D"">Caso o valor descontado não seja informado em até dois dias antes do corte de faturamento, 100% da dívida remanescente 
                    do colaborador será repassada ao convênio.</p><p style=""color: #7D7D7D"">Em caso de dúvidas entre em contato com nosso <b>suporte ao cliente</b>.</p>
                    <div align=""middle""><a href=""{urlBase}/?origin=/dismissal/simulation"" style=""color: #0786c1""><h3>Informar Valor Descontado em Rescisão</h3></a><br><h5 style=""color: #7D7D7D"">Caso já tenha informado o valor do desconto, favor 
                    desconsidere essa mensagem.</h5></div>";
                    
                var text = $@"Atenção! Data da solicitação: {DateTime.Now} Código convênio: {group[0].Id} Nome do convênio: {group[0].Name}
                    Quantidade de desligamentos efetivados: {lotSuccess.Count()} Não foi possível completar a requisição de alguns desligamentos,
                    acesse Histórico de Solicitações para analisar as ocorrências. Evite cobranças desnecessários para o seu 
                    convênio. Acesse o portal e informe agora mesmo o valor Bullla a ser descontado na rescisão do(s) colaborador(es) demitido(s).
                    Caso o valor descontado não seja informado em até dois dias antes do corte de faturamento, 100% da dívida remanescente 
                    do colaborador será repassada ao convênio.Em caso de dúvidas entre em contato com nosso suporte ao cliente.
                    Informar Valor Descontado em Rescisão Caso já tenha informado o valor do desconto, favor 
                    desconsidere essa mensagem.";

                
                await SendEmail (subject, html, text, lotError[0].Convenio);


            }
            else if(lotError.Count() > 0 && filter.Count() == 1)
            {
                var subject = "Não foi possível cancelar a conta(as) e cartão(ões)";

                var html = $@"<div align=""middle""><img src=""https://cdn-icons-png.flaticon.com/128/1680/1680012.png"" height=""62"" 
                    width=""62""><h2 style=""color: #868686"">Atenção!</h2></br></div><h4 style=""color: #868686"">Data da solicitação: {DateTime.Now.ToString("dd/MM/yyyy")}</br>Código convênio: {group[0].Id}</br>Nome do convênio: {group[0].Name}
                    </h4><p style=""color: #7D7D7D"">Não foi possível completar a requisição do desligamento, acesse o <a href=""{urlBase}/?origin=/solicitations/list"" style=""color: #0786c1""><b  style=""color: #0786c1"">Histórico de Solicitações</b> </a> para analisar
                    as ocorrências.</p><p style=""color: #7D7D7D"">Em caso de dúvidas entre em contato com nosso <b>suporte ao cliente</b>.</p><div align=""middle"">
                    </br><h5 style=""color: #7D7D7D"">Caso já tenha informado o valor do desconto, favor desconsidere essa mensagem.</h5></div>";
 
                var text = $@"Atenção! Data da solicitação: {DateTime.Now}  Código convênio: {group[0].Id}  Nome do convênio: {group[0].Name}
                    Não foi possível completar a requisição do desligamento, acesse o Histórico de Solicitações para analisar
                    as ocorrências. Em caso de dúvidas entre em contato com nosso suporte ao cliente. Caso já tenha informado o valor do 
                    desconto, favor desconsidere essa mensagem.";

                await SendEmail (subject, html, text, lotError[0].Convenio);

            }

            return "Process Finish";
        }

        public async Task<string> SendEmail(string subject, string html, string text, long convenio, bool job = false)
        {
            var menu = "Rescisão do colaborador";
            var users = await _userService.GetAllbyMenu(menu);
            var group = await _shopRepository.GetAllByShop(convenio);
            var userNotification = users.Where(str => str.GroupId == group[0].IdGroup).ToList();

            for(int i = 0; i < userNotification.Count(); i++)
            {
                var message = new SendEmailMessageDto()
                {
                    from = "atendimento@bullla.com.br",
                    to = userNotification[i].Email,
                    subject = subject,
                    html = html,
                    text = text
                };

                if(job)
                    await Task.Run(() => rabbitService.Publish("JobSendEmailRescission", JsonConvert.SerializeObject(message)));


                await Task.Run(() => rabbitService.Publish("send-email", JsonConvert.SerializeObject(message)));
            }

            return "Process Finish";
        }


        public async Task<bool> EmailRescisionD5(List<AccountDismissal> filter)
        {
                var shops = await _shopRepository.GetAllByShop(filter[0].Convenio);

                foreach (var row in shops)
                {   
                    if(row.ClosingDay > 0 && row.ClosingDay <= 31)
                    {
                        var between = BetweenDates(row.ClosingDay);
                        if(between.Situation)
                        {
                            await EmailRescision (filter, true);
                            return true;
                        }
                        
                    }
                    return false;
                }
                return false;  
        }



        public async Task<AccountValidationDto> ValidateFileDismissal(FileUploadDto fileUpload, long convenio)
        {
            var accountValidation = new AccountValidationDto();
            var shop = await _shopRepository.GetById(convenio);
            //read fileUpload as csv
            List<AccountDismissalCsvDto> result;
            using (var reader = new StreamReader(fileUpload.FormFile.OpenReadStream()))
            {
                var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
                config.HasHeaderRecord = true;
                config.Delimiter = ";";
                config.BadDataFound = null;
                config.IgnoreBlankLines = true;
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Context.RegisterClassMap<CsvMapperAccountDismissalDto>();
                    result = csv.GetRecords<AccountDismissalCsvDto>().ToList();
                }
            }

            var accounts = new List<AccountDto>();
            //validate each record
            var lineNumber = 0;
            foreach (var res in result)
            {
                lineNumber++;
                AccountDto customer = null;
                //add occurrence to Accountvalidation
                //Validação Nome
                if (String.IsNullOrEmpty(res.Name))
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "Nome",
                        Message = "Nome não pode ser vazio"
                    });
                }
                else if (res.Name.Any(c => !char.IsLetter(c) && !char.IsWhiteSpace(c)))
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "Nome",
                        Message = "Nome deve conter apenas letras"
                    });
                }
                else if (res.Name.Length < 3)
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "Nome",
                        Message = "Nome deve conter pelo menos 3 caracteres"
                    });
                }
                //Validação CPF
                if (String.IsNullOrEmpty(res.Cpf))
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "CPF",
                        Message = "CPF não pode ser vazio"
                    });
                }
                else if (res.Cpf.Length != 14 || res.Cpf.Replace(".", "").Replace("-", "").Length != 11)
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "CPF",
                        Message = "CPF deve conter 14 caracteres no formato 000.000.000-00"
                    });
                }
                else if (!Cpf.Validate(res.Cpf))
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "CPF",
                        Message = "CPF inválido"
                    });
                }
                else
                {
                    res.Cpf = res.Cpf.Replace(".", "").Replace("-", "");
                    customer = await this.GetByCpfAndConvenio(res.Cpf, convenio);
                    if (customer == null)
                    {
                        var customerGroup = await this.GetByCpfAndGrupo(res.Cpf, shop.IdGroup);
                        if (customerGroup == null)
                        {
                            accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                            {
                                LineNumber = lineNumber,
                                Fatal = true,
                                Field = "CPF",
                                Message = "CPF inexistente"
                            });
                        }
                        else
                        {
                            accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                            {
                                LineNumber = lineNumber,
                                Fatal = true,
                                Field = "CPF",
                                Message = "CPF não pertence ao convênio selecionado"
                            });
                        }
                    }

                }

                if (String.IsNullOrEmpty(res.ReasonDescription))
                {
                    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                    {
                        LineNumber = lineNumber,
                        Fatal = true,
                        Field = "Tipo de Desligamento",
                        Message = "Tipo de Desligamento não pode ser vazio"
                    });
                }
                else
                {
                    if (res.ReasonDescription.ToLowerInvariant() == "voluntário" ||
                        res.ReasonDescription.ToLowerInvariant() == "voluntario" ||
                        res.ReasonDescription.ToLowerInvariant() == "desligamento_voluntario" ||
                        res.ReasonDescription.ToLowerInvariant().StartsWith("volunt")
                        )
                    {
                        res.ReasonDescription = "DESLIGAMENTO_VOLUNTARIO";
                    }
                    else if (res.ReasonDescription.ToLowerInvariant() == "involuntário" ||
                        res.ReasonDescription.ToLowerInvariant() == "involuntario" ||
                        res.ReasonDescription.ToLowerInvariant() == "desligamento_involuntario" ||
                        res.ReasonDescription.ToLowerInvariant() == "desligamento" ||
                        res.ReasonDescription.ToLowerInvariant().StartsWith("involunt")
                        )
                    {
                        res.ReasonDescription = "DESLIGAMENTO";
                    }
                    if (res.ReasonDescription != "DESLIGAMENTO" && res.ReasonDescription != "DESLIGAMENTO_VOLUNTARIO")
                    {
                        accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                        {
                            LineNumber = lineNumber,
                            Fatal = true,
                            Field = "Tipo de Desligamento",
                            Message = "Tipo de Desligamento inválido. Valores permitidos: Voluntário, Involuntário"
                        });
                    }
                }
                accountValidation.Success = !accountValidation.Occurrences.Any(o => o.Fatal);

                if (!accountValidation.Occurrences.Any(o => o.Fatal && o.LineNumber == lineNumber) && customer != null)
                {
                    customer.Reason = res.ReasonDescription;
                    accountValidation.Accounts.Add(customer);
                }

                //else if (res.Name == null || res.Name == "")
                //{
                //    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                //    {
                //        RegistrationNumber = res.RegistrationNumber,
                //        Name = "Nome não pode ser vazio",
                //        Cpf = res.Cpf,
                //        Email = res.Email,
                //        PhoneNumber = res.PhoneNumber
                //    });
                //}
                //else if (res.Cpf == null || res.Cpf == "")
                //{
                //    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                //    {
                //        RegistrationNumber = res.RegistrationNumber,
                //        Name = res.Name,
                //        Cpf = "CPF não pode ser vazio",
                //        Email = res.Email,
                //        PhoneNumber = res.PhoneNumber
                //    });
                //}
                //else if (res.Email == null || res.Email == "")
                //{
                //    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                //    {
                //        RegistrationNumber = res.RegistrationNumber,
                //        Name = res.Name,
                //        Cpf = res.Cpf,
                //        Email = "Email não pode ser vazio",
                //        PhoneNumber = res.PhoneNumber
                //    });
                //}
                //else if (res.PhoneNumber == null || res.PhoneNumber == "")
                //{
                //    accountValidation.Occurrences.Add(new AccountOccurrenceDto()
                //    {
                //        RegistrationNumber = res.RegistrationNumber,
                //        Name = res.Name,
                //        Cpf = res.Cpf,
                //        Email = res.Email,
                //        PhoneNumber = "Telefone
                //    });
                //}
            }
            //return Accountvalidation
            return accountValidation;
        }

        public async Task ImportFileDismissal(FileUploadDto fileUpload, long convenioId, UserDtoAccounts user)
        {
            var convenio = await _shopRepository.GetById(convenioId);
             var filter = new List<AccountDismissal>();
            
            List<AccountDismissalCsvDto> result;

            using (var reader = new StreamReader(fileUpload.FormFile.OpenReadStream()))
            {
                var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
                config.HasHeaderRecord = true;
                config.Delimiter = ";";
                config.BadDataFound = null;
                config.IgnoreBlankLines = true;
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Context.RegisterClassMap<CsvMapperAccountDismissalDto>();
                    result = csv.GetRecords<AccountDismissalCsvDto>().ToList();
                }
            }

            var contas = new List<BulllaEmpresaCancelamentoContaRequest>();
            foreach (var account in result)
            {
                if (account.ReasonDescription.ToLowerInvariant() == "voluntário" ||
                    account.ReasonDescription.ToLowerInvariant() == "voluntario" ||
                    account.ReasonDescription.ToLowerInvariant() == "desligamento_voluntario" ||
                    account.ReasonDescription.ToLowerInvariant().StartsWith("volunt")
                    )
                {
                    account.ReasonDescription = "DESLIGAMENTO_VOLUNTARIO";
                }
                else if (account.ReasonDescription.ToLowerInvariant() == "involuntário" ||
                    account.ReasonDescription.ToLowerInvariant() == "involuntario" ||
                    account.ReasonDescription.ToLowerInvariant() == "desligamento_involuntario" ||
                    account.ReasonDescription.ToLowerInvariant() == "desligamento" ||
                    account.ReasonDescription.ToLowerInvariant().StartsWith("involunt")
                    )
                {
                    account.ReasonDescription = "DESLIGAMENTO";
                }
                var customer = await _accountRepository.GetByCpfAndConvenio(account.Cpf, convenioId);
                if (customer != null)
                {
                    var solicitation = new Solicitation()
                    {
                        SolicitationType = SolicitationTypeEnum.Dismissal,
                        UserId = user.Id,
                        ShopId = convenioId,
                        ShopName = convenio.Name,
                        ShopDocument = convenio.Cnpj,
                        GroupId = convenio.IdGroup,
                        GroupName = convenio.GroupName,
                        ClientId = customer.Id,
                        Name = customer.Name,
                        Email = customer.Email,
                        Phone = customer.PhoneNumber,
                        Cpf = customer.Cpf.Replace(".", "").Replace("-", ""),
                        PreviousLimit = customer.CardLimit,
                        NewLimit = customer.CardLimit,
                        AccountStatus = customer.Status,
                        CardStatus = customer.CardStatus,
                        BlockType = account.ReasonDescription,
                        Observation = "A solicitação foi realizada com sucesso!\nCancelamento será realizado em até 24 horas.\nInforme valor para fins rescisórios até 1 dia antes da data de corte e evite cobranças desnecessárias."
                    };
                    await _solicitationRepository.Create(solicitation);
                    if (solicitation.Id > 0)
                    {
                        contas.Add(new BulllaEmpresaCancelamentoContaRequest()
                        {
                            idConta = customer.Id,
                            idRegistro = solicitation.Id,
                            idTipoBloqueio = account.ReasonDescription
                        });
                    }

                        filter.Add(new AccountDismissal()
                        {
                            Convenio = convenioId,
                            Cpf = account.Cpf,
                            UserId = user.Id,
                            SolicitationId = solicitation.Id
                        });
                }

            }

            _bulllaEmpresaIntegration.CancelamentoConta(convenioId, user.Email, contas);

            //Timer para aguardar a integração do registro de bloqueio na tabela T_BLOQUEIOCLIENTE
            Thread.Sleep(4000);

            for(int i = 0; i < filter.Count(); i++)
            {
                filter[i].Cpf = Regex.Replace(filter[i].Cpf.ToString(), "[^0-9]+", "");
                var status = await _accountRepository.GetStatusDismissal(filter[i].Cpf);
                if(status != null)
                {
                    var solicitation = new Solicitation()
                    {
                        Id = filter[i].SolicitationId,
                        Observation = status.Observation,
                        BlockTypeId = status.IdTypeBlock,
                        CardStatus = status.CardStatus
                    };
                    
                    _ = _solicitationRepository.RegisterSuccess(filter[i].SolicitationId, solicitation);
                }
            }

            await NotificationRescision (filter);
            await EmailRescision (filter);     
         
        }


        public async Task<string> SecurityUnblock(long convenioId, AccountModel data, int userId)
        {
            var convenio = await _shopRepository.GetById(convenioId);
            foreach (var account in data.accounts)
            {
                var customer = await _accountRepository.Get(account.idConta, convenioId);
                if (customer != null)
                {
                    var solicitation = new Solicitation()
                    {
                        SolicitationType = SolicitationTypeEnum.SecurityUnblock,
                        UserId = userId,
                        ShopId = convenioId,
                        ShopName = convenio.Name,
                        ShopDocument = convenio.Cnpj,
                        GroupId = convenio.IdGroup,
                        GroupName = convenio.GroupName,
                        ClientId = customer.Id,
                        Name = customer.Name,
                        Email = customer.Email,
                        Phone = customer.PhoneNumber,
                        Cpf = customer.Cpf,
                        PreviousLimit = customer.CardLimit,
                        NewLimit = customer.CardLimit,
                        AccountStatus = customer.Status,
                        CardStatus = customer.CardStatus,
                        Observation = "A solicitação foi realizada com sucesso! Seu colaborador já pode realizar o desbloqueio do cartão pelo aplicativo ou central de atendimento Bullla."
                    };
                    await _solicitationRepository.Create(solicitation);
                }

            }
            return _asIntegration.DesbloqueioDeSecuranca(convenioId.ToString(), userId, data);
        }

        private string ValidacaoFiltro(string cpfColaborador, DateTime? dataInicio, DateTime? dataFim, int? pagina, int? tamanhoPagina, int? statusRecisao)
        {
            var filtro = "";

            if (!String.IsNullOrEmpty(cpfColaborador))
            {
                filtro += "cpf=" + cpfColaborador.Replace(".", "").Replace("-", "") + "&"; 
            }

            if (dataInicio != null)
            {
                filtro += "dataInicio=" + Convert.ToDateTime(dataInicio).ToString("dd/MM/yyyy") + "&";
            }

            if (dataFim != null)
            {
                filtro += "dataFim=" + Convert.ToDateTime(dataFim).ToString("dd/MM/yyyy") + "&";
            }

            if (pagina != null)
            {
                filtro += "pagina=" + pagina + "&";
            }

            if (tamanhoPagina != null)
            {
                filtro += "tamanhoPagina=" + tamanhoPagina + "&";
            }

            if (statusRecisao != null)
            {
                filtro += "statusRecisao=" + statusRecisao + "&";
            }

            return filtro;
        }

        public async Task<dynamic> ContasDesligadas(long convenio, string cpfColaborador, DateTime? dataInicio, DateTime? dataFim, int? pagina, int? tamanhoPagina, int userId, int? statusRecisao)
        {
            var dataFiltro = ValidacaoFiltro(cpfColaborador, dataInicio, dataFim, pagina, tamanhoPagina, statusRecisao);

            return _asIntegration.ContasDesligadas(convenio.ToString(), userId, dataFiltro);
        }
        public async Task<dynamic> Faturas(long convenio, int idConta, int userId)
        {
            return _asIntegration.Faturas(convenio.ToString(), userId, idConta);
        }

        public async Task<dynamic> Contas(long convenio, string cpf, string email, string nome, int? pagina, int? tamanhoPagina, string ascendente, string ordem, string status, int userId)
        {
            var dataFiltro = FiltroContas(cpf, email, nome, pagina, tamanhoPagina, ascendente, ordem, status);

            return _asIntegration.Contas(convenio.ToString(), userId, dataFiltro);
        }

        private string FiltroContas(string cpf, string email, string nome, int? pagina, int? tamanhoPagina, string ascendente, string ordem, string status)
        {
            var filtro = "";

            if (!String.IsNullOrEmpty(cpf))
            {
                filtro += "cpf=" + cpf + "&";
            }

            if (!String.IsNullOrEmpty(nome))
            {
                filtro += "nome=" + nome + "&";
            }

            if (pagina != null)
            {
                filtro += "pagina=" + pagina + "&";
            }

            if (tamanhoPagina != null)
            {
                filtro += "tamanhoPagina=" + tamanhoPagina + "&";
            }

            if (!String.IsNullOrEmpty(ascendente))
            {
                filtro += "ascendente=" + ascendente + "&";
            }

            if (!String.IsNullOrEmpty(ordem))
            {
                filtro += "ordem=" + ordem + "&";
            }

            if (!String.IsNullOrEmpty(status))
            {
                filtro += "status=" + status + "&";
            }

            if (!String.IsNullOrEmpty(email))
            {
                filtro += "email=" + email + "&";
            }


            return filtro;
        }

        private async Task<string> GerarLinkTrctAsync(FileDto file)
        {
            if (file != null)
            {
                var ext = Path.GetExtension(file.file.FileName);
                var filename = "trct" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + file.description + ext;
                string key = "accounts/trct/" + filename;
                using var stream = new MemoryStream();
                file.file.CopyTo(stream);
                await AwsS3Integration.UploadFileAsync(stream, key);
                var url = AwsS3Integration.SignedUrl(key);

                return url;
            }
            else
            {
                return string.Empty;
            }
        }

        public async Task<dynamic> AbatimentoDivida(long convenio, AbatimentoDivida data, int userId)
        {
            if (data.file.file != null)
            {
                data.linkArquivo = await GerarLinkTrctAsync(data.file);
            }

            return _asIntegration.AbatimentoDivida(convenio.ToString(), userId, data);
        }

        public async Task<dynamic> FaturasResidual(long convenio, int idConta, int userId)
        {
            return _asIntegration.FaturasResidual(convenio.ToString(), userId, idConta);
        }

        public async Task<dynamic> Cargas(long convenio, int idConta, int userId)
        {
            return _asIntegration.Cargas(convenio.ToString(), userId, idConta);
        }

        public async Task<dynamic> Cartoes(long convenio, string cpf, string email, string nome, int? pagina, int? tamanhoPagina, string ascendente, string ordem, string status, int userId)
        {
            var dataFiltro = FiltroCartoes(cpf, email, nome, pagina, tamanhoPagina, ascendente, ordem, status);

            return _asIntegration.Cartoes(convenio.ToString(), userId, dataFiltro);
        }

        private string FiltroCartoes(string cpf, string email, string nome, int? pagina, int? tamanhoPagina, string ascendente, string ordem, string status)
        {
            var filtro = "";

            if (!String.IsNullOrEmpty(cpf))
            {
                filtro += "cpf=" + cpf + "&";
            }

            if (!String.IsNullOrEmpty(nome))
            {
                filtro += "nome=" + nome + "&";
            }

            if (pagina != null)
            {
                filtro += "pagina=" + pagina + "&";
            }

            if (tamanhoPagina != null)
            {
                filtro += "tamanhoPagina=" + tamanhoPagina + "&";
            }

            if (!String.IsNullOrEmpty(ascendente))
            {
                filtro += "ascendente=" + ascendente + "&";
            }

            if (!String.IsNullOrEmpty(ordem))
            {
                filtro += "ordem=" + ordem + "&";
            }

            if (!String.IsNullOrEmpty(status))
            {
                filtro += "status=" + status + "&";
            }

            if (!String.IsNullOrEmpty(email))
            {
                filtro += "email=" + email + "&";
            }


            return filtro;
        }


        public async Task<string> JobNotificationEmail()
        {
            Console.WriteLine("JobNotificationEmail - Start Process");

            var groups = await _userRepository.GetAllGroups();

            for(int i = 0; i < groups.Count(); i++)
            {
                var shops = await _shopRepository.GetAllByGroup(groups[i].Id);

                for(int y = 0; y < shops.Count(); y++)
                {
                    if(shops[y].ClosingDay > 0 && shops[y].ClosingDay <= 31)
                    {
                        var between = BetweenDates(shops[y].ClosingDay);
                        if(between.Situation)
                        {
                           
                            dynamic pendingDismissal = await ContasDesligadas(shops[y].Id, null, between.DtStart, between.DtEnd, null, null, 240, 2);
                            string convert = System.Text.Json.JsonSerializer.Serialize(pendingDismissal);

                            if(convert.Contains("cpfColaborador"))
                            {
                                    var filter = new List<AccountDismissal>
                                    {
                                        new()
                                        {
                                            Convenio = shops[y].Id,
                                            Cpf = null,
                                            UserId = 240,
                                            SolicitationId = 0
                                        }
                                    }; 

                                    await EmailRescision(filter, true, true);
                            }                 
                        }
                    }
                }
            }
            return "JobNotificationEmail - Process Finish";
        }

        private ValidDate BetweenDates(int day)
        {
            ValidDate result = new();
            DateTime d5 = new();
            DateTime d2 = new();
            DateTime d30 = new();
            DateTime closingDay = new();
			DateTime today = DateTime.Today;

            DateTime lastDayMonth = new(today.Year,
                                                 today.Month,  
                                                 DateTime.DaysInMonth(today.Year,
                                                                    today.Month));

            if(day <= lastDayMonth.Day)
			{
				closingDay = new DateTime(today.Year,
                                          today.Month, 
                                          day);				
			}
			else
			{
				today = today.AddMonths(1);
				closingDay = new DateTime(today.Year,
                                          today.Month,
										  day);                                      	
			}   
            
            int compare = DateTime.Compare(today, closingDay);

			if(compare > 0)
                closingDay = closingDay.AddMonths(1);


            d5 = closingDay.AddDays(-5);
			d2 = closingDay.AddDays(-2);
            d30 = closingDay.AddDays(-30);
           
            
            if( DateTime.Today >= d5 &&  DateTime.Today <= d2)
            {
                return result = new()
                {
                    DtStart = d30,
                    DtEnd = DateTime.Today.AddDays(-1),
                    Situation = true
                };
            }
            
            return result;
        }


        public async Task<ReportDismissalDiscount> ReportDismissalDiscount(long? shopId, long? groupId, DateTime? dtStart, DateTime? dtEnd, int? page, int? pageSize, int userId, string userMail, string userType, string search)
        {
            var dataFilter = ReportValidateFilter(shopId, groupId, dtStart, dtEnd, page, pageSize, search);

            var content = _asIntegration.ReportDismissalDiscount(userMail, dataFilter);

            string json = JsonSerializer.Serialize(content);

            ReportDismissalDiscount result = JsonSerializer.Deserialize<ReportDismissalDiscount>(json);


            if (Enum.TryParse(userType, out UserTypeEnum parsedUserType))
            {
                switch(parsedUserType)
                {
                    case  UserTypeEnum.Admin:
                        ReportDismissalDiscount accessFull = new()
                        {
                            CodigoRetorno = result.CodigoRetorno,
                            TotalPaginas = result.TotalPaginas,
                            MensagemRetorno = result.MensagemRetorno,
                            Lista = result.Lista
                            .Select(item => JsonSerializer.Deserialize<ReportDismissalDiscountResponse>(JsonSerializer.Serialize(item)))
                            .ToList<object>()
                        };
                        return accessFull;
                    
                    case UserTypeEnum.Master or UserTypeEnum.Convencional:
                        ReportDismissalDiscount limitedAccess = new()
                        {
                            CodigoRetorno = result.CodigoRetorno,
                            TotalPaginas = result.TotalPaginas,
                            MensagemRetorno = result.MensagemRetorno,
                            Lista = result.Lista
                            .Select(item => JsonSerializer.Deserialize<LimitedReportDismissalDiscountResponse>(JsonSerializer.Serialize(item)))
                            .ToList<object>()
                        };
                        return limitedAccess;


                    default:
                        throw new InvalidOperationException("Tipo de usuário não reconhecido");
                }
            }
            else
            {
                throw new InvalidOperationException("Tipo de usuário não reconhecido");
            }

        }


        private string ReportValidateFilter(long? shopId, long? groupId, DateTime? dtStart, DateTime? dtEnd, int? page, int? pageSize, string search)
        {
            var filter = "";


            if (shopId != null)
            {
                filter += "codigoConvenio=" + shopId + "&";
            }


            if (groupId != null && groupId != 0)
            {
                filter += "codigoGrupo=" + groupId + "&";
            }

            if (dtStart != null)
            {
                filter += "dataInicio=" + Convert.ToDateTime(dtStart).ToString("dd/MM/yyyy") + "&";
            }

            if (dtEnd != null)
            {
                filter += "dataFim=" + Convert.ToDateTime(dtEnd).ToString("dd/MM/yyyy") + "&";
            }

            if (page != null)
            {
                filter += "pagina=" + page + "&";
            }

            if (pageSize != null)
            {
                filter += "tamanhoPagina=" + pageSize + "&";
            }

            
            if (!String.IsNullOrEmpty(search))
            {
                MatchCollection matches = Regex.Matches(search, @"\d");
                int qndDigit = matches.Count;

                filter += (qndDigit == 11) ? "cpf=" + search + "&" : "nome=" + search + "&";

            }

            return filter;
        }


       public async Task<MemoryStream> ReportDismissalCSV(long? shopId, long? groupId, DateTime? dtStart, DateTime? dtEnd, int userId, string userMail, string userType, string search)
       {
            var retorno = await ReportDismissalDiscount(shopId, groupId, dtStart, dtEnd, 0, 0, userId, userMail, userType, search);
            var csv = new StringBuilder();
            var enc = new UTF8Encoding(true);
            var header = "";
            var accessFull = @"CPF;Nome Colaborador;Telefone Colaborador;Email Colaborador;Código Grupo;Nome do Grupo;Código Convênio;Nome do Convênio;Qnt Transações;Data de Cancelamento;Dívida Total;Desconto Rescisão;Dívida Boletada;Qnt Boletos;Primeiro Vencimento;Valor Primeiro Boleto;Último Vencimento;Valor Último Boleto";
            var limitedAccess = "@CPF;Nome Colaborador;Telefone Colaborador;Email Colaborador;Código Convênio;Nome do Convênio;Qnt Transações;Data de Cancelamento;Dívida Total;Desconto Rescisão;Dívida Boletada;Qnt Boletos;Primeiro Vencimento;Valor Primeiro Boleto;Último Vencimento;Valor Último Boleto";

            if (Enum.TryParse(userType, out UserTypeEnum parsedUserType))
            {
                header = (parsedUserType == UserTypeEnum.Admin) ? accessFull : limitedAccess;
            }
            else
            {
                throw new InvalidOperationException("Tipo de usuário não reconhecido");
            }

            csv.AppendLine(header);
            CultureInfo pt = new CultureInfo("pt-BR");

            foreach (var list in retorno.Lista)
            {
                var line = new List<string>();

                if (parsedUserType == UserTypeEnum.Admin)
                {
                    var row = (ReportDismissalDiscountResponse)list;
                    line.AddRange(new string[]
                    {
                        row.Cpf,
                        row.Nome,
                        row.Telefone,
                        row.Email,
                        row.CodigoGrupo.ToString(),
                        row.NomeGrupo,
                        row.CodigoConvenio.ToString(),
                        row.NomeConvenio,
                        row.QntTransacoes.ToString(),
                        row.DataCancelamento.ToString(),
                        row.DividaTotal.ToString(),
                        row.DescontoRescisao.ToString(),
                        row.DividaBoletada.ToString(),
                        row.QntBoletos.ToString(),
                        row.PrimeiroVencimento.ToString(),
                        row.ValorPrimeiroBoleto.ToString(),
                        row.UltimoVencimento.ToString(),
                        row.ValorUltimoBoleto.ToString()
                    });
                }
                else
                {
                    var row = (LimitedReportDismissalDiscountResponse)list;
                    line.AddRange(new string[]
                    {
                        row.Cpf,
                        row.Nome,
                        row.Telefone,
                        row.Email,
                        row.CodigoConvenio.ToString(),
                        row.NomeConvenio,
                        row.QntTransacoes.ToString(),
                        row.DataCancelamento.ToString(),
                        row.DividaTotal.ToString(),
                        row.DescontoRescisao.ToString(),
                        row.DividaBoletada.ToString(),
                        row.QntBoletos.ToString(),
                        row.PrimeiroVencimento.ToString(),
                        row.ValorPrimeiroBoleto.ToString(),
                        row.UltimoVencimento.ToString(),
                        row.ValorUltimoBoleto.ToString()
                    });
                }

                csv.AppendLine(string.Join(";", line));
            }

            byte[] preamble = enc.GetPreamble();
            byte[] byteArray = enc.GetBytes(csv.ToString());

            MemoryStream stream = new(Combine(preamble, byteArray))
            {
                Position = 0
            };

            return stream;
        }

    }
}
