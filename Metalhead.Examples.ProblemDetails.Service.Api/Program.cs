using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using System.Diagnostics;

using Metalhead.Examples.ProblemDetailsService.Api;

var builder = WebApplication.CreateBuilder(args);

// Configure the logger.
builder.Logging.ClearProviders().AddSerilog();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    // Uncomment the following line to only log HttpLogging events (also add 'using Serilog.Events').
    //.Filter.ByIncludingOnly(e => e.Properties.ContainsKey("SourceContext") && e.Properties["SourceContext"].ToString().Contains("Microsoft.AspNetCore.HttpLogging"))
    .CreateLogger();

try
{
    builder.Host.UseSerilog();
    builder.Services.AddAuthorization();
    builder.Services.AddControllers();

    // The ProblemDetails service facilitates the customisation of ProblemDetails, e.g. to add a trace ID.
    // Additionally, in the development environment, exception stack traces will be included in ProblemDetails.
    builder.Services.AddProblemDetails(problemDetailsOptions =>
        problemDetailsOptions.CustomizeProblemDetails = (problemDetailsContext) =>
        {
            var httpContext = problemDetailsContext.HttpContext;

            var exception = httpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
            if (exception is not null)
            {
                // Add a trace ID to problem details.  Alternatively, it can be added in an exception handler,
                // but that would only work for non-development environments, unless using UseExceptionHandler
                // in for the development environment!
                // Adding here works for all environments.
                problemDetailsContext.ProblemDetails.Detail = exception.Message;
                if (!problemDetailsContext.ProblemDetails.Extensions.ContainsKey("traceId"))
                {
                    var traceId = Activity.Current?.Id ?? problemDetailsContext.HttpContext.TraceIdentifier;
                    problemDetailsContext.ProblemDetails.Extensions.Add("traceId", traceId);
                }
            }
        });

    
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        // Handle unhandled exceptions in non-development environments.
        app.UseExceptionHandler(applicationBuilder =>
            applicationBuilder.Run(async httpContext =>
            {
                if (httpContext.Response.HasStarted)
                    return;

                var problemDetailsService = httpContext.RequestServices.GetService<IProblemDetailsService>();
                var problemDetailsContext = new ProblemDetailsContext { HttpContext = httpContext };
                if (problemDetailsService is null || !await problemDetailsService.TryWriteAsync(problemDetailsContext))
                {
                    // Failed to write problem details to the response.  This can happen if the media types in
                    // the Accept header do not include a supported media type for this problem details writer,
                    // e.g. a subset of 'application/problem+json' or 'application/json'.
                    // Write a fallback message without the problem details writer.
                    httpContext.Response.ContentType = System.Net.Mime.MediaTypeNames.Text.Plain;
                    var exception = httpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
                    ProblemDetailsDefaults.Apply(problemDetailsContext.ProblemDetails, httpContext.Response.StatusCode);
                    var message = $"""
                        type: {problemDetailsContext.ProblemDetails.Type}
                        title: {problemDetailsContext.ProblemDetails.Title}
                        status: {problemDetailsContext.ProblemDetails.Status}
                        detail: {exception?.Message}
                        traceId: {Activity.Current?.Id ?? httpContext.TraceIdentifier}
                        """;
                    await httpContext.Response.WriteAsync(message);
                }
            }));
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();

    // Middleware for writing problem details.
    app.UseMiddleware<ProblemDetailsMiddleware>();

    // NOTE: Either UseMiddleware (above) or UseStatusCodePages (below) can be used...
    // The same functionality is provided by both, but UseMiddleware is more flexible.

    // Middleware for writing problem details for status codes between 400-599 (not unhandled exceptions).
    //app.UseStatusCodePages(ProblemDetailsStatusCodePages.HandleStatusCodeAsync);

    // Add Maths endpoints.
    MathEndpoints.Register(app);
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    // Flush and close the log before application exit.
    Log.CloseAndFlush();
}
