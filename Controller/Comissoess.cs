using APIregistroDeVenda.DTO;
using APIregistroDeVenda.Service;
using Microsoft.AspNetCore.Mvc;

namespace APIregistroDeVenda.Controller;

[ApiController]
[Route("api/comissoes")]
public class Comissoes : ControllerBase
{
    private readonly ComissaoService _service;

    public Comissoes(ComissaoService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Cadastrar(
        [FromBody] ComissaoCalcularRequest request)
    {
        int quantidade = await _service.CadastrarAsync(request);

        return StatusCode(StatusCodes.Status201Created, new
        {
            mensagem = "Vendas cadastradas com sucesso.",
            quantidadeVendas = quantidade
        });
    }

    [HttpGet]
    public async Task<ActionResult<VendasResponse>> Consultar()
    {
        return Ok(await _service.ListarVendasAsync());
    }

    [HttpGet("faixas")]
    public async Task<ActionResult<List<ComissaoFaixaResponse>>>
        ConsultarPorFaixa()
    {
        return Ok(await _service.ConsultarPorFaixaAsync());
    }

  
    [HttpGet("{id}")]
    public async Task<ActionResult<VendaComissaoResponse>> ConsultarPorId(
    [FromRoute] int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                mensagem = "O ID deve ser um número inteiro maior que zero."
            });
        }

        var resultado = await _service.ConsultarPorIdAsync(id);

        if (resultado is null)
        {
            return NotFound(new
            {
                mensagem = "Venda não encontrada."
            });
        }

        return Ok(resultado);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<VendaComissaoResponse>> Atualizar(
        [FromRoute] int id,
        [FromBody] VendaComissaoAtualizarRequest request)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                mensagem = "O ID deve ser um número inteiro maior que zero."
            });
        }

        var resultado = await _service.AtualizarAsync(id, request);

        if (resultado is null)
            return NotFound(new { mensagem = "Venda não encontrada." });

        return Ok(resultado);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Excluir([FromRoute] int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                mensagem = "O ID deve ser um número inteiro maior que zero."
            });
        }

        bool excluiu = await _service.ExcluirAsync(id);

        if (!excluiu)
            return NotFound(new { mensagem = "Venda não encontrada." });

        return NoContent();
    }
}