using TotpGenerator.SampleApi.Models;

namespace TotpGenerator.SampleApi.Contracts
{
    public interface IUserService
    {
        ValueTask<int> GetTwoFactorTotpCode(string email);

        ValueTask<bool> ValidateTwoFactorTotpCode(string email, int totpCode);
    }
}
