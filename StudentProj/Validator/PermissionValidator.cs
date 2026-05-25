using FluentValidation;
using StudentProj.DTO;

namespace StudentProj.Validators
{
    public class PermissionValidator : AbstractValidator<PermissionDTO>
    {
        public PermissionValidator()
        {
            RuleFor(x => x.PermissionName)
                .NotEmpty().WithMessage("Permission name is required!")
                .NotNull().WithMessage("Permission name cannot be null!")
                .MinimumLength(3).WithMessage("Permission name must be at least 3 characters!")
                .MaximumLength(30).WithMessage("Permission name cannot exceed 30 characters!")
                // Allows letters, colons (:), and hyphens (-) to support formats like "read:student"
                .Matches("^[a-zA-Z:-]+$")
                .WithMessage("Permission name can only contain letters, colons (:), and hyphens (-).");
        }
    }
}