using RabbitMQ.Client;

namespace Fiap.Notifications.Api.Services.IoC
{
	/// <summary>
	/// Classe de configuração de IoC
	/// </summary>
	public static class Configuration
	{
		/// <summary>
		/// Registrar as dependencias dentro do motor de IoC
		/// </summary>
		/// <param name="services"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		public static IServiceCollection RegisterDependencies(this IServiceCollection services, IConfiguration configuration)
		{

      services.AddSingleton<ConnectionFactory>();

      services.AddHostedService<RabbitMQConsumerService>();

      //Interfaces
      services.AddScoped<Domain.Services.Interface.IRabbitMQService, RabbitMQService>();

      // Domain Services			
      services.AddScoped<Domain.Services.NotificarService>();	

      return services;
		}
	}
}
