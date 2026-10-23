using Fiap.Notifications.Domain.Services.Event;
using RabbitMQ.Client;

namespace Fiap.Notifications.Api.Services.Extensions
{
  public static class Extension
  {
    public static ConnectionFactory CarregarConnectionFactory(this IConfiguration config)
    {
      var section = config.GetSection("RabbitMQ");

      return new ConnectionFactory
      {
        HostName = ValorOuPadrao(section["Host"], "localhost"),
        Port = int.TryParse(section["Port"], out var port) ? port : 5672,
        UserName = ValorOuPadrao(section["User"], "admin"),
        Password = ValorOuPadrao(section["Password"], "admin1234"),
        VirtualHost = ValorOuPadrao(section["VirtualHost"], "/")
      };
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

    static string ValorOuPadrao(string? valor, string padrao) => string.IsNullOrWhiteSpace(valor) ? padrao : valor;
  }
}

