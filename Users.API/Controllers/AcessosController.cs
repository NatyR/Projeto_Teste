using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.API.Common.Annotations.Validations;
using Users.API.Common.Dto;
using Users.API.Dto.Acesso;
using Users.API.Entities;
using Users.API.Interfaces.Repositories;
using Users.API.Interfaces.Services;
using Users.API.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Users.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcessosController : ControllerBase
    {
        private readonly IAcessoService _acessoService;
        public AcessosController(IAcessoService acessoService)
        {
            _acessoService = acessoService;
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }


        [HttpPost("download")]
        [Authorize]
        public async Task<IActionResult> Download([FromBody] AcessoFilterDto filterOptions)
        {
            var res = await _acessoService.GetAll(filterOptions);
            if (res == null)
            {
                return NotFound();
            }
            var csv = new StringBuilder();
            var enc = new UTF8Encoding(true);
            var header = @"ID;Data;Usuario;CPF;Nome;Email;Telefone;Perfil;Metodo;URL;Dados";
            csv.AppendLine(header);
            foreach (var log in res)
            {
                var postData = !String.IsNullOrEmpty(log.PostData) ? log.PostData.Replace("\r", "").Replace("\n","") : "";
                var date = Convert.ToDateTime(log.CreatedAt).ToString("dd/MM/yyyy HH:mm:ss");
                var line = $"{log.Id};{date};{log.UsuarioId};{log.Cpf};{log.Nome};{log.Email};{log.Telefone};{log.Perfil};{log.Method};{log.Url};{postData}";
                csv.AppendLine(line);
            }
            byte[] preamble = enc.GetPreamble();
            byte[] byteArray = enc.GetBytes(csv.ToString());

            MemoryStream stream = new MemoryStream(Combine(preamble, byteArray));
            stream.Position = 0;

            return File(stream, "application/octet-stream", "log_acesso.csv");
        }
        [HttpPost("paged")]
        [Authorize]
        [ProducesResponseType(typeof(List<AcessoDto>), 200)]
        public async Task<ActionResult<ResponseDto<AcessoDto>>> GetAllPaged([FromBody] AcessoFilterDto filterOptions, [FromQuery] int limit, [FromQuery] int skip, [FromQuery] string order)
        {


            var response = await _acessoService.GetAllPaged(filterOptions, limit, skip, order);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var acesso = await _acessoService.Get(id);
            if (acesso == null)
            {
                return NotFound();
            }
            return Ok(acesso);
        }

      



    }
}
