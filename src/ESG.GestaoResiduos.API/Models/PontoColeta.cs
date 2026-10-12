namespace ESG.GestaoResiduos.API.Models;

/// <summary>
/// Ponto de coleta seletiva. Entidade central do modelo: relaciona-se com
/// descartes, alertas e relatórios de compliance.
/// </summary>
public class PontoColeta
{
    public int IdPonto { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Localizacao { get; set; } = string.Empty;
    public TipoResiduo TipoResiduo { get; set; }
    public decimal CapacidadeMaxKg { get; set; }
    public decimal QtdeAtualKg { get; set; }
    public StatusPonto Status { get; set; } = StatusPonto.ATIVO;

    /// <summary>Percentual de ocupação atual (não persistido).</summary>
    public decimal PercentualOcupacao =>
        CapacidadeMaxKg > 0 ? Math.Round(QtdeAtualKg / CapacidadeMaxKg * 100m, 2) : 0m;

    public ICollection<Descarte> Descartes { get; set; } = new List<Descarte>();
    public ICollection<AlertaColeta> Alertas { get; set; } = new List<AlertaColeta>();
    public ICollection<RelatorioCompliance> Relatorios { get; set; } = new List<RelatorioCompliance>();
}
