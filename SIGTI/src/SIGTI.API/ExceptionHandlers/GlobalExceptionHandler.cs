using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SIGTI.Application.Common.Exceptions;
using SIGTI.Domain.Exceptions;

namespace SIGTI.API.ExceptionHandlers
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken
        )
        {
            var problem = exception switch
            {
                NotFoundException => new ProblemDetails
                {
                    Title = "Recurso não encontrado",
                    Status = StatusCodes.Status404NotFound,
                    Detail = exception.Message,
                },
                DomainException => new ProblemDetails
                {
                    Title = "Erro de regra de negócio",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = exception.Message,
                },
                ValidationException => new ProblemDetails
                {
                    Title = "Erro de validação",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = exception.Message,
                },
                UnauthorizedException => new ProblemDetails
                {
                    Title = "Não autorizado",
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = exception.Message,
                },
                _ => null,
            };

            if (problem is null)
            {
                _logger.LogError(exception, "Erro inesperado na aplicação.");
                return false;
            }

            _logger.LogWarning(
                exception,
                "Exceção tratada: {Message}",
                exception.Message
            );

            httpContext.Response.StatusCode =
                problem.Status ?? StatusCodes.Status500InternalServerError;

            problem.Extensions["traceId"] = httpContext.TraceIdentifier;

            await httpContext.Response.WriteAsJsonAsync(
                problem,
                cancellationToken
            );

            return true;
        }
    }
}
