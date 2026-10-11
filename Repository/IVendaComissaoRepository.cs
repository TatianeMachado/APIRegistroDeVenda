using APIregistroDeVenda.Domain;

namespace APIregistroDeVenda.Repository;

public interface IVendaComissaoRepository
{
    Task AdicionarAsync(List<VendaComissao> vendas);

    Task<List<VendaComissao>> ListarAsync();

    Task<VendaComissao?> ConsultarPorIdAsync(int id);

    Task AtualizarAsync(VendaComissao venda);

    Task ExcluirAsync(VendaComissao venda);
}