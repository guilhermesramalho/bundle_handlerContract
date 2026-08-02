namespace PortalAle.Application.Base;

/// <summary>
/// Result Pattern: encapsula sucesso/falha de um caso de uso sem uso de exceções
/// para fluxo de controle de negócio. Ver DT-016/DT-020.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }

    public IReadOnlyList<string> Errors { get; }

    protected Result(bool isSuccess, IReadOnlyList<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static Result Success() => new(true, []);

    public static Result Failure(string erro) => new(false, [erro]);

    public static Result Failure(IEnumerable<string> erros) => new(false, [.. erros]);
}

/// <summary>
/// Result Pattern com valor de retorno tipado.
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    protected Result(bool isSuccess, T? value, IReadOnlyList<string> errors)
        : base(isSuccess, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, []);

    public static new Result<T> Failure(string erro) => new(false, default, [erro]);

    public static new Result<T> Failure(IEnumerable<string> erros) => new(false, default, [.. erros]);
}
