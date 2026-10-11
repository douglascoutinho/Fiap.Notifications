using Fiap.Notifications.Domain.Services.Event;
using RabbitMQ.Client;
using System.Text.Json;

namespace Fiap.Notifications.Api.Services.Extensions
{
  public static class Extension
  {
    public static ConnectionFactory CarregarConnectionFactory(this IConfiguration config)
    {
      var factory = new ConnectionFactory
      {
        HostName = config["RabbitMQ:HostName"] ?? string.Empty,
        Port = int.Parse(config["RabbitMQ:Port"] ?? string.Empty),
        UserName = config["RabbitMQ:UserName"] ?? string.Empty,
        Password = config["RabbitMQ:Password"] ?? string.Empty,
        VirtualHost = config["RabbitMQ:VirtualHost"] ?? "/"
      };

      return factory;
    }

    public static string ProcessarMensagemConsole(this EventResult<ProcessarPagamentoEvent> result)
    {
      if (result?.Data?.StatusPagamento == Domain.Services.Event.Enum.StatusPagamentoEnum.Aprovado)
      {
        return $"Pagamento Aprovado: Email de confirmação foi enviado para o endereco: {result?.Data?.Email} - {result?.Key}";
      }

      return string.Empty;
    }

    public static string ProcessarMensagemConsole(this EventResult<ProcessoCriacaoUsuarioEvent> result)
    {
      return $"Email de boas vindas enviado. Email: {result?.Data?.Email} - Nome: {result?.Data?.Nome} - {result?.Key}";
    }
  }
}

