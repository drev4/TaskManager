using FluentValidation;
using TaskMgr.Api.Application.Tasks.Commands.RecordTaskTime;

namespace TaskMgr.Api.Application.Validators;

/// <summary>
/// Validator for RecordTaskTimeCommand
/// </summary>
public class RecordTaskTimeCommandValidator : AbstractValidator<RecordTaskTimeCommand>
{
    public RecordTaskTimeCommandValidator()
    {
        RuleFor(x => x.Hours)
            .GreaterThan(0)
            .WithMessage("Hours must be greater than 0");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters");
    }
}
