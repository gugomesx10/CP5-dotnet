namespace CP5.Application.Results;

public enum OperationStatus
{
    Success,
    NotFound,
    ValidationError,
    Conflict,
    ExternalServiceFailure,
    UnexpectedError
}