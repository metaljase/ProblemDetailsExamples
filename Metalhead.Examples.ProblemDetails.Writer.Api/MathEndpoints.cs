using Microsoft.AspNetCore.Http.HttpResults;
using System.Numerics;

namespace Metalhead.Examples.ProblemDetailsWriter.Api;

public static class MathEndpoints
{
    public static void Register(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", ViewEndpoints)
            .WithName(nameof(ViewEndpoints))
            .WithOpenApi();

        var mathGroup = endpoints.MapGroup("/api/v1/");

        // /api/v1/divide?numerator=1&denominator=0
        mathGroup.MapGet("/divide", GetDivide)
            .WithName(nameof(GetDivide))
            .WithOpenApi();

        // /api/v1/squareroot?radicand=-1
        mathGroup.MapGet("/squareroot", GetSquareRoot)
            .WithName(nameof(GetSquareRoot))
            .WithOpenApi();

        // /api/v1/complexsquareroot?value=-1
        mathGroup.MapGet("/complexsquareroot", GetComplexSquareRoot)
            .WithName(nameof(GetComplexSquareRoot))
            .WithOpenApi();

        // /api/v1/logarithm?value=0
        mathGroup.MapGet("/logarithm", GetLogarithm)
            .WithName(nameof(GetLogarithm))
            .WithOpenApi();

        // /api/v1/factorialwithoverflow?n=21
        mathGroup.MapGet("/factorialwithoverflow", GetFactorialWithOverflow)
            .WithName(nameof(GetFactorialWithOverflow))
            .WithOpenApi();

        // /api/v1/factorial?n=101
        mathGroup.MapGet("/factorialrange", GetFactorial)
            .WithName(nameof(GetFactorial))
            .WithOpenApi();

        // /api/v1/inversecosine?value=1.1
        mathGroup.MapGet("/inversecosine", GetInverseCosine)
            .WithName(nameof(GetInverseCosine))
            .WithOpenApi();

        // /api/v1/throwex
        mathGroup.MapGet("/throwex", ThrowException)
            .WithName(nameof(ThrowException))
            .WithOpenApi();
    }

    public static Ok<string> ViewEndpoints()
    {
        string message = "Please view the 'Metalhead.Examples.ProblemDetails.Writer.Api.http' file in the project for endpoint examples.";
        return TypedResults.Ok(message);
    }

    public static Results<Ok<double>, BadRequest> GetDivide(HttpContext httpContext, double numerator, double denominator)
    {
        if (denominator == 0)
        {
            var errorFeature = new MathErrorFeature
            {
                MathError = MathErrorType.DivisionByZeroError,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}{httpContext.Request.QueryString}"
            };
            httpContext.Features.Set(errorFeature);
            return TypedResults.BadRequest();
        }
        double result = numerator / denominator;
        return TypedResults.Ok(result);
    }

    // An example without using a feature, for comparing output with endpoints using features (e.g. MathErrorFeature).
    public static Results<Ok<double>, BadRequest> GetSquareRoot(HttpContext httpContext, double radicand)
    {
        if (radicand < 0)
        {
            //var errorFeature = new MathErrorFeature
            //{
            //    MathError = MathErrorType.NegativeRadicandError,
            //    Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}{httpContext.Request.QueryString}"
            //};
            //httpContext.Features.Set(errorFeature);
            return TypedResults.BadRequest();
        }
        double result = Math.Sqrt(radicand);
        return TypedResults.Ok(result);
    }

    public static Results<Ok<string>, BadRequest> GetComplexSquareRoot(HttpContext httpContext, double radicand)
    {
        if (radicand < 0)
        {
            var errorFeature = new MathErrorFeature
            {
                MathError = MathErrorType.ComplexNumberOperationError,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}{httpContext.Request.QueryString}"
            };
            httpContext.Features.Set(errorFeature);

            var complexResult = $"{Math.Sqrt(-radicand)}i";
            return TypedResults.Ok(complexResult);
        }
        double result = Math.Sqrt(radicand);
        return TypedResults.Ok(result.ToString());
    }

    public static Results<Ok<double>, BadRequest> GetLogarithm(HttpContext httpContext, double value)
    {
        if (value <= 0)
        {
            var errorFeature = new MathErrorFeature
            {
                MathError = MathErrorType.InvalidLogarithmError,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}{httpContext.Request.QueryString}"
            };
            httpContext.Features.Set(errorFeature);
            return TypedResults.BadRequest();
        }
        double result = Math.Log(value);
        return TypedResults.Ok(result);
    }

    public static Results<Ok<long>, BadRequest> GetFactorialWithOverflow(HttpContext httpContext, int n)
    {
        try
        {
            long result = FactorialWithOverflow(n);
            return TypedResults.Ok(result);
        }
        catch (OverflowException)
        {
            var errorFeature = new MathErrorFeature
            {
                MathError = MathErrorType.OverflowError,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}{httpContext.Request.QueryString}"
            };
            httpContext.Features.Set(errorFeature);
            return TypedResults.BadRequest();
        }
    }

    private static long FactorialWithOverflow(int n)
    {
        checked // This will throw an OverflowException if the result is too large.
        {
            if (n < 0) throw new ArgumentOutOfRangeException(nameof(n), "Factorial is not defined for negative numbers.");
            return n <= 1 ? 1 : n * FactorialWithOverflow(n - 1);
        }
    }

    public static Results<Ok<string>, BadRequest> GetFactorial(HttpContext httpContext, int n)
    {
        if (n < 0 || n > 100) // Limiting to 100 for practical reasons
        {
            var errorFeature = new MathErrorFeature
            {
                MathError = MathErrorType.OutOfRangeFactorialError,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}{httpContext.Request.QueryString}"
            };
            httpContext.Features.Set(errorFeature);
            return TypedResults.BadRequest();
        }
        BigInteger result = FactorialWithoutOverflow(n);
        return TypedResults.Ok(result.ToString());
    }

    private static BigInteger FactorialWithoutOverflow(int n)
    {
        if (n == 0) return 1;
        BigInteger result = 1;
        for (int i = 1; i <= n; i++)
        {
            result *= i;
        }
        return result;
    }

    public static Results<Ok<double>, BadRequest> GetInverseCosine(HttpContext httpContext, double value)
    {
        if (value < -1 || value > 1)
        {
            var errorFeature = new MathErrorFeature
            {
                MathError = MathErrorType.InvalidTrigonometricArgumentError,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}{httpContext.Request.QueryString}"
            };
            httpContext.Features.Set(errorFeature);
            return TypedResults.BadRequest();
        }
        double result = Math.Acos(value);
        return TypedResults.Ok(result);
    }

    // An example of an endpoint that throws an InvalidOperationException.
    public static Results<Ok<string>, BadRequest> ThrowException()
    {        
        throw new InvalidOperationException("Sample Exception");
#pragma warning disable CS0162 // Unreachable code detected
        return TypedResults.Ok("This will not be reached.");
#pragma warning restore CS0162 // Unreachable code detected
    }
}
