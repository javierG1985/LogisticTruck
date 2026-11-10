using FluentValidation;
using LogiTrack.Application.Common.Pagination;

namespace LogiTrack.Application.Features.Drivers.Queries.ListDrivers;

public sealed class ListDriversValidator : AbstractValidator<ListDriversQuery>
{
    public ListDriversValidator()
    {
        Include(new PagedRequestValidator<ListDriversQuery>());

        RuleFor(query => query.Search)
            .MaximumLength(100)
            .WithMessage("La búsqueda no puede superar 100 caracteres.");

        RuleFor(query => query.LicenseCategory)
            .IsInEnum()
            .When(query => query.LicenseCategory is not null)
            .WithMessage("La categoría de licencia no es válida.");

        RuleFor(query => query.Sort)
            .Must(DriverSorts.Map.Accepts)
            .WithMessage($"El orden solo admite {string.Join(", ", DriverSorts.Map.FieldNames)}, con '-' para descendente.");
    }
}
