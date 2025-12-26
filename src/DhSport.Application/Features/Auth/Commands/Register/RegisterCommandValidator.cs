using FluentValidation;

namespace DhSport.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Dto.LogonId)
            .NotEmpty().WithMessage("로그인 ID는 필수입니다.")
            .Length(3, 50).WithMessage("로그인 ID는 3-50자 사이여야 합니다.")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("로그인 ID는 영문자, 숫자, 언더스코어만 사용 가능합니다.");

        RuleFor(x => x.Dto.Passwd)
            .NotEmpty().WithMessage("비밀번호는 필수입니다.")
            .MinimumLength(8).WithMessage("비밀번호는 최소 8자 이상이어야 합니다.");

        RuleFor(x => x.Dto.UserNm)
            .NotEmpty().WithMessage("사용자 이름은 필수입니다.")
            .Length(2, 50).WithMessage("사용자 이름은 2-50자 사이여야 합니다.");

        RuleFor(x => x.Dto.Email)
            .NotEmpty().WithMessage("이메일은 필수입니다.")
            .EmailAddress().WithMessage("유효한 이메일 형식이 아닙니다.");

        RuleFor(x => x.Dto.Tel)
            .Matches(@"^[0-9-+() ]+$").WithMessage("유효한 전화번호 형식이 아닙니다.")
            .When(x => !string.IsNullOrEmpty(x.Dto.Tel));
    }
}
