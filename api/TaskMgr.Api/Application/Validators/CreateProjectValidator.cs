using FluentValidation;
using TaskMgr.Api.Application.DTOs;

namespace TaskMgr.Api.Application.Validators;

/// <summary>
/// Validator for CreateProjectDto
/// </summary>
public class CreateProjectValidator : AbstractValidator<CreateProjectDto>
{
    /// <summary>
    /// Initializes validation rules for creating a project
    /// </summary>
    public CreateProjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Project name is required")
            .MaximumLength(200)
            .WithMessage("Project name cannot exceed 200 characters")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Project name cannot be empty or whitespace");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Project description cannot exceed 1000 characters");

        RuleFor(x => x.Color)
            .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .When(x => !string.IsNullOrEmpty(x.Color))
            .WithMessage("Color must be a valid hex color code (e.g., #FF0000)");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.DueDate.HasValue)
            .WithMessage("Due date must be in the future");
    }
}
