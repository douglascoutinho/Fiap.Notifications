using Fiap.Notifications.Api.Services.Extensions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Fiap.Notifications.Api.Services
{
  public class RabbitMQConsumerService : BackgroundService
  {
    readonly IConfiguration _config;
    private readonly ILogger<RabbitMQConsumerService> _logger;
    private const string QueueName = "mensagens";

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMQConsumerService(IConfiguration config, ILogger<RabbitMQConsumerService> logger)
    {
      _config = config;
      _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      Console.WriteLine("============ MESSAGE CONSUMER V12 ============");

      var factory =  _config.CarregarConnectionFactory();
   
      Console.WriteLine($"Host: {factory.HostName}:{factory.Port} | User: {factory.UserName}");

      // Retentativa: o RabbitMQ pode demorar para ficar disponível
      const int maxTentativas = 10;
      for (int tentativa = 1; tentativa <= maxTentativas && !stoppingToken.IsCancellationRequested; tentativa++)
      {
        try
        {
          _connection = await factory.CreateConnectionAsync(stoppingToken);
          _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
          break;
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Tentativa {tentativa}/{maxTentativas} falhou: {ex.Message}");
          if (tentativa == maxTentativas) return;
          await Task.Delay(3000, stoppingToken);
        }
      }

      if (_channel is null) return;

      await _channel.QueueDeclareAsync(
          queue: QueueName,
          durable: true,
          exclusive: false,
          autoDelete: false,
          arguments: null,
          cancellationToken: stoppingToken);

      Console.WriteLine($"Aguardando mensagens da fila '{QueueName}'...");

      var consumer = new AsyncEventingBasicConsumer(_channel);
      consumer.ReceivedAsync += async (model, ea) =>
      {
        var message = Encoding.UTF8.GetString(ea.Body.ToArray());

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Mensagem recebida: {message}");

        await Task.Delay(1000); // Simula processamento

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Mensagem processada com sucesso!");
      };

      await _channel.BasicConsumeAsync(
          queue: QueueName,
          autoAck: true,
          consumer: consumer,
          cancellationToken: stoppingToken);

      // Mantém o serviço vivo até a aplicação ser encerrada
      await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
      if (_channel is not null) await _channel.CloseAsync(cancellationToken);
      if (_connection is not null) await _connection.CloseAsync(cancellationToken);
      await base.StopAsync(cancellationToken);
    }
  }
}