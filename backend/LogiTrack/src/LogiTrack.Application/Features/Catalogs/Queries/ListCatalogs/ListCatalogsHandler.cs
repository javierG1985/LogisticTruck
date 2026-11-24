using LogiTrack.Application.Common.Abstractions;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Features.Catalogs.Queries.ListCatalogs;

public sealed class ListCatalogsHandler(IApplicationDbContext db) : IQueryHandler<ListCatalogsQuery, CatalogsResponse>
{
    public async ValueTask<CatalogsResponse> Handle(ListCatalogsQuery query, CancellationToken cancellationToken)
    {
        var items = await db.CatalogItems
            .AsNoTracking()
            .OrderBy(item => item.CatalogCode)
            .ThenBy(item => item.SortOrder)
            .Select(item => new { item.CatalogCode, item.Code, item.Label })
            .ToListAsync(cancellationToken);

        var catalogs = items
            .GroupBy(item => item.CatalogCode)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<CatalogItemResponse>)group
                    .Select(item => new CatalogItemResponse(item.Code, item.Label))
                    .ToList());

        return new CatalogsResponse(catalogs);
    }
}
