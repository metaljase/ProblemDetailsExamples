using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using System.Diagnostics;

using Metalhead.Examples.ProblemDetailsProblem.Api;

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

    // NOTE: AddProblemDetails has not been added, therefore the ProblemDetails service has not been added.
    // The ProblemDetails service facilitates the customisation of ProblemDetails, e.g. to add a trace ID.
    // Additionally, in the development environment, exception stack traces will be included in ProblemDetails.

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
        {
            applicationBuilder.Run(async httpContext =>
            {
                if (httpContext.Response.HasStarted)
                    return;
                // The Problem Details service has not been added, which facilitates the customisation of
                // ProblemDetails, e.g. to add a trace ID.  So add it here instead; however, it will not
                // be added for the development environment because it does not use this exception handler.
                var errorMessage = httpContext.Features.Get<IExceptionHandlerFeature>()?.Error?.Message;
                var extensions = new Dictionary<string, object?>
                {
                    { "traceId", Activity.Current?.Id ?? httpContext.TraceIdentifier }
                };

                await Results.Problem(detail: errorMessage, extensions: extensions).ExecuteAsync(httpContext);
            });
        });
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