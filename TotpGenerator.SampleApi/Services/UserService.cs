using Microsoft.Extensions.Options;
using TotpGenerator;
using TotpGenerator.SampleApi.Contracts;
using TotpGenerator.SampleApi.Enums;
using TotpGenerator.SampleApi.Models;
using TotpGenerator.SampleApi.Options;

namespace TotpGenerator.SampleApi.Services
{
    public class UserService : IUserService
    {
        private readonly UserOption _userOption;
        public UserService(IOptions<UserOption> options)
        {
            _userOption = options.Value;
        }
        public async ValueTask<int> GetTwoFactorTotpCode(string email)
        {
            var user = await GetUserByEmail(email);

            var modifier = GetModifier(user, TotpEnum.TwoFactor);

            var totpCode = TotpService.GenerateCode(user.SecurityStamp.ToString(), modifier);

            return totpCode;
        }

        public async ValueTask<bool> ValidateTwoFactorTotpCode(string email, int totpCode)
        {
            var user = await GetUserByEmail(email);

            string modifier = GetModifier(user, TotpEnum.TwoFactor);

            var isValid = TotpService.ValidateCode(
                user.SecurityStamp.ToString(),
                totpCode,
                modifier,
                _userOption.TwoFactorTotpExpiration);

            return isValid;
        }

        private ValueTask<User> GetUserByEmail(string email)
        {
            var user = new User
            {
                Id = 1,
                Name = "Farhad",
                SecurityStamp = Guid.Parse("87214ac2-ba59-41e7-8d18-e868560925be")
            };

            return ValueTask.FromResult(user);
        }

        private static string GetModifier(User user, TotpEnum totpEnum)
        {
            return $"Totp:{user.Id}:{totpEnum}";
        }

    }
}
