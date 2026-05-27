using FluentValidation;
using StudentProj.DTO;

namespace StudentProj.Validator
{
    public class RegisterValidator : AbstractValidator<RegisterDTO>
    {
        public RegisterValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required")
                .NotNull()
                .WithMessage("Name cannot be null")
                .MinimumLength(2)
                .WithMessage("Name must be at least 2 characters long")
                .MaximumLength(30);
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is Required")
                .EmailAddress()
                .Matches(@"^[a-zA-Z0-9._%+-]+@gmail\.com$")
                .WithMessage("Email must be a valid Gmail address")
                .NotNull()
                .WithMessage("Email cannot be null");
            RuleFor(x => x.Address)
                .NotEmpty()
                .WithMessage("Address is required")
                .NotNull()
                .WithMessage("Address cannot be null")
                .MaximumLength(200);

            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("Phone number is required")
                .NotNull()
                .WithMessage("Phone number cannot be null")
                .Matches(@"^\d{10}$")
                .MaximumLength(10);


            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is Required")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters long")
                .MaximumLength(20);

        }

    }
}
