//10додаткове
using Api.Contracts;
using FluentValidation;

namespace Api.Validators;

public sealed class SaveProductRequestValidator : AbstractValidator<SaveProductRequest>
{
    public SaveProductRequestValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("Артикул обов'язковий.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва обов'язкова.")
            .MaximumLength(200).WithMessage("Назва до 200 символів.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Одиниця виміру обов'язкова.")
            .MaximumLength(20).WithMessage("Одиниця виміру до 20 символів.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Кількість не може бути від'ємною.");
    }
}