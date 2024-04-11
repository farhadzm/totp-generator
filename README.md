You can generate `Totp` code based on time, `UserSecurityStamp` and `UserIdentifier` and `ExpirationTime`

Nuget:
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

