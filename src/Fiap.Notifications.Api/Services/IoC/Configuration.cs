using Fiap.Notifications.Api.Services.RabbitMQ;

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
      services.AddHostedService<ConsumerDefaultEventService>();
      services.AddHostedService<ConsumerPaymentProcessedEventService>();
      services.AddHostedService<ConsumerUserCreatedEventService>();

      //Interfaces
      services.AddScoped<Domain.Services.Interface.IRabbitMQService, PublishService>();

      // Domain Services			
      services.AddScoped<Domain.Services.NotificarService>();	

      return services;
		}
	}
}
