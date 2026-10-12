using ESG.GestaoResiduos.API.Common.Pagination;
using ESG.GestaoResiduos.API.Services.Interfaces;
using ESG.GestaoResiduos.API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESG.GestaoResiduos.API.Controllers;

/// <summary>
/// Operações de descarte de resíduos. O registro de descarte é o endpoint crítico
/// do sistema (exige JWT) e dispara as automações de compliance.
/// </summary>
[ApiController]
[Route("api/descartes")]
[Produces("application/json")]
public class DescartesController : ControllerBase
{
    private readonly IDescarteService _service;

    public DescartesController(IDescarteService service) => _service = service;

    /// <summary>Lista descartes de forma paginada, opcionalmente filtrando por ponto de coleta.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<DescarteResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<DescarteResponse>>> Listar(
        [FromQuery] PaginationParams pagination,
        [FromQuery] int? idPonto,
        CancellationToken ct)
    {
        var resultado = await _service.ListarAsync(pagination, idPonto, ct);
        return Ok(resultado);
    }

    /// <summary>
    /// Registra um novo descarte (endpoint crítico — requer autenticação). Aplica as
    /// regras automáticas: atualização de saldo, alerta aos 80%, status CHEIO e relatório mensal.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(DescarteResultadoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<DescarteResultadoResponse>> Registrar(
        [FromBody] RegistrarDescarteRequest request,
        CancellationToken ct)
    {
        var resultado = await _service.RegistrarAsync(request, ct);
        return CreatedAtAction(nameof(Listar), null, resultado);
    }
}
