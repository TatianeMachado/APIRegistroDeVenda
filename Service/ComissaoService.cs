using APIregistroDeVenda.Context;
using APIregistroDeVenda.Domain;
using APIregistroDeVenda.DTO;
using APIregistroDeVenda.Repository;

namespace APIregistroDeVenda.Service;

public class ComissaoService
{
    private readonly IVendaComissaoRepository _repository;

    public ComissaoService(IVendaComissaoRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> CadastrarAsync(
    ComissaoCalcularRequest request)
    {
        var vendas = request.Vendas
            .Select(venda => new VendaComissao
            {
                Vendedor = venda.Vendedor!,
                Valor = venda.Valor!.Value
            })
            .ToList();

        await _repository.AdicionarAsync(vendas);

        return vendas.Count;
    }
    public async Task<VendaComissaoResponse?> AtualizarAsync(
    int id,
    VendaComissaoAtualizarRequest request)
    {
        var venda = await _repository.ConsultarPorIdAsync(id);

        if (venda is null)
        {
            return null;
        }

        venda.Vendedor = request.Vendedor!;
        venda.Valor = request.Valor!.Value;

        await _repository.AtualizarAsync(venda);

        return new VendaComissaoResponse
        {
            VendaComissaoId = venda.VendaComissaoId,
            Vendedor = venda.Vendedor,
            Valor = venda.Valor,
            Comissao = CalcularComissao(venda.Valor)
        };
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        var venda = await _repository.ConsultarPorIdAsync(id);

        if (venda is null)
        {
            return false;
        }

        await _repository.ExcluirAsync(venda);

        return true;
    }
    public List<ComissaoVendedorResponse> Calcular(
        ComissaoCalcularRequest request)
    {
        return request.Vendas
            .GroupBy(venda => venda.Vendedor!)
            .Select(grupo => new ComissaoVendedorResponse
            {
                Vendedor = grupo.Key,
                QuantidadeVendas = grupo.Count(),
                TotalVendas = grupo.Sum(
                    venda => venda.Valor!.Value),
                TotalComissao = grupo.Sum(
                    venda => CalcularComissao(venda.Valor!.Value))
            })
            .ToList();
    }

    private static decimal CalcularComissao(decimal valorVenda)
    {
        if (valorVenda < 100m)
            return 0m;

        if (valorVenda < 500m)
            return valorVenda * 0.01m;

        return valorVenda * 0.05m;
    }

    public async Task<VendaComissaoResponse?> ConsultarPorIdAsync(int id)
    {
        var venda = await _repository.ConsultarPorIdAsync(id);

        if (venda is null)
        {
            return null;
        }

        return new VendaComissaoResponse
        {
            VendaComissaoId = venda.VendaComissaoId,
            Vendedor = venda.Vendedor,
            Valor = venda.Valor,
            Comissao = CalcularComissao(venda.Valor)
        };
    }


    //EXATAMENTE O DESAFIO

    public async Task<VendasResponse> ListarVendasAsync()
    {
        var vendas = await _repository.ListarAsync();

        return new VendasResponse
        {
            Vendas = vendas
                .Select(venda => new VendaRegistroResponse
                {
                    Vendedor = venda.Vendedor,
                    Valor = venda.Valor
                })
                .ToList()
        };
    }

    public async Task<List<ComissaoFaixaResponse>> ConsultarPorFaixaAsync()
    {
        var vendas = await _repository.ListarAsync();

        var faixas = new List<ComissaoFaixaResponse>
    {
        new()
        {
            Regra = "Vendas abaixo de R$ 100,00",
            Percentual = 0m
        },
        new()
        {
            Regra = "Vendas de R$ 100,00 até menos de R$ 500,00",
            Percentual = 1m
        },
        new()
        {
            Regra = "Vendas a partir de R$ 500,00",
            Percentual = 5m
        }
    };

        foreach (var venda in vendas)
        {
            int indice = venda.Valor < 100m
                ? 0
                : venda.Valor < 500m ? 1 : 2;

            decimal comissao = CalcularComissao(venda.Valor);

            var faixa = faixas[indice];

            faixa.Vendas.Add(new VendaComissaoResponse
            {
                VendaComissaoId = venda.VendaComissaoId,
                Vendedor = venda.Vendedor,
                Valor = venda.Valor,
                Comissao = comissao
            });

            faixa.TotalComissao += comissao;
        }

        return faixas;
    }
}