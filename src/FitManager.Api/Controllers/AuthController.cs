using System.ComponentModel.DataAnnotations;
using FitManager.Api.Auth;
using FitManager.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FitManager.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(UserManager<ApplicationUser> users, TokenIssuer tokenIssuer) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive || await users.IsLockedOutAsync(user) || !await users.CheckPasswordAsync(user, request.Password))
        {
            if (user is not null && user.IsActive && !await users.IsLockedOutAsync(user))
                await users.AccessFailedAsync(user);
            return Unauthorized(new { error = "بيانات الدخول غير صحيحة أو الحساب غير نشط." });
        }
        await users.ResetAccessFailedCountAsync(user);

        var roles = await users.GetRolesAsync(user);
        return Ok(new LoginResponse(tokenIssuer.Issue(user.Id, user.Email!, user.SecurityStamp!, roles), "Bearer", tokenIssuer.LifetimeMinutes,
            user.FullName, user.Email!, roles));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var user = await users.GetUserAsync(User);
        if (user is not null) await users.UpdateSecurityStampAsync(user);
        return NoContent();
    }
}

public sealed record LoginRequest([Required, EmailAddress] string Email, [Required, MinLength(1)] string Password);
public sealed record LoginResponse(string AccessToken, string TokenType, int ExpiresInMinutes,
    string FullName, string Email, IEnumerable<string> Roles);
