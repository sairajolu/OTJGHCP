using FluentValidation;

namespace TaskBridge.Application.Projects;

/// <summary>Validates project creation input.</summary>
public sealed class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    /// <summary>Initializes the project creation validation rules.</summary>
    public CreateProjectRequestValidator()
    {
        RuleFor(request => request.TeamId).NotEmpty();
        RuleFor(request => request.Name).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Description).MaximumLength(2000);
    }
}

/// <summary>Validates project status update input.</summary>
public sealed class UpdateProjectStatusRequestValidator : AbstractValidator<UpdateProjectStatusRequest>
{
    /// <summary>Initializes the project status validation rules.</summary>
    public UpdateProjectStatusRequestValidator()
    {
        RuleFor(request => request.ProjectId).NotEmpty();
        RuleFor(request => request.Status).IsInEnum();
    }
}