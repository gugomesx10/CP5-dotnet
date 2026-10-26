namespace CP5.Application.Exceptions;

public sealed class RepositoryException : Exception
{
    public RepositoryException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}