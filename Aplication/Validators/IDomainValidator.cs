namespace Application.Validators;

public interface IDomainValidator<in T>
{
    Task ValidateAsync(T instance, CancellationToken cancellationToken = default);
}
