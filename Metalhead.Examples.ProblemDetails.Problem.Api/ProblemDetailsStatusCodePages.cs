using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;

namespace Metalhead.Examples.ProblemDetailsProblem.Api;

public class ProblemDetailsStatusCodePages
{
    public static async Task HandleStatusCodeAsync(StatusCodeContext statusCodeContext)
    {
        var httpContext = statusCodeContext.HttpContext;
        if (httpContext.Response.HasStarted)
            return;

        var problemDetailsContext = new ProblemDetailsContext { HttpContext = httpContext };
        problemDetailsContext.ProblemDetails.Status = httpContext.Response.StatusCode;
        problemDetailsContext.ProblemDetails.Instance = $"{httpContext.Request.Path}{httpContext.Request.QueryString}";
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
            // Cannot write problem details to the response.  Write a fallback message in plain text.
            httpContext.Response.ContentType = System.Net.Mime.MediaTypeNames.Text.Plain;

            await httpContext.Response.WriteAsync(
                HttpHelper.CreateProblemDetailsAsString(problemDetailsContext.ProblemDetails));
        }
    }
}
