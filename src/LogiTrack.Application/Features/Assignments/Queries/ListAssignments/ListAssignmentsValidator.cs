using FluentValidation;
using LogiTrack.Application.Common.Pagination;

namespace LogiTrack.Application.Features.Assignments.Queries.ListAssignments;

public sealed class ListAssignmentsValidator : AbstractValidator<ListAssignmentsQuery>
{
    public ListAssignmentsValidator()
    {
        Include(new PagedRequestValidator<ListAssignmentsQuery>());

        RuleFor(query => query.Sort)
            .Must(AssignmentSorts.Map.Accepts)
            .WithMessage($"El orden solo admite {string.Join(", ", AssignmentSorts.Map.FieldNames)}, con '-' para descendente.");
    }
}
