using System.Diagnostics;

namespace Metalhead.Examples.ProblemDetailsProblem.Api;

public class ProblemDetailsMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext httpContext)
    {
        await _next(httpContext);

        if (httpContext.Response.HasStarted)
            return;

        var problemDetailsContext = new ProblemDetailsContext { HttpContext = httpContext };
        problemDetailsContext.ProblemDetails.Status = httpContext.Response.StatusCode;
        problemDetailsContext.ProblemDetails.Extensions = new Dictionary<string, object?>
        {
            { "traceId", Activity.Current?.Id ?? httpContext.TraceIdentifier }
        };

        if (httpContext.Features.Get<MathErrorFeature>() is { } mathErrorFeature)
        {
            mathErrorFeature.SetProblemDetails(problemDetailsContext);
        }

        try
        {
            await Results.Problem(problemDetailsContext.ProblemDetails).ExecuteAsync(httpContext);
        }
        catch (Exception)
        {
            // Cannot write problem details to the response.  This can happen if the media types in
            // the Accept header do not include a supported media type for this problem details writer,
            // e.g. a subset of 'application/problem+json' or 'application/json'.
            // Write a fallback message without the problem details writer.
            httpContext.Response.ContentType = System.Net.Mime.MediaTypeNames.Text.Plain;

            await httpContext.Response.WriteAsync(
                HttpHelper.CreateProblemDetailsAsString(problemDetailsContext.ProblemDetails));
        }
    }
}