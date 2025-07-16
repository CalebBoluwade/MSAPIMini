using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Serilog.Context;

namespace MS.API.Mini.Middleware
{
    public abstract class NotFoundException(string message) : Exception(message);

    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> exLogger): IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken){
            exLogger.LogError(exception,"Exception Details");

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status412PreconditionFailed,
                Title = "An Error Occured",
                Detail = exception.Message
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }

    public class ErrorHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> exLogger)
    {
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                using (LogContext.PushProperty("CorrelationId", httpContext.TraceIdentifier))
                {
                    await next(httpContext);
                }
            }
            catch (NotFoundException ex)
            {
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await httpContext.Response.WriteAsJsonAsync(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                // Handle unauthorized access
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsJsonAsync("Unauthorized access.");
            }
            catch (Exception ex)
            {
                exLogger.LogError(ex, "Something went wrong");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var response = new
            {
                context.Response.StatusCode,
                Message = "An Error Occured",
                Detailed = exception.Message
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}