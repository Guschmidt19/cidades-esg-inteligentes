using System.ComponentModel.DataAnnotations;

namespace ESG.GestaoResiduos.API.Common.Pagination;

/// <summary>
/// Parâmetros de paginação recebidos via query string nos endpoints de listagem.
/// Limita o tamanho da página para proteger o desempenho da aplicação.
/// </summary>
public class PaginationParams
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;
    private int _pageNumber = 1;

    /// <summary>Número da página (base 1).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "pageNumber deve ser maior ou igual a 1.")]
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>Quantidade de itens por página (máximo 100).</summary>
    [Range(1, MaxPageSize, ErrorMessage = "pageSize deve estar entre 1 e 100.")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 1 : value);
    }
}
