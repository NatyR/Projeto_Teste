using AutoMapper;
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
using Users.API.Common.Annotations.Validations;
using Users.API.Common.Dto;
using Users.API.Dto.User;
using Users.API.Entities;
using Users.API.Interfaces.Repositories;
using Users.API.Interfaces.Services;
using Users.API.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Users.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        public UsersController(IUserRepository repository, IUserService userService,
            IMapper mapper)
        {
            _repository = repository;
            _userService = userService;
            _mapper = mapper;
        }

        /// <summary>
        /// Endpoint responsável por listar todas as solicitacoes 
        /// </summary>
        [HttpPost("paged")]
        [Authorize]
        [ProducesResponseType(typeof(List<UserDto>), 200)]
        public async Task<ActionResult<ResponseDto<UserDto>>> GetAllPaged([FromBody] UserFilterDto filterOptions, [FromQuery] int limit, [FromQuery] int skip, [FromQuery] string order)
        {
            var response = await _userService.GetAllPaged(filterOptions, limit, skip, order);
            return Ok(response);
        }

        [HttpPost("csv")]
        [Authorize]
        public async Task<IActionResult> DownloadCsv([FromBody] UserFilterDto filterOptions)
        {
            var userProfile = Convert.ToInt64(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value);
            var userSystem = Convert.ToInt64(User.Claims.FirstOrDefault(x => x.Type == "sistema").Value);
            if (userSystem != 1 && filterOptions.SistemaId == 1)
            {
                return Unauthorized();
            }
            var userId = Convert.ToInt64(User.Claims.FirstOrDefault(x => x.Type == "id").Value);
            var user = await _repository.Get(userId);

            var response = await _userService.GetAllPaged(user.GroupId, filterOptions.SistemaId, 99999, 0, null, "id desc");
            var res = response.Data;
            if (res == null)
            {
                return NotFound();
            }
            var csv = new StringBuilder();
            var enc = new UTF8Encoding(true);
            var header = new string[] { };
            header = new string[] { "ID", "Usuário", "Tipo de Usuário", "Data de Cadastro", "Código do Grupo", "Nome do Grupo", "Código do Convênio", "Nome do Convênio", "CNPJ do Convênio", "CPF", "Matrícula", "E-mail", "Telefone", "Criado por", "Tipo Usuário Criação", "Status Usuário", "Editado em" };
            csv.AppendLine(String.Join(';', header));
            foreach (var row in res)
            {
                var line = new string[]
                {
                    row.Id.ToString(),
                    row.Name,
                    row.UserType.ToString(),
                    row.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                    row.GroupId.ToString(),
                    row.GroupName,
                    row.ShopId.ToString(),
                    row.ShopName,
                    row.ShopDocument,
                    row.Cpf,
                    row.RegistrationNumber,
                    row.Email,
                    row.Telefone,
                    row.CreatedByUser != null ? row.CreatedByUser.Name : "",
                    row.CreatedByUser != null ? row.CreatedByUser.UserType.ToString() : "",
                    row.Status,
                    row.UpdatedAt != null ? ((DateTime)row.UpdatedAt).ToString("dd/MM/yyyy HH:mm:ss") : ""
                };
                csv.AppendLine(String.Join(';', line));
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
        /// Endpoint responsável por listar os usuários
        /// </summary>
        [HttpGet("sistema/{sistema_id}")]
        [ProducesResponseType(typeof(ResponseDto<UserDto>), 200)]
        [Authorize]
        public async Task<ActionResult<ResponseDto<UserDto>>> GetAllPaged([FromRoute] int sistema_id, [FromQuery] int limit, [FromQuery] int skip, [FromQuery] string search, [FromQuery] string order)
        {
            var userProfile = Convert.ToInt64(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value);
            var userSystem = Convert.ToInt64(User.Claims.FirstOrDefault(x => x.Type == "sistema").Value);
            if (userSystem != 1 && sistema_id == 1)
            {
                return Unauthorized();
            }
            var userId = Convert.ToInt64(User.Claims.FirstOrDefault(x => x.Type == "id").Value);
            var user = await _repository.Get(userId);

            var response = await _userService.GetAllPaged(user.GroupId, sistema_id, limit, skip, search, order);
            return Ok(response);
        }
        [HttpGet()]
        [ProducesResponseType(typeof(ResponseDto<UserDto>), 200)]
        [Authorize]
        public async Task<ActionResult<ResponseDto<UserDto>>> GetAllUsers()
        {
            var userProfile = Convert.ToInt64(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value);
            var userSystem = Convert.ToInt64(User.Claims.FirstOrDefault(x => x.Type == "sistema").Value);
            if (userSystem != 1)
            {
                return Unauthorized();
            }
            var response = await _userService.GetAll();
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        public Task<UserDto> GetUserById([FromRoute] int id)
        {
            return _userService.Get(id);
        }

        [HttpPost]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] UserAddDto user)
        {
            try
            {
                int.TryParse(User.Claims.First(c => c.Type == "id").Value, out int idUser);
                var newUser = await _userService.Add(user, idUser);
                if(newUser == null)
                {
                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }
                    return BadRequest("Não foi possível cadastrar o usuário");
                }
                return CreatedAtAction("GetUserById", new { id = newUser.Id }, newUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("{id}/status")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Status(int id, [FromBody] UserStatusDto user)
        {
            try
            {
                await _userService.UpdateStatus(user);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPut("{id}")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put(int id, [FromBody] UserUpdateDto user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != user.Id)
            {
                return BadRequest();
            }
            await _userService.Update(user);
            return Ok(user);
        }

        [HttpPut("activate")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Activate([FromBody] long[] ids)
        {

            await _userService.Activate(ids);
            return Ok();
        }
        [HttpPut("deactivate")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Deactivate([FromBody] long[] ids)
        {

            await _userService.Deactivate(ids);
            return Ok();
        }

        [HttpPut("cancel")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Cancel([FromBody] long[] ids)
        {

            await _userService.Cancel(ids);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _userService.Delete(id);
            return NoContent();
        }

        [HttpGet("groups")]
        [Authorize]
        public async Task<List<Group>> GetGroups([FromQuery] string search)
        {
            int.TryParse(User.Claims.First(c => c.Type == "id").Value, out int idUser);

            var user = await _userService.Get(idUser);

            return await _repository.GetGroupsByUser(_mapper.Map<User>(user), search);
        }
        [HttpGet("all-groups")]
        [Authorize]
        public async Task<List<Group>> GetAllGroups()
        {
            return await _repository.GetAllGroups();
        }

       
        [HttpGet("group/{id}")]
        [Authorize]
        public async Task<Group> GetGroup(long id)
        {
            int.TryParse(User.Claims.First(c => c.Type == "id").Value, out int idUser);

            var user = await _userService.Get(idUser);

            return await _repository.GetGroupById(id, _mapper.Map<User>(user));
        }

        [HttpGet("convenios")]
        [Authorize]
        public async Task<List<Shop>> GetShops()
        {
            int.TryParse(User.Claims.First(c => c.Type == "id").Value, out int idUser);
            var user = await _userService.Get(idUser);
            if (user.UserType == Common.Enum.User.UserTypeEnum.Admin)
            {
                return (await _repository.GetShopsByGroup(-1)).OrderBy(s=>s.Id).ToList();
            }
            if (user.UserType == Common.Enum.User.UserTypeEnum.Convencional)
            {
                return (await _repository.GetShopsByUser(user.Id)).OrderBy(s => s.Id).ToList();
            }
            else
            {
                return (await _repository.GetShopsByGroup(user.GroupId)).OrderBy(s => s.Id).ToList();
            }
        }

        [HttpPost("users/menu")]
        [Authorize]
        [ProducesResponseType(typeof(List<UserMenuDto>), 200)]
        public async Task<ActionResult<ResponseDto<UserMenuDto>>> GetAllbyMenu(string searchTerm)
        {
           var response = await _userService.GetAllbyMenu(searchTerm);
            return Ok(response); 
             
        }

    }
}
