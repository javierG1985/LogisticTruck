using FluentValidation;
using LogiTrack.Application.Common.Exceptions;
using LogiTrack.Domain.SharedKernel;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Api.Http;

// Traduce excepciones a ProblemDetails (RFC 9457). Nunca expone el stack trace.
public sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            ValidationException validation => CreateValidationProblem(validation),
            NotFoundException notFound => CreateProblem(StatusCodes.Status404NotFound, "Recurso no encontrado", notFound.Message),
            ConflictException conflict => CreateProblem(StatusCodes.Status409Conflict, "Conflicto", conflict.Message),
            DomainException domain => CreateProblem(StatusCodes.Status409Conflict, "Regla de negocio incumplida", domain.Message),
            PreconditionFailedException precondition => CreateProblem(StatusCodes.Status412PreconditionFailed, "Precondición fallida", precondition.Message),
            PreconditionRequiredException required => CreateProblem(StatusCodes.Status428PreconditionRequired, "Precondición requerida", required.Message),
            DbUpdateConcurrencyException => CreateProblem(
                StatusCodes.Status412PreconditionFailed,
                "Precondición fallida",
                "El recurso fue modificado por otra operación. Vuelve a leerlo y reintenta."),
            _ => CreateUnexpectedProblem(exception),
        };

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem,
        });
    }

    private ProblemDetails CreateUnexpectedProblem(Exception exception)
    {
        LogUnhandledException(logger, exception);

        return CreateProblem(
            StatusCodes.Status500InternalServerError,
            "Error interno",
            "Ocurrió un error inesperado. Consulta los registros del servidor.");
    }

    private static ValidationProblemDetails CreateValidationProblem(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(error => ToCamelCase(error.PropertyName))
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Solicitud no válida",
            Detail = "Hay errores de validación en la solicitud.",
            Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
        };
    }

    private static ProblemDetails CreateProblem(int status, string title, string detail) =>
        new()
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = TypeFor(status),
        };

    // Enlace a la sección de la especificación que define cada código.
    private static string TypeFor(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
        StatusCodes.Status404NotFound => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5",
        StatusCodes.Status409Conflict => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.10",
        StatusCodes.Status412PreconditionFailed => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.13",
        StatusCodes.Status428PreconditionRequired => "https://datatracker.ietf.org/doc/html/rfc6585#section-3",
        _ => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1",
    };

    private static string ToCamelCase(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception);
}
