namespace Fiap.Notifications.Domain.Services.Event
{
  public class JogoEvent
  {
    public JogoEvent(int id, string? nomeJogo, decimal preco)
    {
      Id = id;
      NomeJogo = nomeJogo;
      Preco = preco;
    }

    public int Id { get; private set; }
    public string? NomeJogo { get; private set; }
    public decimal Preco { get; private set; }
  }
}
