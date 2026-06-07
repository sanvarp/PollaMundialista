using PollaMundialista.Application.Auth;

using Xunit;

namespace PollaMundialista.Tests;

public class AuthValidatorTests
{
    private readonly RegisterRequestValidator _register = new();
    private readonly LoginRequestValidator _login = new();

    [Theory]
    [InlineData("pepe@correo.com", true)]
    [InlineData("a@b.co", true)]
    [InlineData("user.name+tag@sub.domain.io", true)]
    [InlineData("a@b", false)]        // no TLD
    [InlineData("a@b.", false)]       // trailing dot, no TLD
    [InlineData("abc", false)]        // no @
    [InlineData("@b.com", false)]     // missing local part
    [InlineData("a@b@c.com", false)]  // two @
    public void Email_requires_a_real_tld(string email, bool expectedValid)
    {
        var register = _register.Validate(new RegisterRequest(email, "Passw0rd", "Tester"));
        var login = _login.Validate(new LoginRequest(email, "x"));

        Assert.Equal(expectedValid, register.IsValid);
        Assert.Equal(expectedValid, login.IsValid);
    }
}