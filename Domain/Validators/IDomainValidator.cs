using Domain.Models;

namespace Domain.Validators;

public interface IDomainValidator<in T>
{
    Task ValidateAsync(T instance, CancellationToken cancellationToken = default);
}

public interface ICreateTimeEntryValidator : IDomainValidator<TimeEntryModel>
{
}

public interface IUpdateTimeEntryValidator : IDomainValidator<TimeEntryModel>
{
}

public interface IDeleteTimeEntryValidator : IDomainValidator<TimeEntryModel>
{
}
