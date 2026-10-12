using ESG.GestaoResiduos.API.Common.Pagination;
using ESG.GestaoResiduos.API.Services.Interfaces;
using ESG.GestaoResiduos.API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESG.GestaoResiduos.API.Controllers;

/// <summary>Alertas de capacidade dos pontos de coleta e atendimento de coletas.</summary>
[ApiController]
[Route("api/alertas")]
[Produces("application/json")]
public class AlertasController : ControllerBase
{
    private readonly IAlertaService _service;

    public AlertasController(IAlertaService service) => _service = service;

    /// <summary>Lista alertas de forma paginada, filtrando por status e/ou ponto.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<AlertaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AlertaResponse>>> Listar(
        [FromQuery] AlertaFiltro filtro,
        CancellationToken ct)
    {
        var resultado = await _service.ListarAsync(filtro, ct);
        return Ok(resultado);
    }

    /// <summary>
    /// Atende um alerta registrando a coleta (endpoint crítico — requer autenticação):
    /// marca o alerta como ATENDIDO, libera a capacidade do ponto e o reativa.
    /// </summary>
    [HttpPut("{id:int}/atender")]
    [Authorize]
    [ProducesResponseType(typeof(AtenderAlertaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AtenderAlertaResponse>> Atender(int id, CancellationToken ct)
    {
        var resultado = await _service.AtenderAsync(id, ct);
        return Ok(resultado);
    }
}
