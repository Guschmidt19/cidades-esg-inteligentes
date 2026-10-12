namespace ESG.GestaoResiduos.API.Models;

/// <summary>
/// Consolida relatórios mensais de conformidade ambiental por ponto de coleta.
/// Utilizado em auditorias e análises de desempenho ESG (pilar Governance).
/// </summary>
public class RelatorioCompliance
{
    public int IdRelatorio { get; set; }
    public int IdPonto { get; set; }
    public int MesReferencia { get; set; }
    public int AnoReferencia { get; set; }
    public decimal TotalKgColetado { get; set; }
    public int QtdDescartes { get; set; }
    public bool Conforme { get; set; } = true;
    public DateTime DataGeracao { get; set; } = DateTime.UtcNow;

    public PontoColeta? Ponto { get; set; }
}
