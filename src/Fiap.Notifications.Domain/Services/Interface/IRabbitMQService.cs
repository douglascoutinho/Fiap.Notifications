namespace Fiap.Notifications.Domain.Services.Interface
{
    public interface IRabbitMQService
    {
        Task SendMessageAsync(string message);
    }
}
