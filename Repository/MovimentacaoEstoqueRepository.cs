using System.Data;
using APIregistroDeVenda.Context;
using APIregistroDeVenda.Domain;
using APIregistroDeVenda.DTO;
using Microsoft.EntityFrameworkCore;

namespace APIregistroDeVenda.Repository;

public class MovimentacaoEstoqueRepository
    : IMovimentacaoEstoqueRepository
{
    private readonly AppDbContext _context;

    public MovimentacaoEstoqueRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<T> ExecutarEmTransacaoAsync<T>(
        Func<Task<T>> operacao)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        var resultado = await operacao();

        await transaction.CommitAsync();

        return resultado;
    }

    public async Task<Produto?> ConsultarProdutoPorCodigoAsync(
        string codigo)
    {
        return await _context.Produtos
            .SingleOrDefaultAsync(p => p.CodigoProduto == codigo);
    }

    public async Task<Produto> ConsultarProdutoPorIdAsync(int id)
    {
        return await _context.Produtos
            .SingleAsync(p => p.ProdutoId == id);
    }

    public async Task<MovimentacaoEstoque?> ConsultarMovimentacaoAsync(
        int id)
    {
        return await _context.MovimentacoesEstoque
            .SingleOrDefaultAsync(m => m.MovimentacaoEstoqueId == id);
    }

    public async Task<List<MovimentacaoEstoque>>
        ConsultarPosterioresAsync(
            int produtoId,
            int movimentacaoId)
    {
        return await _context.MovimentacoesEstoque
            .Where(m =>
                m.ProdutoId == produtoId &&
                m.MovimentacaoEstoqueId > movimentacaoId)
            .OrderBy(m => m.MovimentacaoEstoqueId)
            .ToListAsync();
    }

    public void Adicionar(MovimentacaoEstoque movimentacao)
    {
        _context.MovimentacoesEstoque.Add(movimentacao);
    }

    public void Remover(MovimentacaoEstoque movimentacao)
    {
        _context.MovimentacoesEstoque.Remove(movimentacao);
    }

    public async Task SalvarAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<MovimentacaoEstoqueResponse>> ListarAsync()
    {
        return await _context.MovimentacoesEstoque
            .AsNoTracking()
            .OrderBy(m => m.MovimentacaoEstoqueId)
            .Select(m => new MovimentacaoEstoqueResponse
            {
                MovimentacaoEstoqueId = m.MovimentacaoEstoqueId,
                CodigoProduto = m.Produto!.CodigoProduto!,
                DescricaoProduto = m.Produto!.Descricao!,
                Tipo = m.Tipo,
                Descricao = m.Descricao,
                Quantidade = m.Quantidade,
                EstoqueFinal = m.EstoqueFinal,
                DataMovimentacao = m.DataMovimentacao
            })
            .ToListAsync();
    }

    public async Task<MovimentacaoEstoqueResponse?> ConsultarPorIdAsync(
        int id)
    {
        return await _context.MovimentacoesEstoque
            .AsNoTracking()
            .Where(m => m.MovimentacaoEstoqueId == id)
            .Select(m => new MovimentacaoEstoqueResponse
            {
                MovimentacaoEstoqueId = m.MovimentacaoEstoqueId,
                CodigoProduto = m.Produto!.CodigoProduto!,
                DescricaoProduto = m.Produto!.Descricao!,
                Tipo = m.Tipo,
                Descricao = m.Descricao,
                Quantidade = m.Quantidade,
                EstoqueFinal = m.EstoqueFinal,
                DataMovimentacao = m.DataMovimentacao
            })
            .SingleOrDefaultAsync();
    }

    public async Task<List<ProdutoEstoqueResponse>>
        ConsultarEstoqueAsync()
    {
        return await _context.Produtos
            .AsNoTracking()
            .OrderBy(p => p.ProdutoId)
            .Select(p => new ProdutoEstoqueResponse
            {
                CodigoProduto = p.CodigoProduto!,
                DescricaoProduto = p.Descricao!,
                Estoque = p.Estoque
            })
            .ToListAsync();
    }
}