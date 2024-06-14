namespace Metalhead.Examples.ProblemDetailsProblem.Api;

public class MathErrorFeature
{
    public MathErrorType MathError { get; set; }

    public void SetProblemDetails(ProblemDetailsContext problemDetailsContext)
    {
        (string Detail, string Type) details = MathError switch
        {
            MathErrorType.DivisionByZeroError => ("Division by zero is not allowed.",
                "https://example.com/probs/division-by-zero"),
            MathErrorType.NegativeRadicandError => ("The radicand cannot be negative for a square root operation.",
                "https://example.com/probs/invalid-radicand"),
            MathErrorType.InvalidLogarithmError => ("The value must be positive for a logarithm operation.",
                "https://example.com/probs/invalid-logarithm"),
            MathErrorType.OverflowError => ("The factorial result is too large to be represented.",
                "https://example.com/probs/overflow"),
            MathErrorType.InvalidTrigonometricArgumentError => ("The value must be between -1 and 1 for the inverse cosine operation.",
                "https://example.com/probs/invalid-trigonometric-argument"),
            MathErrorType.OutOfRangeFactorialError => ("Factorial must be a non-negative integer less than or equal to 100.",
                "https://example.com/probs/out-of-range-factorial"),
            MathErrorType.ComplexNumberOperationError => ("Square root of a negative number results in a complex number, which is not supported.",
                "https://example.com/probs/complex-number-operation"),
            _ => ("Unknown math error encountered", "https://example.com/probs/other")
        };

        problemDetailsContext.ProblemDetails.Type = details.Type;
        problemDetailsContext.ProblemDetails.Title = "Bad Request";
        problemDetailsContext.ProblemDetails.Detail = details.Detail;
    }
}

public enum MathErrorType
{
    DivisionByZeroError,
    NegativeRadicandError,
    InvalidLogarithmError,
    OverflowError,
    InvalidTrigonometricArgumentError,
    OutOfRangeFactorialError,
    ComplexNumberOperationError
}
