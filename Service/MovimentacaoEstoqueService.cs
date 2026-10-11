using APIregistroDeVenda.Domain;
using APIregistroDeVenda.DTO;
using APIregistroDeVenda.Repository;

namespace APIregistroDeVenda.Service;

public class MovimentacaoEstoqueService
{
    private readonly IMovimentacaoEstoqueRepository _repository;

    public MovimentacaoEstoqueService(
        IMovimentacaoEstoqueRepository repository)
    {
        _repository = repository;
    }

    public async Task<MovimentacaoEstoqueResponse> CriarAsync(
        MovimentacaoEstoqueCriarRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CodigoProduto))
        {
            throw new ArgumentException(
                "Informe o código do produto.");
        }

        ValidarMovimentacao(request.Quantidade, request.Tipo);

        return await _repository
            .ExecutarEmTransacaoAsync<MovimentacaoEstoqueResponse>(
                async () =>
                {
                    var produto =
                        await _repository.ConsultarProdutoPorCodigoAsync(
                            request.CodigoProduto);

                    if (produto is null)
                    {
                        throw new KeyNotFoundException(
                            "Produto não encontrado.");
                    }

                    int quantidade = request.Quantidade!.Value;
                    TipoMovimentacao tipo = request.Tipo!.Value;

                    int estoqueFinal =
                        tipo == TipoMovimentacao.Entrada
                            ? checked(produto.Estoque + quantidade)
                            : checked(produto.Estoque - quantidade);

                    produto.Estoque = estoqueFinal;

                    var movimentacao = new MovimentacaoEstoque
                    {
                        ProdutoId = produto.ProdutoId,
                        Tipo = tipo,
                        Descricao = ObterDescricao(tipo),
                        Quantidade = quantidade,
                        EstoqueFinal = estoqueFinal,
                        DataMovimentacao = DateTime.UtcNow
                    };

                    _repository.Adicionar(movimentacao);

                    await _repository.SalvarAsync();

                    return CriarResponse(movimentacao, produto);
                });
    }

    public async Task<MovimentacaoEstoqueResponse?> AtualizarAsync(
        int id,
        MovimentacaoEstoqueAtualizarRequest request)
    {
        ValidarMovimentacao(request.Quantidade, request.Tipo);

        return await _repository
            .ExecutarEmTransacaoAsync<MovimentacaoEstoqueResponse?>(
                async () =>
                {
                    var movimentacao =
                        await _repository.ConsultarMovimentacaoAsync(id);

                    if (movimentacao is null)
                    {
                        return null;
                    }

                    var produto =
                        await _repository.ConsultarProdutoPorIdAsync(
                            movimentacao.ProdutoId);

                    var posteriores =
                        await _repository.ConsultarPosterioresAsync(
                            movimentacao.ProdutoId,
                            movimentacao.MovimentacaoEstoqueId);

                    long efeitoAnterior = CalcularEfeito(
                        movimentacao.Tipo,
                        movimentacao.Quantidade);

                    TipoMovimentacao tipoNovo = request.Tipo!.Value;
                    int quantidadeNova = request.Quantidade!.Value;

                    long efeitoNovo = CalcularEfeito(
                        tipoNovo,
                        quantidadeNova);

                    long diferenca = efeitoNovo - efeitoAnterior;

                    movimentacao.Tipo = tipoNovo;
                    movimentacao.Quantidade = quantidadeNova;
                    movimentacao.Descricao = ObterDescricao(tipoNovo);

                    movimentacao.EstoqueFinal = checked(
                        (int)((long)movimentacao.EstoqueFinal + diferenca));

                    foreach (var posterior in posteriores)
                    {
                        posterior.EstoqueFinal = checked(
                            (int)((long)posterior.EstoqueFinal + diferenca));
                    }

                    produto.Estoque = checked(
                        (int)((long)produto.Estoque + diferenca));

                    await _repository.SalvarAsync();

                    return CriarResponse(movimentacao, produto);
                });
    }

    public async Task<List<MovimentacaoEstoqueResponse>> ConsultarAsync()
    {
        return await _repository.ListarAsync();
    }

    public async Task<MovimentacaoEstoqueResponse?> ConsultarPorIdAsync(
        int id)
    {
        return await _repository.ConsultarPorIdAsync(id);
    }

    public async Task<EstoqueResponse> ConsultarEstoqueAsync()
    {
        var produtos = await _repository.ConsultarEstoqueAsync();

        return new EstoqueResponse
        {
            Estoque = produtos
        };
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        return await _repository.ExecutarEmTransacaoAsync<bool>(
            async () =>
            {
                var movimentacao =
                    await _repository.ConsultarMovimentacaoAsync(id);

                if (movimentacao is null)
                {
                    return false;
                }

                var produto =
                    await _repository.ConsultarProdutoPorIdAsync(
                        movimentacao.ProdutoId);

                var posteriores =
                    await _repository.ConsultarPosterioresAsync(
                        movimentacao.ProdutoId,
                        movimentacao.MovimentacaoEstoqueId);

                long efeito = CalcularEfeito(
                    movimentacao.Tipo,
                    movimentacao.Quantidade);

                produto.Estoque = checked(
                    (int)((long)produto.Estoque - efeito));

                foreach (var posterior in posteriores)
                {
                    posterior.EstoqueFinal = checked(
                        (int)((long)posterior.EstoqueFinal - efeito));
                }

                _repository.Remover(movimentacao);

                await _repository.SalvarAsync();

                return true;
            });
    }

    private static void ValidarMovimentacao(
        int? quantidade,
        TipoMovimentacao? tipo)
    {
        if (quantidade is null || quantidade <= 0)
        {
            throw new ArgumentException(
                "A quantidade deve ser maior que zero.");
        }

        if (tipo != TipoMovimentacao.Entrada &&
            tipo != TipoMovimentacao.Saida)
        {
            throw new ArgumentException(
                "Informe entrada ou saída.");
        }
    }

    private static long CalcularEfeito(
        TipoMovimentacao tipo,
        int quantidade)
    {
        return tipo == TipoMovimentacao.Entrada
            ? quantidade
            : -(long)quantidade;
    }

    private static string ObterDescricao(TipoMovimentacao tipo)
    {
        return tipo == TipoMovimentacao.Entrada
            ? "Entrada de mercadoria"
            : "Saída de mercadoria";
    }

    private static MovimentacaoEstoqueResponse CriarResponse(
        MovimentacaoEstoque movimentacao,
        Produto produto)
    {
        return new MovimentacaoEstoqueResponse
        {
            MovimentacaoEstoqueId = movimentacao.MovimentacaoEstoqueId,
            CodigoProduto = produto.CodigoProduto!,
            DescricaoProduto = produto.Descricao!,
            Tipo = movimentacao.Tipo,
            Descricao = movimentacao.Descricao,
            Quantidade = movimentacao.Quantidade,
            EstoqueFinal = movimentacao.EstoqueFinal,
            DataMovimentacao = movimentacao.DataMovimentacao
        };
    }
}