using FluentValidation;
using StudentProj.DTO;

namespace StudentProj.Validator
{
    public class LoginValidator : AbstractValidator<LoginDTO>
    {
        public LoginValidator() 
        {
            RuleFor(x =>x.Email)
                .NotEmpty()
                .WithMessage("Email is Required")
                .EmailAddress()
                .WithMessage("Invalid Email Address")
                .Matches(@"^[a-zA-Z0-9._%+-]+@gmail\.com$")
                .WithMessage("Email must be a valid Gmail address");
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is Required")
                .MinimumLength(6)
                //.WithMessage("Password must be at least 6 characters long")
                .MaximumLength(100);
        }
    }
}
