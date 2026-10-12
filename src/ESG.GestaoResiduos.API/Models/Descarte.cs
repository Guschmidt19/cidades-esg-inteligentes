namespace ESG.GestaoResiduos.API.Models;

/// <summary>
/// Registra cada operação de descarte realizada por um gerador em um ponto de coleta.
/// </summary>
public class Descarte
{
    public int IdDescarte { get; set; }
    public int IdPonto { get; set; }
    public int IdGerador { get; set; }
    public DateTime DataDescarte { get; set; } = DateTime.UtcNow;
    public TipoResiduo TipoResiduo { get; set; }
    public decimal PesoKg { get; set; }

    public PontoColeta? Ponto { get; set; }
    public GeradorResiduo? Gerador { get; set; }
}
