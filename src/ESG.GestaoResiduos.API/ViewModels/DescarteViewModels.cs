using System.ComponentModel.DataAnnotations;
using ESG.GestaoResiduos.API.Models;

namespace ESG.GestaoResiduos.API.ViewModels;

/// <summary>Dados para registrar um novo descarte (endpoint crítico, exige JWT).</summary>
public class RegistrarDescarteRequest
{
    [Required(ErrorMessage = "O ponto de coleta é obrigatório.")]
    [Range(1, int.MaxValue, ErrorMessage = "idPonto inválido.")]
    public int IdPonto { get; set; }

    [Required(ErrorMessage = "O gerador é obrigatório.")]
    [Range(1, int.MaxValue, ErrorMessage = "idGerador inválido.")]
    public int IdGerador { get; set; }

    [Required(ErrorMessage = "O tipo de resíduo é obrigatório.")]
    [EnumDataType(typeof(TipoResiduo), ErrorMessage = "Tipo de resíduo inválido.")]
    public TipoResiduo TipoResiduo { get; set; }

    [Range(0.01, 1_000_000, ErrorMessage = "O peso deve ser maior que zero.")]
    public decimal PesoKg { get; set; }
}

public class DescarteResponse
{
    public int IdDescarte { get; set; }
    public int IdPonto { get; set; }
    public string PontoNome { get; set; } = string.Empty;
    public int IdGerador { get; set; }
    public string GeradorNome { get; set; } = string.Empty;
    public DateTime DataDescarte { get; set; }
    public string TipoResiduo { get; set; } = string.Empty;
    public decimal PesoKg { get; set; }
}

/// <summary>
/// Resultado do registro de descarte, expondo os efeitos colaterais automatizados
/// (regras de negócio que substituem os triggers PL/SQL do projeto anterior).
/// </summary>
public class DescarteResultadoResponse
{
    public DescarteResponse Descarte { get; set; } = new();
    public decimal QtdeAtualPonto { get; set; }
    public decimal PercentualOcupacao { get; set; }
    public string StatusPonto { get; set; } = string.Empty;
    public bool AlertaGerado { get; set; }
    public bool PontoFicouCheio { get; set; }
    public string Mensagem { get; set; } = string.Empty;
}
