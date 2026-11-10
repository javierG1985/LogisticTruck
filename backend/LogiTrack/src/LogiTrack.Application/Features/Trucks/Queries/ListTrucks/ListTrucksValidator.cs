using FluentValidation;
using LogiTrack.Application.Common.Pagination;

namespace LogiTrack.Application.Features.Trucks.Queries.ListTrucks;

public sealed class ListTrucksValidator : AbstractValidator<ListTrucksQuery>
{
    public ListTrucksValidator()
    {
        Include(new PagedRequestValidator<ListTrucksQuery>());

        RuleFor(query => query.Search)
            .MaximumLength(100)
            .WithMessage("La búsqueda no puede superar 100 caracteres.");

        RuleFor(query => query.Sort)
            .Must(TruckSorts.Map.Accepts)
            .WithMessage($"El orden solo admite {string.Join(", ", TruckSorts.Map.FieldNames)}, con '-' para descendente.");
    }
}
