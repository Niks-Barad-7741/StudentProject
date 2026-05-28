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

            RuleFor(x => x.RoleIds)
                .NotEmpty()
                .WithMessage("Role IDs are required")
                .Matches(@"^[0-9]+(,[0-9]+)*$")
                .WithMessage("Role IDs must be a comma-separated list of numbers (e.g. '1,2')");
        }
    }
}