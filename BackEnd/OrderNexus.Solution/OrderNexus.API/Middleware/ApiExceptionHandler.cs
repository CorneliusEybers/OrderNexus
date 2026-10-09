using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrderNexus.Application.Exceptions;

namespace OrderNexus.API.Middleware
{
    /// <summary>Provides safe HTTP errors with detailed internal diagnostics.</summary>
    public sealed class ApiExceptionHandler : IExceptionHandler
    {
        #region Class Variables
        private readonly ILogger<ApiExceptionHandler> _logger;
        #endregion

        #region Constructor
        public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) => _logger = logger;
        #endregion

        #region Public Methods
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
            CancellationToken cancellationToken)
        {
            int status = exception is BusinessException business ? business.StatusCode : 500;
            if (exception is DbUpdateException dbError && dbError.InnerException is SqliteException sqlite &&
                sqlite.SqliteErrorCode == 19 && sqlite.SqliteExtendedErrorCode is 2067 or 1555)
                status = 409;

            string traceId = httpContext.TraceIdentifier;
            System.Reflection.MethodBase? method = exception.TargetSite;
            _logger.LogError(exception,
                "Request failure AssemblyName={AssemblyName} MethodName={MethodName} ExceptionType={ExceptionType} ExceptionMessage={ExceptionMessage} TraceId={TraceId} Path={Path}",
                method?.DeclaringType?.Assembly.GetName().Name ?? "Unknown",
                method is null ? "Unknown" : $"{method.DeclaringType?.FullName}.{method.Name}",
                exception.GetType().FullName, exception.Message, traceId, httpContext.Request.Path);

            var problem = new ProblemDetails
            {
                Status = status,
                Title = status switch { 400 => "Invalid request", 404 => "Not found", 409 => "Conflict", _ => "Server error" },
                Detail = exception is BusinessException ? exception.Message : status == 409
                    ? "This operation conflicts with existing data. Refresh and retry." : "An unexpected error occurred. Contact support with the trace ID.",
                Instance = httpContext.Request.Path
            };
            problem.Extensions["traceId"] = traceId;
            httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }
        #endregion
    }
}
