using FluentAssertions;

namespace TotpGenerator.Test
{
    public class TotpServiceTest
    {
        private readonly int _timeoutInMinute = 1;
        [Fact]
        public void When_UserIdentifierIsInvalid_Then_FalseShouldBeReturnedInValidateMethod()
        {
            string modifer = GetNewModifier();

            var securityStamp = Guid.NewGuid().ToString();

            var code = TotpService.GenerateCode(modifer, securityStamp);

            var invalidModifier = GetNewModifier();

            var verify = TotpService.ValidateCode(securityStamp, code, invalidModifier, _timeoutInMinute);

            verify.Should().BeFalse();
        }

        [Fact]
        public void When_UserSecurityStampIsInvalid_Then_FalseShouldBeReturnedInValidateMethod()
        {
            string modifer = GetNewModifier();

            string securityStamp = GetNewSecurityStamp();

            var code = TotpService.GenerateCode(modifer, securityStamp);

            var verify = TotpService.ValidateCode(GetNewSecurityStamp(), code, modifer, _timeoutInMinute);

            verify.Should().BeFalse();
        }

        [Fact]
        public async Task When_ExpirationTimeIsExpired_Then_FalseShouldBeReturnedInValidateMethod()
        {
            string modifer = GetNewModifier();

            string securityStamp = GetNewSecurityStamp();

            var code = TotpService.GenerateCode(modifer, securityStamp);

            await Task.Delay(TimeSpan.FromMinutes(_timeoutInMinute).Add(TimeSpan.FromSeconds(5)));

            var verify = TotpService.ValidateCode(securityStamp, code, modifer, _timeoutInMinute);

            verify.Should().BeFalse();
        }

        [Fact]
        public void When_CodeAndModifierIsValid_Then_TrueShouldBeReturnedInValidateMethod()
        {
            string modifer = GetNewModifier();

            string securityStamp = GetNewSecurityStamp();

            var code = TotpService.GenerateCode(securityStamp, modifer);

            var verify = TotpService.ValidateCode(securityStamp, code, modifer, _timeoutInMinute);

            verify.Should().BeTrue();
        }

        private static string GetNewModifier()
        {
            return $"{Guid.NewGuid()}";
        }

        private static string GetNewSecurityStamp()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
