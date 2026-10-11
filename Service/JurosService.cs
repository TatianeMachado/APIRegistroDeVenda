namespace APIregistroDeVenda.Service;

public class JurosService
{
    private readonly IRelogio _relogio;

    public JurosService(IRelogio relogio)
    {
        _relogio = relogio;
    }

    public decimal Calcular(
        decimal valor,
        DateOnly dataVencimento)
    {
        int diasAtraso =
            _relogio.Hoje.DayNumber - dataVencimento.DayNumber;

        if (diasAtraso <= 0)
        {
            return 0m;
        }

        return valor * 0.025m * diasAtraso;
    }
}