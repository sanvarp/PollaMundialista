using FluentValidation;

using PollaMundialista.Application.Common;

namespace PollaMundialista.Application.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .Matches(ValidationPatterns.Email).WithMessage("El correo no es válido.")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("La contraseña debe incluir una mayúscula.")
            .Matches("[a-z]").WithMessage("La contraseña debe incluir una minúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe incluir un número.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("El nombre para mostrar es obligatorio.")
            .MaximumLength(60);
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().Matches(ValidationPatterns.Email).WithMessage("El correo no es válido.");
        RuleFor(x => x.Password).NotEmpty();
    }
}