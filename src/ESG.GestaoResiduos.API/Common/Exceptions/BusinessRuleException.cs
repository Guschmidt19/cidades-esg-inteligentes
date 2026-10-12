namespace ESG.GestaoResiduos.API.Common.Exceptions;

/// <summary>Violação de regra de negócio (mapeado para HTTP 422).</summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
