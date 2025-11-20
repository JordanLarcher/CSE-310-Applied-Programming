using FluentValidation;
using TaskScheduler.Application.DTOs.Tasks;

namespace TaskScheduler.Application.Validators
{
    public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
    {
        public CreateTaskDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Task title is required")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

            When(x => x.DueDate.HasValue, () =>
            {
                RuleFor(x => x.DueDate!.Value)
                    .GreaterThan(DateTime.UtcNow.AddMinutes(-1))
                    .WithMessage("Due date must be in the future");
            });

            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("Invalid priority value");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Invalid category")
                .When(x => x.CategoryId.HasValue);
        }
    }
}
