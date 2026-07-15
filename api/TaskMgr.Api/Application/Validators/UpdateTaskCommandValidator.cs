using FluentValidation;
using TaskMgr.Api.Application.Tasks.Commands.UpdateTask;

namespace TaskMgr.Api.Application.Validators;

/// <summary>
/// Validator for UpdateTaskCommand
/// </summary>
public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(300)
            .WithMessage("Task title cannot exceed 300 characters")
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("Task title cannot be empty or whitespace");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Task description cannot exceed 2000 characters");

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
            .WithMessage("Invalid task status");

        RuleFor(x => x.Priority)
            .IsInEnum()
            .When(x => x.Priority.HasValue)
            .WithMessage("Invalid task priority");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .When(x => x.DueDate.HasValue)
            .WithMessage("Due date must be in the future");

        RuleFor(x => x.EstimatedHours)
            .GreaterThan(0)
            .When(x => x.EstimatedHours.HasValue)
            .WithMessage("Estimated hours must be greater than 0")
            .LessThanOrEqualTo(1000)
            .When(x => x.EstimatedHours.HasValue)
            .WithMessage("Estimated hours cannot exceed 1000");

        RuleFor(x => x.Tags)
            .Must(tags => tags!.All(tag => !string.IsNullOrWhiteSpace(tag)))
            .When(x => x.Tags != null && x.Tags.Any())
            .WithMessage("Tags cannot be empty or whitespace")
            .Must(tags => tags!.Count <= 10)
            .When(x => x.Tags != null)
            .WithMessage("Cannot have more than 10 tags")
            .Must(tags => tags!.All(tag => tag.Length <= 50))
            .When(x => x.Tags != null && x.Tags.Any())
            .WithMessage("Each tag cannot exceed 50 characters");
    }
}
