namespace CP5.Application.Results;

public sealed class OperationResult<T>
{
    public bool IsSuccess => Status == OperationStatus.Success;

    public OperationStatus Status { get; }
    public T? Data { get; }
    public string Message { get; }
    public string? ErrorCode { get; }

    private OperationResult(
        OperationStatus status,
        T? data,
        string message,
        string? errorCode)
    {
        Status = status;
        Data = data;
        Message = message;
        ErrorCode = errorCode;
    }

    public static OperationResult<T> Success(
        T data,
        string message = "Operação realizada com sucesso.")
    {
        ArgumentNullException.ThrowIfNull(data);

        return new OperationResult<T>(
            OperationStatus.Success,
            data,
            message,
            null);
    }

    public static OperationResult<T> Failure(
        OperationStatus status,
        string errorCode,
        string message)
    {
        if (status == OperationStatus.Success)
            throw new ArgumentException(
                "Uma falha não pode ter status Success.",
                nameof(status));

        if (string.IsNullOrWhiteSpace(errorCode))
            throw new ArgumentException(
                "O código de erro é obrigatório.",
                nameof(errorCode));

        return new OperationResult<T>(
            status,
            default,
            message,
            errorCode);
    }
}