using FluentValidation;
using TaskScheduler.Application.DTOs.Tasks;

namespace TaskScheduler.Application.Validators
{
    public class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
    {
        public UpdateTaskDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Task title is required")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.Title));

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.DueDate)
                .GreaterThan(DateTime.UtcNow.AddMinutes(-1))
                .WithMessage("Due date must be in the future")
                .When(x => x.DueDate.HasValue);

            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("Invalid priority value")
                .When(x => x.Priority.HasValue);

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status value")
                .When(x => x.Status.HasValue);

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Invalid category")
                .When(x => x.CategoryId.HasValue);
        }
    }
}
