namespace CP5.Domain.Exceptions;

public sealed class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entidade, object id)
        : base($"{entidade} com ID {id} não foi encontrado.")
    {
    }
}