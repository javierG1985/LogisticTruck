using FluentValidation;
using LogiTrack.Application.Common.Pagination;

namespace LogiTrack.Application.Modules.Fleet.Trucks.ListTrucks;

public sealed class ListTrucksValidator : AbstractValidator<ListTrucksQuery>
{
    private static readonly string[] SortableFields = ["plate", "brand", "modelYear", "capacityKg", "status", "createdAt"];

    public ListTrucksValidator()
    {
        Include(new PagedRequestValidator<ListTrucksQuery>());

        RuleFor(query => query.Search)
            .MaximumLength(100)
            .WithMessage("La búsqueda no puede superar 100 caracteres.");

        RuleFor(query => query.Sort)
            .Must(BeSortable)
            .WithMessage("El orden solo admite plate, brand, modelYear, capacityKg, status o createdAt, con '-' para descendente.");
    }

    private static bool BeSortable(string? sort) =>
        sort is null
        || sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .All(token => SortableFields.Contains(token.TrimStart('-')));
}
