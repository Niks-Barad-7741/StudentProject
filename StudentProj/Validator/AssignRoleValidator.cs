using FluentValidation;
using StudentProj.DTO;

namespace StudentProj.Validator
{
    public class AssignRoleValidator : AbstractValidator<AssignRoleDTO>
    {
        public AssignRoleValidator()
        {
            RuleFor(x => x.StudentId)
                .GreaterThan(0)
                .WithMessage("Student Id must be greater than 0");

            //RuleFor(x => x.RoleId)
            //    .NotEmpty()
            //    .WithMessage("Role name is required")
            //    .Must(role => role == "Admin" || role == "User")
            //    .WithMessage("Role must be Admin or User");
            RuleFor(x => x.RoleId)
                .InclusiveBetween(1, 5)
                .WithMessage("Role must be Admin or User");
        }
    }
}