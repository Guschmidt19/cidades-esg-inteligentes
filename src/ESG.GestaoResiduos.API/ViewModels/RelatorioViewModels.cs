using ESG.GestaoResiduos.API.Common.Pagination;

namespace ESG.GestaoResiduos.API.ViewModels;

public class RelatorioFiltro : PaginationParams
{
    public int? Ano { get; set; }
    public int? Mes { get; set; }
    public int? IdPonto { get; set; }
    public bool? Conforme { get; set; }
}

public class RelatorioComplianceResponse
{
    public int IdRelatorio { get; set; }
    public int IdPonto { get; set; }
    public string PontoNome { get; set; } = string.Empty;
    public int MesReferencia { get; set; }
    public int AnoReferencia { get; set; }
    public decimal TotalKgColetado { get; set; }
    public int QtdDescartes { get; set; }
    public bool Conforme { get; set; }
    public DateTime DataGeracao { get; set; }
}

/// <summary>Consolidação agregada de compliance por período (otimizada via GROUP BY no banco).</summary>
public class RelatorioConsolidadoResponse
{
    public int AnoReferencia { get; set; }
    public int MesReferencia { get; set; }
    public decimal TotalKgColetado { get; set; }
    public int QtdDescartes { get; set; }
    public int QtdRelatorios { get; set; }
    public int QtdConformes { get; set; }
    public int QtdNaoConformes { get; set; }
    public decimal PercentualConformidade { get; set; }
}
