using FluentValidation;
using StudentProj.DTO;

namespace StudentProj.Validator
{
    public class StudentValidator : AbstractValidator<StudentDTO>
    {
        public StudentValidator() 
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
                .WithMessage("Invalid Email Address")
                .Matches(@"^[a-zA-Z0-9._%+-]+@gmail\.com$")
                .WithMessage("Email must be a valid Gmail address");

            RuleFor(x => x.Address)
                .NotEmpty()
                .WithMessage("Address is required")
                .NotNull()
                .WithMessage("Address cannot be null")
                .MaximumLength(200)
                .WithMessage("Address Cannot Exceed 200 Characters");

            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("Phone number is required")
                .NotNull()
                .WithMessage("Phone number cannot be null")
                .Matches(@"^\d{10}$")
                .WithMessage("Phone number must be exactly 10 digits")
                .MaximumLength(10);
        }
    }
}
