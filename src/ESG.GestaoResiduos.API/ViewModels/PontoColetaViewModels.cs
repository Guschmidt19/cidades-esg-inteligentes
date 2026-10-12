using ESG.GestaoResiduos.API.Common.Pagination;
using ESG.GestaoResiduos.API.Models;

namespace ESG.GestaoResiduos.API.ViewModels;

/// <summary>Filtros opcionais combinados com paginação no endpoint de pontos de coleta.</summary>
public class PontoColetaFiltro : PaginationParams
{
    public StatusPonto? Status { get; set; }
    public TipoResiduo? TipoResiduo { get; set; }
    public string? Busca { get; set; }
    public decimal? OcupacaoMinima { get; set; }
}

public class PontoColetaResponse
{
    public int IdPonto { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Localizacao { get; set; } = string.Empty;
    public string TipoResiduo { get; set; } = string.Empty;
    public decimal CapacidadeMaxKg { get; set; }
    public decimal QtdeAtualKg { get; set; }
    public decimal PercentualOcupacao { get; set; }
    public string Status { get; set; } = string.Empty;
}
