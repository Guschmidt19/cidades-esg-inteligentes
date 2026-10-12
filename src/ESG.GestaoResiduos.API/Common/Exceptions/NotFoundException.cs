namespace ESG.GestaoResiduos.API.Common.Exceptions;

/// <summary>Recurso não encontrado (mapeado para HTTP 404).</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
