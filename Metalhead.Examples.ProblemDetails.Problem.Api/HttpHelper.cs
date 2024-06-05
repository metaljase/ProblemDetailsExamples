using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Metalhead.Examples.ProblemDetailsProblem.Api;

public class HttpHelper
{
    public static IEnumerable<string> ParseAcceptHeader(HttpRequest request)
    {
        var acceptHeader = request.Headers.Accept.ToString();

        if (string.IsNullOrWhiteSpace(acceptHeader))
        {
            return [];
        }

        var mimeTypes = acceptHeader
            .Split(',')
            .Select(part => part.Split(';')[0].Trim())
            .Where(mimeType => !string.IsNullOrWhiteSpace(mimeType));

        return mimeTypes;
    }

    public static string CreateProblemDetailsAsString(ProblemDetails problemDetails)
    {
        return CreateProblemDetailsAsString(problemDetails, problemDetails.Status);
    }

    public static string CreateProblemDetailsAsString(ProblemDetails problemDetails, int? statusCode)
    {
        ProblemDetailsDefaults.Apply(problemDetails, statusCode);

        StringBuilder stringBuilder = new();
        if (problemDetails.Type is not null)
        {
            stringBuilder.AppendLine($"type: {problemDetails.Type}");
        }
        if (problemDetails.Title is not null)
        {
            stringBuilder.AppendLine($"title: {problemDetails.Title}");
        }
        stringBuilder.AppendLine($"status: {problemDetails.Status}");
        if (problemDetails.Detail is not null)
        {
            stringBuilder.AppendLine($"detail: {problemDetails.Detail}");
        }
        if (problemDetails.Instance is not null)
        {
            stringBuilder.AppendLine($"instance: {problemDetails.Instance}");
        }
        foreach (var extension in problemDetails.Extensions)
        {
            stringBuilder.AppendLine($"{extension.Key}: {extension.Value}");
        }

        return stringBuilder.ToString();
    }
}
