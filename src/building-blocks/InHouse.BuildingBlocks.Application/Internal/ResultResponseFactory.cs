using InHouse.BuildingBlocks.Primitives;

namespace InHouse.BuildingBlocks.Application;

internal static class ResultResponseFactory
{
    public static bool TryCreateFailure<TResponse>(IReadOnlyList<Error> errors, out TResponse? response)
    {
        response = default;

        var t = typeof(TResponse);

        if (t == typeof(Result))
        {
            response = (TResponse)(object)Result.Failure(errors);
            return true;
        }

        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Result<>))
        {
            // Result<T>.Failure(errors)
            var failureMethod = t.GetMethod("Failure", new[] { typeof(IEnumerable<Error>) });
            if (failureMethod is null)
                return false;

            var failure = failureMethod.Invoke(null, new object[] { errors });
            if (failure is null)
                return false;

            response = (TResponse)failure;
            return true;
        }

        return false;
    }
}
