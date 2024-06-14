﻿using System.Diagnostics;

namespace Metalhead.Examples.ProblemDetailsService.Api;

public class ProblemDetailsMiddleware(RequestDelegate next, IProblemDetailsService problemDetailsService)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext httpContext)
    {
        await _next(httpContext);

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

        if (problemDetailsService is null || !await problemDetailsService.TryWriteAsync(problemDetailsContext))
        {
            // Cannot write problem details to the response.  This can happen if the media types in the Accept header do not
            // include a supported media type for this problem details writer, e.g. a subset of 'application/problem+json'
            // or 'application/json'.  Therefore, write a fallback message in plain text instead.
            // NOTE: HTTP does allow the response to be written using a media type not specified in the request Accept header.
            // This can be achieved using the 'Microsoft.AspNetCore.Http.Results.Problem' method, which is demonstrated in the
            // 'Metalhead.Examples.ProblemDetails.Problem.Api.ProblemDetailsMiddleware' class.
            httpContext.Response.ContentType = System.Net.Mime.MediaTypeNames.Text.Plain;

            await httpContext.Response.WriteAsync(
                HttpHelper.CreateProblemDetailsAsString(problemDetailsContext.ProblemDetails));
        }
    }
}