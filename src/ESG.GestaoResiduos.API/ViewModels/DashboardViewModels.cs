namespace ESG.GestaoResiduos.API.ViewModels;

public class DashboardResponse
{
    public int TotalPontos { get; set; }
    public int PontosAtivos { get; set; }
    public int PontosCheios { get; set; }
    public int AlertasPendentes { get; set; }
    public decimal TotalColetadoKg { get; set; }
    public decimal TaxaConformidade { get; set; }
    public IReadOnlyList<TotalPorTipoResiduo> ColetaPorTipo { get; set; } = new List<TotalPorTipoResiduo>();
    public IReadOnlyList<PontoCriticoResponse> PontosCriticos { get; set; } = new List<PontoCriticoResponse>();
}

public class TotalPorTipoResiduo
{
    public string TipoResiduo { get; set; } = string.Empty;
    public decimal TotalKg { get; set; }
    public int QtdDescartes { get; set; }
}

public class PontoCriticoResponse
{
    public int IdPonto { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal PercentualOcupacao { get; set; }
    public string Status { get; set; } = string.Empty;
}
