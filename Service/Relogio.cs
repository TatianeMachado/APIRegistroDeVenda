namespace APIregistroDeVenda.Service;

public class Relogio : IRelogio
{
    private readonly TimeZoneInfo _fuso =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public DateOnly Hoje
    {
        get
        {
            var agora = TimeZoneInfo.ConvertTime(
                DateTimeOffset.UtcNow,
                _fuso);

            return DateOnly.FromDateTime(agora.DateTime);
        }
    }
}