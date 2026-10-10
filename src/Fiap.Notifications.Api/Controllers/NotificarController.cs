using Fiap.Notifications.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.Notifications.Api.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize(Roles = "Administrador,Usuario")]
  public class NotificarController : ControllerBase
  {
    readonly NotificarService _notificarService;

    public NotificarController(NotificarService notificarService)
    {
      _notificarService = notificarService;
    }

    /// <summary>
    /// Notifica os usuários cadastrados no sistema, enviando e-mails de boas-vindas e de confirmação de compra.
    /// </summary>
    /// <param name="dto">Corpo da requisição</param>
    /// <returns></returns>
    /// <response code="200"></response>
    [HttpPost("")]
    [ProducesResponseType(typeof(string), 200)]
    public IActionResult Post([FromBody] string menssagem)
    {
      _notificarService.Processar(menssagem);

      return Ok("Solicitação Enviada Com Sucesso.");
    }
  }
}