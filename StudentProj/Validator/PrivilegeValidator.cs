using FluentValidation;
using StudentProj.DTO;

namespace StudentProj.Validators
{
    public class PrivilegeValidator : AbstractValidator<PrivilegeDTO>
    {
        public PrivilegeValidator()
        {
            RuleFor(x => x.PrivilegeName)
                .NotEmpty().WithMessage("Privilege name is required!")
                .NotNull().WithMessage("Privilege name cannot be null!")
                .MinimumLength(3).WithMessage("Privilege name must be at least 3 characters!")
                .MaximumLength(30).WithMessage("Privilege name cannot exceed 30 characters!")
                // Allows letters, colons (:), and hyphens (-) to support formats like "read:student"
                .Matches("^[a-zA-Z:-]+$")
                .WithMessage("Privilege name can only contain letters, colons (:), and hyphens (-).");
        }
    }
}