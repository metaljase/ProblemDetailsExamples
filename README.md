# What is ProblemDetailsExamples?

ProblemDetailsExamples contains three ASP.NET Core 8 projects, each demonstrating a different technique how HTTP APIs can generate HTTP error responses using the [Problem Details RFC 9457 specification](https://datatracker.ietf.org/doc/html/rfc9457).

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

# Endpoint examples

All three projects contain the following endpoints.  The input used in the endpoint examples below will result in an error response.

- `/api/v1/divide?numerator=1&denominator=0` & `/api/v1/math/divide/1/0` - Returns 400 Bad Request with 'feature' details.
- `/api/v1/squareroot?radicand=-1` & `/api/v1/math/squareroot/-1` - Returns 400 Bad Request without 'feature' details.
- `/api/v1/throwex` & `/api/v1/math/throwex` - Returns exception stack trace in development, otherwise 500 Internal Server Error.
- `/api/v1/math/defectiveresponse` - Returns 400 Bad Request with missing 'feature' details.
- `/api/v1/math/workaroundresponse` - Returns 400 Bad Request with 'feature' details.

# Response output

When an exception is thrown, it's bad practice to make the stack trace publicly  available, even the exception message in some cases.  Therefore, in all three projects, the stack trace is only written to the response when running in the development environment.

The response output varies depending on the technique used in each project, which may help you decide which technique is best suited for your project.

Some techniques require the request Accept header to contain a media type that is a subset of `application/problem+json` or `application/json` to write a problem details object to the response, as shown below:

| Problem details writer | Subset of application/json or application/problem+json required in request Accept header to generate problem details object |
|-|-|
| [`Microsoft.AspNetCore.Http.Results.Problem`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.results.problem?view=aspnetcore-8.0) | No |
| [`Microsoft.AspNetCore.Mvc.ControllerBase.Problem`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.problem?view=aspnetcore-8.0) | No |
| [`Microsoft.AspNetCore.Http.IProblemDetailsService`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.iproblemdetailsservice?view=aspnetcore-8.0) via controller endpoints<br>Uses&nbsp;`Microsoft.AspNetCore.Mvc.Infrastructure.DefaultApiProblemDetailsWriter` | No |
| [`Microsoft.AspNetCore.Http.IProblemDetailsService`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.iproblemdetailsservice?view=aspnetcore-8.0) via minimal API endpoints<br> Uses&nbsp;`Microsoft.AspNetCore.Http.DefaultProblemDetailsWriter` | Yes |

NOTE: [`Microsoft.AspNetCore.Http.IProblemDetailsWriter`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.iproblemdetailswriter?view=aspnetcore-8.0) is not included in the table above because it's used to write a custom implementation that can be written to generate problem details objects using your own requirements.

Considering the table above, the following table shows whether the response is written as a problem details object or a fallback in plain text:
<table><thead>
  <tr>
    <th rowspan="3">Project</th>
    <th colspan="4">Accept Header: application/json</th>
    <th colspan="4">Accept Header: image/jpeg</th>
  </tr>
  <tr>
    <th colspan="2">Status Codes<br>400-599</th>
    <th colspan="2">Unhandled<br>Exception</th>
    <th colspan="2">Status Codes<br>400-599<br></th>
    <th colspan="2">Unhandled<br>Exception<br></th>
  </tr>
  <tr>
    <th>Dev</th><th>Non-Dev<br></th><th>Dev</th><th>Non-Dev</th><th>Dev</th><th>Non-Dev</th><th>Dev</th><th>Non-Dev</th>
  </tr></thead>
<tbody>
  <tr>
    <td>ProblemDetails.Problem</td><td>PD</td><td>PD</td><td>Text</td><td>PD</td><td>PD</td><td>PD</td><td>Text</td><td>PD</td>
  </tr>
  <tr>
    <td>ProblemDetails.Service<br>via minimal API endpoints</td><td>PD</td><td>PD</td><td>PD</td><td>PD</td><td>Text</td><td>Text</td><td>Text</td><td>Text</td>
  </tr>
  <tr>
    <td>ProblemDetails.Service<br>via controller endpoints</td><td>PD</td><td>PD</td><td>PD</td><td>PD</td><td>PD<br></td><td>PD</td><td>PD<br></td><td>PD</td>
  </tr>
  <tr>
    <td>ProblemDetails.Writer</td><td>PD</td><td>PD</td><td>Text</td><td>PD</td><td>Text</td><td>Text</td><td>Text</td><td>Text</td>
  </tr>
</tbody></table>

### Example output
Problem details object, with 'math' feature details:
```json
{
  "type": "https://example.com/probs/division-by-zero",
  "title": "Bad Request",
  "status": 400,
  "detail": "Division by zero is not allowed.",
  "instance": "/api/v1/divide?numerator=1&denominator=0",
  "traceId": "00-0effaf938421c56593664b6dd3365e20-f6fc2f7949ba33ed-00"
}
```
Problem details object, without any feature details:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "instance": "/api/v1/squareroot?radicand=-1",
  "traceId": "00-83d9c1c2af136692bd97845571c3e41b-e37be8b0a1a3aa8f-00"
}
```
Plain text, when the problem details writer cannot write a problem details object:
```text
type: https://example.com/probs/division-by-zero
title: Bad Request
status: 400
detail: Division by zero is not allowed.
instance: /api/v1/divide?numerator=1&denominator=0
traceId: 00-170f229e1e396fbd0ca16460c1abe7f8-2aab82e2ee7cdee4-00
```
Problem details object, resulting from an unhandled exception in a non-development environment:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "detail": "Sample Exception",
  "traceId": "00-2ecff895699e7612e8ba8e1c9def6f74-b222cca5baa69266-00"
}
```
Problem details object, resulting from an unhandled exception in the development environment:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "System.InvalidOperationException",
  "status": 500,
  "detail": "Sample Exception",
  "exception": {
    "details": "System.InvalidOperationException: Sample Exception\r\n   at Metalhead.Examples.ProblemDetailsService.Api.MathEndpoints.ThrowException() in C:\\Users\\jason.keeler\\source\\repos\\ProblemDetailsExamples\\Metalhead.Examples.ProblemDetails.Service.Api\\MathEndpoints.cs:line 217\r\n   at lambda_method31(Closure, Object, HttpContext)\r\n   at Microsoft.AspNetCore.Routing.EndpointMiddleware.Invoke(HttpContext httpContext)\r\n   at Metalhead.Examples.ProblemDetailsService.Api.ProblemDetailsMiddleware.InvokeAsync(HttpContext httpContext) in C:\\Users\\jason.keeler\\source\\repos\\ProblemDetailsExamples\\Metalhead.Examples.ProblemDetails.Service.Api\\ProblemDetailsMiddleware.cs:line 11\r\n   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)\r\n   at Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIMiddleware.Invoke(HttpContext httpContext)\r\n   at Swashbuckle.AspNetCore.Swagger.SwaggerMiddleware.Invoke(HttpContext httpContext, ISwaggerProvider swaggerProvider)\r\n   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)",
    "headers": {
      "Accept": [
        "application/json"
      ],
      "Host": [
        "localhost:7181"
      ],
      "traceparent": [
        "00-8d90050944aa1c4e2b82d31ae01c5e1c-57b093803ae0c94c-00"
      ]
    },
    "path": "/api/v1/throwex",
    "endpoint": "HTTP: GET /api/v1/throwex => ThrowException",
    "routeValues": {}
  },
  "traceId": "00-8d90050944aa1c4e2b82d31ae01c5e1c-ccf6b96113b14761-00"
}
```
Plain text, when the problem details writer cannot write an unhandled exception as a problem details object in the development environment:
```text
System.InvalidOperationException: Sample Exception
   at Metalhead.Examples.ProblemDetailsWriter.Api.MathEndpoints.ThrowException() in C:\Users\jason.keeler\source\repos\ProblemDetailsExamples\Metalhead.Examples.ProblemDetails.Writer.Api\MathEndpoints.cs:line 197
   at lambda_method29(Closure, Object, HttpContext)
   at Microsoft.AspNetCore.Routing.EndpointMiddleware.Invoke(HttpContext httpContext)
   at Metalhead.Examples.ProblemDetailsWriter.Api.ProblemDetailsMiddleware.InvokeAsync(HttpContext httpContext) in C:\Users\jason.keeler\source\repos\ProblemDetailsExamples\Metalhead.Examples.ProblemDetails.Writer.Api\ProblemDetailsMiddleware.cs:line 11
   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
   at Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIMiddleware.Invoke(HttpContext httpContext)
   at Swashbuckle.AspNetCore.Swagger.SwaggerMiddleware.Invoke(HttpContext httpContext, ISwaggerProvider swaggerProvider)
   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
   at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

HEADERS
=======
Accept: application/json
Host: localhost:7192
traceparent: 00-6c1d13332cda70eccdd60f4190759bf9-e4fe08cf15084344-00
```

# Setup instructions
1. Clone the ProblemDetailsExamples repository.
2. Open the .NET solution in Visual Studio 2022 (or a compatible alternative).
3. Update `appsettings.json` and `appsettings.Development.json` if necessary, e.g. path to log file.
4. Set one of the above projects as the startup project.
5. Build the solution and run!
6. Use Swagger to test the endpoints in the development environment, or use the `<project name>.http` files to test the endpoints.
