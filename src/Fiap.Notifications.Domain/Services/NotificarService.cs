using Fiap.Notifications.Domain.Services.Interface;
using System.ComponentModel.DataAnnotations;

namespace Fiap.Notifications.Domain.Services
{
  public class NotificarService
  {
    IRabbitMQService _rabbitMQ;

    public NotificarService(IRabbitMQService rabbitMQ)
    {
      _rabbitMQ = rabbitMQ;
    }

    public void Processar(string menssagem)
    {
      if (string.IsNullOrEmpty(menssagem))
        throw new ValidationException("Campo menssagem é obrigatório. ");

      _rabbitMQ.SendMessageAsync("notifications-default", menssagem);
    }
  }
}
