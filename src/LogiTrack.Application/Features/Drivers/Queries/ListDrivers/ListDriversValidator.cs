using FluentValidation;
using LogiTrack.Application.Common.Pagination;

namespace LogiTrack.Application.Features.Drivers.Queries.ListDrivers;

public sealed class ListDriversValidator : AbstractValidator<ListDriversQuery>
{
    private static readonly string[] SortableFields = ["firstName", "lastName", "documentNumber", "status", "licenseExpiresOn", "createdAt"];

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
            .Must(BeSortable)
            .WithMessage("El orden solo admite firstName, lastName, documentNumber, status, licenseExpiresOn o createdAt, con '-' para descendente.");
    }

    private static bool BeSortable(string? sort) =>
        sort is null
        || sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .All(token => SortableFields.Contains(token.TrimStart('-')));
}
