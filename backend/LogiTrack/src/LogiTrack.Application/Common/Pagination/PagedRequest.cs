namespace LogiTrack.Application.Common.Pagination;

// Base de las consultas paginadas: los valores vienen del query string y se validan en PagedRequestValidator.
public abstract record PagedRequest
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
