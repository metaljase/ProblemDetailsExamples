using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;

namespace Metalhead.Examples.ProblemDetailsWriter.Api;

public class ProblemDetailsExceptionHandler(IProblemDetailsWriter problemDetailsWriter) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // The Problem Details service has not been added, which facilitates the customisation of
        // ProblemDetails, e.g. to add a trace ID.  So add it here instead; however, it will not
        // be added for the development environment because it does not use this exception handler.
        var exceptionHandlerFeature = httpContext.Features.Get<IExceptionHandlerFeature>();
        var errorMessage = exceptionHandlerFeature?.Error?.Message;
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        
        if (problemDetailsWriter is not null && problemDetailsWriter.CanWrite(new ProblemDetailsContext() { HttpContext = httpContext }))
        {
            await problemDetailsWriter.WriteAsync(new ProblemDetailsContext
            {
                AdditionalMetadata = exceptionHandlerFeature?.Endpoint?.Metadata,
                HttpContext = httpContext,
                ProblemDetails =
                {
                    Detail = errorMessage,
                    Extensions = { ["traceId"] = traceId }
                },
                Exception = exception
            });
            return true;
        }

        // This exception handler could not handle the exception.
        return false;
    }
}
