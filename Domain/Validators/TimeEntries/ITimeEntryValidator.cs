using Domain.Models;
using FluentValidation;

namespace Domain.Validators.TimeEntries;

public interface ITimeEntryValidator : IValidator<TimeEntryModel>
{
    string Name { get; }
}
