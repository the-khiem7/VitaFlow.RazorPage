using FluentValidation;
using VitaFlow.Core.Entities;

namespace VitaFlow.Services.Validators
{
    /// <summary>
    /// Validator for Donor entity using FluentValidation.
    /// </summary>
    public class DonorValidator : AbstractValidator<Donor>
    {
        public DonorValidator()
        {
            RuleFor(d => d.BloodType).IsInEnum().WithMessage("Invalid blood type.");
            RuleFor(d => d.Email).NotEmpty().EmailAddress().WithMessage("Valid email is required.");
            RuleFor(d => d.PhoneNumber).NotEmpty().WithMessage("Phone number is required.");
            RuleFor(d => d.DateOfBirth).LessThan(System.DateTime.Now.AddYears(-18)).WithMessage("Donor must be at least 18 years old.");
        }
    }
}
