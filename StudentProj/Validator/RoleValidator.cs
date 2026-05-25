using FluentValidation;
using StudentProj.DTO;

namespace StudentProj.Validators
{
    public class RoleValidator : AbstractValidator<RoleDTO>
    {
        public RoleValidator()
        {
            RuleFor(x => x.RoleName)
                // not empty
                .NotEmpty()
                .WithMessage("Role name is required!")

                // not null
                .NotNull()
                .WithMessage("Role name cannot be null!")

                // min 2 characters
                .MinimumLength(2)
                .WithMessage("Role name must be at least 2 characters!")

                // max 12 characters
                .MaximumLength(12)
                .WithMessage("Role name cannot exceed 12 characters!")

                // only letters allowed
                .Matches("^[a-zA-Z]+$")
                .WithMessage("Role name can only contain letters!");
        }
    }
}