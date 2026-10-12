using ESG.GestaoResiduos.API.Common.Pagination;
using ESG.GestaoResiduos.API.Models;

namespace ESG.GestaoResiduos.API.ViewModels;

public class AlertaFiltro : PaginationParams
{
    public StatusAlerta? Status { get; set; }
    public int? IdPonto { get; set; }
}

public class AlertaResponse
{
    public int IdAlerta { get; set; }
    public int IdPonto { get; set; }
    public string PontoNome { get; set; } = string.Empty;
    public DateTime DataAlerta { get; set; }
    public decimal PercentualOcupacao { get; set; }
    public string StatusAlerta { get; set; } = string.Empty;
    public string? Mensagem { get; set; }
}

/// <summary>Resultado do atendimento de um alerta (coleta realizada).</summary>
public class AtenderAlertaResponse
{
    public int IdAlerta { get; set; }
    public string StatusAlerta { get; set; } = string.Empty;
    public int IdPonto { get; set; }
    public decimal QtdeAtualPonto { get; set; }
    public string StatusPonto { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
}
