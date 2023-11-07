using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Common.Annotations.Validations;
using Portal.API.Common.Dto;
using Portal.API.Dto.Menu;
using Portal.API.Dto.Notification;
using Portal.API.Entities;
using Portal.API.Interfaces.Repositories;
using Portal.API.Interfaces.Services;
using Portal.API.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }


        /// <summary>
        /// Retorna apenas as notificações NÃO LIDAS
        /// </summary>
        [HttpGet]
        public async Task<List<NotificationDto>> GetActiveNotifications()
        {
            int.TryParse(User.Claims.First(c => c.Type == "id").Value, out int idUser);
            return await _notificationService.GetActiveNotifications(idUser);
        }

        /// <summary>
        /// Retorna TODAS as notificaçãoes
        /// </summary>
        [HttpGet("all")] 
        public async Task<List<NotificationDto>> GetAllNotifications()
        {
            int.TryParse(User.Claims.First(c => c.Type == "id").Value, out int idUser);
            return await _notificationService.GetAll(idUser);
        }

        /// <summary>
        /// Criar um novo registro de notificação
        /// </summary>
        [HttpPost]
        public async Task Create(NotificationAddDto notification)
        {
            await _notificationService.Create(notification);
        }

        /// <summary>
        /// Atualizar dados de uma notificação
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] long id,[FromBody] NotificationUpdateDto notification)
        {
            if(id != notification.Id)
            {
                return BadRequest();
            }
            await _notificationService.Update(notification);
            return Ok();
        }

        /// <summary>
        /// Endpoint responsável por obter uma notificação por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> View([FromRoute] long id)
        {
            int.TryParse(User.Claims.First(c => c.Type == "id").Value, out int idUser);
            return Ok(await _notificationService.Get(id));
        }

        /// <summary>
        /// Inclusão de data de leitura na notificação
        /// </summary>
        [HttpPut("read")]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Read([FromBody] long[] ids)
        {
            int.TryParse(User.Claims.First(c => c.Type == "id").Value, out int idUser);
            await _notificationService.Read(idUser,ids);
            return Ok(ids);
        }

        /// <summary>
        /// Remoção da data de leitura da notificação
        /// </summary>
        [HttpPut("unread")]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Unread([FromBody] long[] ids)
        {
            int.TryParse(User.Claims.First(c => c.Type == "id").Value, out int idUser);

            await _notificationService.Unread(idUser,ids);
            return Ok(ids);
        }

        /// <summary>
        /// Exclusão de notificações
        /// </summary>
        [HttpPost("remove")]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Remove([FromBody] long[] ids)
        {
            int.TryParse(User.Claims.First(c => c.Type == "id").Value, out int idUser);

            await _notificationService.Remove(idUser, ids);
            return Ok(ids);
        }

        /// <summary>
        /// Endpoint responsável por retornar as notificações de acordo com
        /// configuração de paginação
        /// </summary>
        [HttpPost("paged")]
        [Authorize]
        [ProducesResponseType(typeof(List<NotificationDto>), 200)]
        public async Task<ActionResult<ResponseDto<NotificationDto>>> GetAllPaged([FromQuery] int limit, [FromQuery] int skip, [FromQuery] string order)
        {
            int.TryParse(User.Claims.First(c => c.Type == "id").Value, out int idUser);
            var response = await _notificationService.GetAllPaged(limit, skip, order, idUser);
            return Ok(response);
        }


        /// <summary>
        /// Endpoint responsável por obter os tipos de notificações
        /// </summary>
        [HttpGet("types")]
        [ProducesResponseType(typeof(List<NotificationTypeDto>), 200)]
        public async Task<ActionResult<List<NotificationTypeDto>>> GetTypes()
        {
            return Ok(await _notificationService.GetTypes());
        }


        /// <summary>
        /// Retorna as notificações NÃO LIDAS a partir do tipo
        /// </summary>
        [HttpPost("user/type")]
        [Authorize]
        [ProducesResponseType(typeof(List<NotificationDto>), 200)]
        public async Task<ActionResult<ResponseDto<NotificationDto>>> GetActiveNotificationsByType([FromQuery] long userId, [FromQuery] int type)

        {
            return await _notificationService.GetActiveNotificationsByType(userId, type);
        }

    }
}
