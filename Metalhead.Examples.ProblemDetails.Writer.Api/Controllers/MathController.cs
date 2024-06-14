using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

namespace Metalhead.Examples.ProblemDetailsWriter.Api.Controllers;

[Route("api/v1/[controller]/[action]")]
[ApiController]
public class MathController : ControllerBase
{
    // An example using a feature, for comparing output with endpoints not using features.
    // /api/v1/math/divide/1/0
    [HttpGet("{numerator}/{denominator}", Name = nameof(Divide))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(double))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Results<Ok<double>, BadRequest> Divide(double numerator, double denominator)
    {
        if (denominator == 0)
        {            
            var errorType = new MathErrorFeature
            {
                MathError = MathErrorType.DivisionByZeroError
            };
            HttpContext.Features.Set(errorType);
            return TypedResults.BadRequest();
        }
        double result = numerator / denominator;
        return TypedResults.Ok(result);
    }

    // An example without using a feature, for comparing output with endpoints using features (e.g. MathErrorFeature).
    // /api/v1/math/squareroot/-1
    [HttpGet("{radicand}", Name = nameof(SquareRoot))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(double))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Results<Ok<double>, BadRequest> SquareRoot(double radicand)
    {
        if (radicand < 0)
        {
            //var errorFeature = new MathErrorFeature
            //{
            //    MathError = MathErrorType.NegativeRadicandError
            //};
            //HttpContext.Features.Set(errorFeature);
            return TypedResults.BadRequest();
        }
        double result = Math.Sqrt(radicand);
        return TypedResults.Ok(result);
    }

    // /api/v1/math/complexsquareroot/-1
    [HttpGet("{radicand}", Name = nameof(ComplexSquareRoot))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Results<Ok<string>, BadRequest> ComplexSquareRoot(double radicand)
    {
        if (radicand < 0)
        {
            var errorFeature = new MathErrorFeature
            {
                MathError = MathErrorType.ComplexNumberOperationError
            };
            HttpContext.Features.Set(errorFeature);

            var complexResult = $"{Math.Sqrt(-radicand)}i";
            return TypedResults.Ok(complexResult);
        }
        double result = Math.Sqrt(radicand);
        return TypedResults.Ok(result.ToString());
    }

    // /api/v1/math/logarithm/0
    [HttpGet("{value}", Name = nameof(Logarithm))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(double))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Results<Ok<double>, BadRequest> Logarithm(double value)
    {
        if (value <= 0)
        {
            var errorFeature = new MathErrorFeature
            {
                MathError = MathErrorType.InvalidLogarithmError
            };
            HttpContext.Features.Set(errorFeature);
            return TypedResults.BadRequest();
        }
        double result = Math.Log(value);
        return TypedResults.Ok(result);
    }

    // /api/v1/math/factorialwithoverflow/21
    [HttpGet("{n}", Name = nameof(FactorialWithOverflow))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(long))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Results<Ok<long>, BadRequest> FactorialWithOverflow(int n)
    {
        try
        {
            long result = MathController.GetFactorialWithOverflow(n);
            return TypedResults.Ok(result);
        }
        catch (OverflowException)
        {
            var errorFeature = new MathErrorFeature
            {
                MathError = MathErrorType.OverflowError
            };
            HttpContext.Features.Set(errorFeature);
            return TypedResults.BadRequest();
        }
    }

    private static long GetFactorialWithOverflow(int n)
    {
        checked // This will throw an OverflowException if the result is too large.
        {
            if (n < 0) throw new ArgumentOutOfRangeException(nameof(n), "Factorial is not defined for negative numbers.");
            return n <= 1 ? 1 : n * GetFactorialWithOverflow(n - 1);
        }
    }

    // /api/v1/math/factorial/101
    [HttpGet("{n}", Name = nameof(Factorial))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Results<Ok<string>, BadRequest> Factorial(int n)
    {
        if (n < 0 || n > 100) // Limiting to 100 for practical reasons
        {
            var errorFeature = new MathErrorFeature
            {
                MathError = MathErrorType.OutOfRangeFactorialError
            };
            HttpContext.Features.Set(errorFeature);
            return TypedResults.BadRequest();
        }
        BigInteger result = GetFactorialWithoutOverflow(n);
        return TypedResults.Ok(result.ToString());
    }

    private static BigInteger GetFactorialWithoutOverflow(int n)
    {
        if (n == 0) return 1;
        BigInteger result = 1;
        for (int i = 1; i <= n; i++)
        {
            result *= i;
        }
        return result;
    }

    // /api/v1/math/inversecosine/1.1
    [HttpGet("{value}", Name = nameof(InverseCosine))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(double))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Results<Ok<double>, BadRequest> InverseCosine(double value)
    {
        if (value < -1 || value > 1)
        {
            var errorFeature = new MathErrorFeature
            {
                MathError = MathErrorType.InvalidTrigonometricArgumentError
            };
            HttpContext.Features.Set(errorFeature);
            return TypedResults.BadRequest();
        }
        double result = Math.Acos(value);
        return TypedResults.Ok(result);
    }

    // /api/v1/math/thowex
    [HttpGet(Name = nameof(ThrowEx))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    public Results<Ok<string>, BadRequest> ThrowEx()
    {
        // An example of an endpoint that throws an InvalidOperationException.
        throw new InvalidOperationException("Sample Exception");
#pragma warning disable CS0162 // Unreachable code detected
        return TypedResults.Ok("This will not be reached.");
#pragma warning restore CS0162 // Unreachable code detected
    }


    // This endpoint returns a BadRequestResult when the denominator is 0.  BadRequestResult writes
    // to the response stream which prevents a customised problem details response, i.e. the math
    // feature will not be included in the response.  As a workaround, returning ControllerBase.Problem
    // can be used (see the 'workaroundresponse' endpoint), or using TypedResults.BadRequest.
    // /api/v1/math/defectiveresponse/1/0
    [HttpGet("{numerator}/{denominator}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(double))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult DefectiveResponse(double numerator, double denominator)
    {
        if (denominator == 0)
        {
            var errorType = new MathErrorFeature
            {
                MathError = MathErrorType.DivisionByZeroError
            };
            HttpContext.Features.Set(errorType);
            return BadRequest();
        }
        return Ok(numerator / denominator);
    }

    // This endpoint returns a ControllerBase.Problem when the denominator is 0.  This is a
    // workaround for the issue where returning a BadRequestResult prevents a custom problem
    // details being written to the response, due to BadRequestResult writing to the response.
    // An alternative workaround is to return TypedResults.BadRequest.
    // /api/v1/math/workaroundresponse/1/0
    [HttpGet("{numerator}/{denominator}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(double))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult WorkaroundResponse(double numerator, double denominator)
    {
        if (denominator == 0)
        {
            return Problem(
                type: "https://example.com/probs/division-by-zero",
                title: "Bad Request",
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Division by zero is not allowed.",
                instance: $"{HttpContext.Request.Path}");
        }
        return Ok(numerator / denominator);
    }
}
