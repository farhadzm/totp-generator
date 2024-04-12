You can generate `Totp` code based on time, `UserSecurityStamp` and `UserIdentifier` and `ExpirationTime`.

You can use this package instead of  [Rfc6238AuthenticationService](https://github.com/dotnet/aspnetcore/blob/ce16ff0a51a74811674228835696e5cc78494fd7/src/Identity/Extensions.Core/src/Rfc6238AuthenticationService.cs) class for generating `totp` code with identity.

[NuGet:](https://www.nuget.org/packages/TotpGenerator/)
```xml
<PackageReference Include="TotpGenerator" Version="1.0.2" />
```
![image](https://github.com/farhadzm/totp-generator/assets/48260228/2e8bdddb-b9fa-40f4-9de4-237342f25d30)

**Usage:**
```CSharp
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
```
If you want to modify the TOTP code method generated by Identity, you need to follow these steps:

First, create a class that inherits from the [TotpSecurityStampBasedTokenProvider](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.totpsecuritystampbasedtokenprovider-1) class. Then, you should override the GenerateAsync and ValidateAsync methods and use [TotpService](https://github.com/farhadzm/totp-generator/blob/master/TotpGenerator/TotpService.cs) instead of [Rfc6238AuthenticationService](https://github.com/dotnet/aspnetcore/blob/ce16ff0a51a74811674228835696e5cc78494fd7/src/Identity/Extensions.Core/src/Rfc6238AuthenticationService.cs).

GenerateAsync method:
```CSharp
public override async Task<string> GenerateAsync(
    string purpose,
    UserManager<ApplicationUser> manager,
    ApplicationUser user)
{
    if (manager == null)
    {
        throw new ArgumentNullException(nameof(manager));
    }
    var token = await manager.CreateSecurityTokenAsync(user);
    var modifier = await GetUserModifierAsync(purpose, manager, user);

    return TotpService.GenerateCode(token, modifier).ToString("D6", CultureInfo.InvariantCulture);
}
```
ValidateAsync method:
```CSharp
public override async Task<bool> ValidateAsync(
    string purpose,
    string token,
    UserManager<ApplicationUser> manager,
    ApplicationUser user)
{
    if (manager == null)
    {
        throw new ArgumentNullException(nameof(manager));
    }
    int code;
    if (!int.TryParse(token, out code))
    {
        return false;
    }
    var securityToken = await manager.CreateSecurityTokenAsync(user);
    var modifier = await GetUserModifierAsync(purpose, manager, user);

    return securityToken != null && TotpService.ValidateCode(securityToken, code, modifier, _userOption.TwoFactorTotpExpiration);
}
```
Then, introduce the created class to Identity: 
```CSharp
services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<IdentityContext>()
    .AddDefaultTokenProviders()
    .AddTokenProvider<CustomTotpSecurityStampBasedTokenProvider >("CustomTotp");
```
And when generating and validating the code, you should send its provider name to the UserManager: 
```CSharp
var verificationCode = await _userManager.GenerateTwoFactorTokenAsync(user, "CustomTotp");
```
```CSharp
var verify = await _userManager.VerifyTwoFactorTokenAsync(user, "CustomTotp", request.VerificationCode);
```
