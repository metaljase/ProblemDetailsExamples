# What is ProblemDetailsExamples?

ProblemDetailsExamples contains three ASP.NET Core 8 projects, each demonstrating a different technique how HTTP APIs can generate HTTP error responses using the [Problem Details RFC 7807 specification](https://tools.ietf.org/html/rfc7807).

Projects in the solution:
- `Metalhead.Examples.ProblemDetails.Problem.Api` Writes problem details using [`Microsoft.AspNetCore.Http.Results.Problem`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.results.problem?view=aspnetcore-8.0).
- `Metalhead.Examples.ProblemDetails.Service.Api` Writes problem details using [`Microsoft.AspNetCore.Http.IProblemDetailsService`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.iproblemdetailsservice?view=aspnetcore-8.0).
- `Metalhead.Examples.ProblemDetails.Writer.Api` Writes problem details using a custom [`Microsoft.AspNetCore.Http.IProblemDetailsWriter`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.iproblemdetailswriter?view=aspnetcore-8.0).

# How does it work?

A problem details object can be written to the HTTP response in various contexts such as exception handlers, middleware, and within the actual endpoint methods themselves.  All three projects write problem details in their exception handlers and middleware.

Actually, each project contains two middleware components that write problem details, but they do the same thing for demonstration purposes, so don't use them simultaneously!  To switch between them, comment out either `app.UseMiddleware<ProblemDetailsMiddleware>();` or `app.UseStatusCodePages(ProblemDetailsStatusCodePages.HandleStatusCodeAsync);` in `Program.cs`.

Also, all projects contain a controller endpoint (/api/v1/math/workaroundresponse) that writes problem details in the actual endpoint itself using [`Microsoft.AspNetCore.Mvc.ControllerBase.Problem`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.problem?view=aspnetcore-8.0).  This is a workaround for the issue where returning a `BadRequestResult` in a controller endpoint (/api/v1/math/defectiveresponse) prevents a custom problem details object being written to the response, due to `BadRequestResult` writing to the response.

The projects write log events to the console and file(s) using [Serilog](https://serilog.net/).  App settings contains configuration for Serilog, which can be adjusted if necessary.  Trace IDs are written to the logs, which can be used to correlate with the Trace IDs that are written to problem details.

Swagger can be used in the development environment to test the endpoints and view the problem details that are written to the response.  However, there are many examples in the project's `<project name>.http` file that demonstrate the various response outputs from the endpoints.

# Setup instructions
1. Clone the ProblemDetailsExamples repository.
2. Open the .NET solution in Visual Studio 2022 (or a compatible alternative).
3. Update `appsettings.json` and `appsettings.Development.json` if necessary, e.g. path to log file.
4. Set one of the above projects as the startup project.
5. Build the solution and run!
6. Use Swagger to test the endpoints in the development environment, or use the `<project name>.http` files to test the endpoints.
