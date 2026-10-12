namespace ESG.GestaoResiduos.API.Models;

/// <summary>
/// Alerta gerado automaticamente quando um ponto de coleta atinge ou supera
/// 80% da capacidade máxima. Base para acionamento da equipe de coleta.
/// </summary>
public class AlertaColeta
{
    public int IdAlerta { get; set; }
    public int IdPonto { get; set; }
    public DateTime DataAlerta { get; set; } = DateTime.UtcNow;
    public decimal PercentualOcupacao { get; set; }
    public StatusAlerta StatusAlerta { get; set; } = StatusAlerta.PENDENTE;
    public string? Mensagem { get; set; }

    public PontoColeta? Ponto { get; set; }
}
