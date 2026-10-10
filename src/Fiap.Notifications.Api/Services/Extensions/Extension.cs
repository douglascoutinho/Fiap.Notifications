using RabbitMQ.Client;

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
  }
} 

    