namespace Fiap.Notifications.Domain.Services.Event
{
  public class EnviarEmailConfirmacaoPagamentoEvent
  {
    public int IdUsuario { get; set; }
    public string? Email { get; set; }
    public string? Nome { get; set; }
  }
}
