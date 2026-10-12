using ESG.GestaoResiduos.API.Common.Pagination;
using ESG.GestaoResiduos.API.Services.Interfaces;
using ESG.GestaoResiduos.API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESG.GestaoResiduos.API.Controllers;

/// <summary>Consulta de pontos de coleta com filtros, ordenação por ocupação e paginação.</summary>
[ApiController]
[Route("api/pontos-coleta")]
[Produces("application/json")]
public class PontosColetaController : ControllerBase
{
    private readonly IPontoColetaService _service;

    public PontosColetaController(IPontoColetaService service) => _service = service;

    /// <summary>Lista pontos de coleta paginados, com filtros por status, tipo, busca e ocupação mínima.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<PontoColetaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PontoColetaResponse>>> Listar(
        [FromQuery] PontoColetaFiltro filtro,
        CancellationToken ct)
    {
        var resultado = await _service.ListarAsync(filtro, ct);
        return Ok(resultado);
    }

    /// <summary>Obtém um ponto de coleta específico com seu percentual de ocupação atual.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PontoColetaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PontoColetaResponse>> ObterPorId(int id, CancellationToken ct)
    {
        var ponto = await _service.ObterPorIdAsync(id, ct);
        return Ok(ponto);
    }
}
