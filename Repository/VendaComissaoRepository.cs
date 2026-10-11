using APIregistroDeVenda.Context;
using APIregistroDeVenda.Domain;
using Microsoft.EntityFrameworkCore;

namespace APIregistroDeVenda.Repository;

public class VendaComissaoRepository : IVendaComissaoRepository
{
    private readonly AppDbContext _context;

    public VendaComissaoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(List<VendaComissao> vendas)
    {
        _context.VendasComissao.AddRange(vendas);

        await _context.SaveChangesAsync();
    }

    public async Task<List<VendaComissao>> ListarAsync()
    {
        return await _context.VendasComissao
            .AsNoTracking()
            .OrderBy(venda => venda.VendaComissaoId)
            .ToListAsync();
    }

    public async Task<VendaComissao?> ConsultarPorIdAsync(int id)
    {
        return await _context.VendasComissao
            .AsNoTracking()
            .SingleOrDefaultAsync(
                venda => venda.VendaComissaoId == id);
    }

    public async Task AtualizarAsync(VendaComissao venda)
    {
        _context.VendasComissao.Update(venda);

        await _context.SaveChangesAsync();
    }

    public async Task ExcluirAsync(VendaComissao venda)
    {
        _context.VendasComissao.Remove(venda);

        await _context.SaveChangesAsync();
    }
}