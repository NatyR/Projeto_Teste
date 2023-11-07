using Accounts.API.Common.Dto;
using Accounts.API.Common.Dto.Account;
using Accounts.API.Common.Dto.LimitRequest;
using Accounts.API.Common.Dto.User;
using Accounts.API.Entities;
using Accounts.API.Integrations.AwsS3;
using Accounts.API.Interfaces.Repositories;
using Accounts.API.Interfaces.Services;
using AutoMapper;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.API.Services
{
    public class LimitRequestService : ILimitRequestService
    {
        private readonly ILimitRequestRepository _limitRequestRepository;
        private readonly IMapper _mapper;

        public LimitRequestService(
            ILimitRequestRepository limitRequestRepository,
            IMapper mapper)
        {
            _limitRequestRepository = limitRequestRepository;
            _mapper = mapper;
        }
        public async Task<ResponseDto<LimitRequestDto>> GetAllPaged(long convenio_id, string status, int limit, int skip, string search, string order)
        {
            var entities = await _limitRequestRepository.GetAllPaged(convenio_id, status, limit, skip, search, order);
            var dtos = _mapper.Map<ResponseDto<LimitRequestDto>>(entities);
            return dtos;
        }

        public async Task Approve(long[] ids, UserDtoAccounts user)
        {
            AccountLimitDto accounts = new AccountLimitDto();
            accounts.CurrentUser = user;
            foreach (var id in ids)
            {
                LimitRequest limitRequest = await _limitRequestRepository.GetById(id);
                if(limitRequest != null)
                {
                    accounts.Convenio = limitRequest.ShopId;
                    accounts.Accounts.Add(new AccountSelectedDto()
                    {
                        CardLimit = limitRequest.PreviousLimit,
                        Cpf = limitRequest.Cpf,
                        Name = limitRequest.Name,
                        NewCardLimit = limitRequest.NewLimit,
                        RegistrationNumber = limitRequest.RegistrationNumber,
                    });
                }
            }
            if (accounts.Accounts.Count > 0)
            {
                var csv = new StringBuilder();
                var enc = new UTF8Encoding(true);
                var header = @"Matricula;Nome;CPF;RG;Orgao;Data Nascimento;Data Admissao;Nome Mae;Nome Pai;Celular;Email;CNPJ;Codigo Filial;Nome Filial;Codigo Centro de Custo;Nome Centro de Custo;Unidade Entrega;Entrega Colaborador;Sexo;Nacionalidade;Cargo;CEP;Endereco;Numero;Complemento;Bairro;Cidade;UF;Nome Cartao;Limite;Movimento;Codigo Usuario;Nome Usuario;Email Usuario";
                csv.AppendLine(header);
                foreach (var account in accounts.Accounts)
                {
                    var line = $"{account.RegistrationNumber};{account.Name};{account.Cpf};;;;;;;;;;;;;;;N;;;;;;;;;;;;{account.NewCardLimit};9;{accounts.CurrentUser.Id};{accounts.CurrentUser.Name};{accounts.CurrentUser.Email}";
                    csv.AppendLine(line);
                }
                byte[] preamble = enc.GetPreamble();
                byte[] byteArray = enc.GetBytes(csv.ToString());

                MemoryStream stream = new MemoryStream(Combine(preamble, byteArray));
                await AwsS3Integration.UploadFileAsync(stream, "accounts/import/" + accounts.Convenio.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_LIMITE.csv");
                foreach (var id in ids)
                {
                    await _limitRequestRepository.Approve(id, user.Id);
                }
            }
            
        }

        public async Task Reject(long[] ids, UserDtoAccounts user)
        {
            foreach (var id in ids)
            {
                await _limitRequestRepository.Reject(id, user.Id);
            }
        }
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }
    }
}
