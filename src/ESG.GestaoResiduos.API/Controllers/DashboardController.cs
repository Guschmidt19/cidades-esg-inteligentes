using ESG.GestaoResiduos.API.Services.Interfaces;
using ESG.GestaoResiduos.API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESG.GestaoResiduos.API.Controllers;

/// <summary>Indicadores ESG consolidados para visão gerencial.</summary>
[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    /// <summary>Retorna métricas agregadas: pontos, alertas, coleta por tipo e pontos críticos.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardResponse>> Obter(CancellationToken ct)
    {
        var resultado = await _service.ObterAsync(ct);
        return Ok(resultado);
    }
}
