using APIregistroDeVenda.Domain;
using APIregistroDeVenda.DTO;

namespace APIregistroDeVenda.Repository;

public interface IMovimentacaoEstoqueRepository
{
    Task<T> ExecutarEmTransacaoAsync<T>(Func<Task<T>> operacao);

    Task<Produto?> ConsultarProdutoPorCodigoAsync(string codigo);

    Task<Produto> ConsultarProdutoPorIdAsync(int id);

    Task<MovimentacaoEstoque?> ConsultarMovimentacaoAsync(int id);

    Task<List<MovimentacaoEstoque>> ConsultarPosterioresAsync(
        int produtoId,
        int movimentacaoId);

    void Adicionar(MovimentacaoEstoque movimentacao);

    void Remover(MovimentacaoEstoque movimentacao);

    Task SalvarAsync();

    Task<List<MovimentacaoEstoqueResponse>> ListarAsync();

    Task<MovimentacaoEstoqueResponse?> ConsultarPorIdAsync(int id);

    Task<List<ProdutoEstoqueResponse>> ConsultarEstoqueAsync();
}