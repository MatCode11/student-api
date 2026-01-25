using FluentValidation;
using FirstAPI.DTOs.Teachers;

public class CreateTeacherDtoValidator : AbstractValidator<CreateTeacherDto>
{
    public CreateTeacherDtoValidator()
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
