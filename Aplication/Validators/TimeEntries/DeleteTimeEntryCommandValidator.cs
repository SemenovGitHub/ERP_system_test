using Application.Models.TimeEntries.Commands;
using FluentValidation;

namespace Application.Validators.TimeEntries;

public sealed class DeleteTimeEntryCommandValidator : AbstractValidator<DeleteTimeEntryCommand>
{
    public DeleteTimeEntryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
