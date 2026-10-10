using Fiap.Notifications.Api.Services.Extensions;
using Fiap.Notifications.Domain.Services.Interface;
using RabbitMQ.Client;
using System.Text;

namespace Fiap.Notifications.Api.Services
{
  public class RabbitMQService : IRabbitMQService
  {
    readonly IConfiguration _configuration;
    private const string QueueName = "mensagens";

    public RabbitMQService(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    public async Task SendMessageAsync(string message)
    {
      var factory = _configuration.CarregarConnectionFactory();

      using var connection = await factory.CreateConnectionAsync();
      using var channel = await connection.CreateChannelAsync();

      // Declara a fila (criar se não existir)
      await channel.QueueDeclareAsync
      (
          queue: QueueName,
          durable: true,
          exclusive: false,
          autoDelete: false,
          arguments: null
      );

      // Prepara a mensagem
      var messageBody = Encoding.UTF8.GetBytes(message);

      // Envia a mensagem
      await channel.BasicPublishAsync
      (
        exchange: "",
        routingKey: QueueName,
        body: messageBody
      );
    }
  }
}
