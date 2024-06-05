using Microsoft.Net.Http.Headers;

namespace Metalhead.Examples.ProblemDetailsWriter.Api;

public class ProblemDetailsWriter : IProblemDetailsWriter
{
    private static readonly MediaTypeHeaderValue s_jsonMediaType = new(System.Net.Mime.MediaTypeNames.Application.Json);
    private static readonly MediaTypeHeaderValue s_problemDetailsJsonMediaType = new(System.Net.Mime.MediaTypeNames.Application.ProblemJson);

    public bool CanWrite(ProblemDetailsContext context)
    {
        var acceptHeader = new List<MediaTypeHeaderValue>();
        var acceptedMimeTypes = HttpHelper.ParseAcceptHeader(context.HttpContext.Request);
        foreach (var acceptedMimeType in acceptedMimeTypes)
        {
            acceptHeader.Add(new MediaTypeHeaderValue(acceptedMimeType));
        }

        // A request without the Accept header implies that the user agent will accept any media type in response.
        if (acceptHeader.Count == 0)
        {
            return true;
        }

        for (var i = 0; i < acceptHeader.Count; i++)
        {
            var acceptHeaderValue = acceptHeader[i];

            if (s_jsonMediaType.IsSubsetOf(acceptHeaderValue) || s_problemDetailsJsonMediaType.IsSubsetOf(acceptHeaderValue))
            {
                return true;
            }
        }

        return false;
    }

    public ValueTask WriteAsync(ProblemDetailsContext context)
    {
        var httpContext = context.HttpContext;
        ProblemDetailsDefaults.Apply(context.ProblemDetails, httpContext.Response.StatusCode);

        return new ValueTask(httpContext.Response.WriteAsJsonAsync(
            context.ProblemDetails,
            options: null,
            contentType: System.Net.Mime.MediaTypeNames.Application.ProblemJson));
    }
}
