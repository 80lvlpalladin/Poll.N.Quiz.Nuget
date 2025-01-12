using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Poll.N.Quiz.API.Shared.Extensions;

public static class MediatorExtensions
{
    public static async Task<IResult> SendAndReturnResultAsync<TResponse>(
        this IMediator mediator,
        IRequest<ErrorOr<TResponse>> request,
        CancellationToken cancellationToken = default)
    {
        var errorOrResult = await mediator.Send(request, cancellationToken);

        return errorOrResult.IsError ?
            CreateProblemResult(errorOrResult.Errors) :
            TypedResults.Ok(errorOrResult.Value);
    }

    private static IResult CreateProblemResult(IReadOnlyCollection<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.All(error => error.Type == ErrorType.Validation))
        {
            var modelStateDictionary = errors.GroupBy(error => error.Code).ToDictionary(
                errorGroup => errorGroup.Key,
                errorGroup => errorGroup.Select(error => error.Description).ToArray());

            return TypedResults.ValidationProblem(modelStateDictionary);
        }

        var firstError = errors.First();
        var statusCode = firstError.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        return TypedResults.Problem(statusCode: statusCode, title: firstError.Description);
    }
}
