using FluentValidation;
using FirstAPI.DTOs.Teachers;

public class UpdateTeacherDtoValidator : AbstractValidator<UpdateTeacherDto>
{
    public UpdateTeacherDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Subject)
            .NotEmpty()
            .MaximumLength(100);
    }
}
