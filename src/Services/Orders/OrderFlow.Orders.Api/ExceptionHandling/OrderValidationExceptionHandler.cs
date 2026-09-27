using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Orders.Domain;

namespace OrderFlow.Orders.Api.ExceptionHandling;

internal sealed class OrderValidationExceptionHandler(
    ILogger<OrderValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not OrderValidationException validationException)
        {
            return false;
        }

        logger.LogInformation(exception, "Order request validation failed.");

        var problemDetails = new HttpValidationProblemDetails(validationException.Errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Request validation failed."
        };

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
