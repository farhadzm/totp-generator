You can generate `Totp` code based on time, `UserSecurityStamp` and `UserIdentifier` and `ExpirationTime`.

You can use this package instead of  [`Rfc6238AuthenticationService`](https://github.com/dotnet/aspnetcore/blob/ce16ff0a51a74811674228835696e5cc78494fd7/src/Identity/Extensions.Core/src/Rfc6238AuthenticationService.cs) class for generating `totp` code with identity.

[NuGet:](https://www.nuget.org/packages/TotpGenerator/)
```xml
<PackageReference Include="TotpGenerator" Version="1.0.1" />
```
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

private static string GetModifier(User user, TotpEnum totpEnum)
{
    return $"Totp:{user.Id}:{totpEnum}";
}
```
TotpEnum:
```CSharp
public enum TotpEnum : byte
{
    TwoFactor = 1,
    Activation = 2,
    Deactivation = 4
}

```

