using ESG.GestaoResiduos.API.Common.Pagination;
using ESG.GestaoResiduos.API.Services.Interfaces;
using ESG.GestaoResiduos.API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESG.GestaoResiduos.API.Controllers;

/// <summary>Relatórios de compliance ambiental (pilar Governance do ESG).</summary>
[ApiController]
[Route("api/relatorios-compliance")]
[Produces("application/json")]
public class RelatoriosComplianceController : ControllerBase
{
    private readonly IRelatorioService _service;

    public RelatoriosComplianceController(IRelatorioService service) => _service = service;

    /// <summary>Lista relatórios mensais de compliance de forma paginada e filtrável.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<RelatorioComplianceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<RelatorioComplianceResponse>>> Listar(
        [FromQuery] RelatorioFiltro filtro,
        CancellationToken ct)
    {
        var resultado = await _service.ListarAsync(filtro, ct);
        return Ok(resultado);
    }

    /// <summary>Retorna a consolidação agregada por período (GROUP BY executado no banco).</summary>
    [HttpGet("consolidado")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<RelatorioConsolidadoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RelatorioConsolidadoResponse>>> Consolidado(
        [FromQuery] int? ano,
        CancellationToken ct)
    {
        var resultado = await _service.ConsolidarAsync(ano, ct);
        return Ok(resultado);
    }
}
