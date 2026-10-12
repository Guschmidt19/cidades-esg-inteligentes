namespace ESG.GestaoResiduos.API.Models;

/// <summary>
/// Representa a entidade geradora de resíduos (pessoa física ou jurídica)
/// cadastrada no sistema, usada para rastreabilidade e responsabilização ambiental.
/// </summary>
public class GeradorResiduo
{
    public int IdGerador { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoGerador Tipo { get; set; }
    public string Documento { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public ICollection<Descarte> Descartes { get; set; } = new List<Descarte>();
}
