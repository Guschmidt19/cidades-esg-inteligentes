namespace ESG.GestaoResiduos.API.Models;

/// <summary>Tipo do gerador de resíduo: Pessoa Física ou Jurídica.</summary>
public enum TipoGerador
{
    PF,
    PJ
}

/// <summary>Tipos de resíduo aceitos pelos pontos de coleta.</summary>
public enum TipoResiduo
{
    PLASTICO,
    PAPEL,
    VIDRO,
    METAL,
    ORGANICO,
    ELETRONICO
}

/// <summary>Status operacional do ponto de coleta.</summary>
public enum StatusPonto
{
    ATIVO,
    INATIVO,
    CHEIO
}

/// <summary>Status de um alerta de capacidade.</summary>
public enum StatusAlerta
{
    PENDENTE,
    ATENDIDO
}
