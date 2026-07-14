using FluentValidation;
using TaskMgr.Api.Application.Tasks.Commands.CreateTask;

namespace TaskMgr.Api.Application.Validators;

/// <summary>
/// Validator for CreateTaskCommand
/// </summary>
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Task title is required")
            .MaximumLength(300)
            .WithMessage("Task title cannot exceed 300 characters")
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("Task title cannot be empty or whitespace");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Task description cannot exceed 2000 characters");

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage("Invalid task priority");

        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow)
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
            .Must(tags => tags.All(tag => !string.IsNullOrWhiteSpace(tag)))
            .When(x => x.Tags.Any())
            .WithMessage("Tags cannot be empty or whitespace")
            .Must(tags => tags.Count <= 10)
            .WithMessage("Cannot have more than 10 tags")
            .Must(tags => tags.All(tag => tag.Length <= 50))
            .When(x => x.Tags.Any())
            .WithMessage("Each tag cannot exceed 50 characters");
    }
}
