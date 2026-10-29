using System.ComponentModel.DataAnnotations;
using FitManager.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin/coaches")]
public sealed class CoachesController(UserManager<ApplicationUser> users) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CoachResponse>>> List(CancellationToken cancellationToken)
    {
        var candidates = await users.Users.OrderBy(user => user.FullName).ToListAsync(cancellationToken);
        var coaches = new List<CoachResponse>();
        foreach (var user in candidates)
            if (await users.IsInRoleAsync(user, "Coach"))
                coaches.Add(new CoachResponse(user.Id, user.FullName, user.Email ?? string.Empty, user.IsActive, user.CreatedAt));
        return Ok(coaches);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CoachResponse>> Get(string id)
    {
        var user = await users.FindByIdAsync(id);
        if (user is null || !await users.IsInRoleAsync(user, "Coach")) return NotFound();
        return Ok(ToResponse(user));
    }

    [HttpPost]
    public async Task<ActionResult<CoachResponse>> Create(CreateCoachRequest request)
    {
        var user = new ApplicationUser { UserName = request.Email, Email = request.Email, FullName = request.FullName.Trim(), IsActive = true };
        var created = await users.CreateAsync(user, request.TemporaryPassword);
        if (!created.Succeeded) return BadRequest(new { errors = created.Errors.Select(error => error.Description) });

        var assigned = await users.AddToRoleAsync(user, "Coach");
        if (!assigned.Succeeded)
        {
            await users.DeleteAsync(user);
            return BadRequest(new { errors = assigned.Errors.Select(error => error.Description) });
        }

        var response = ToResponse(user);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CoachResponse>> Update(string id, UpdateCoachRequest request)
    {
        var user = await users.FindByIdAsync(id);
        if (user is null || !await users.IsInRoleAsync(user, "Coach")) return NotFound();

        user.FullName = request.FullName.Trim();
        user.Email = request.Email.Trim();
        user.NormalizedEmail = users.NormalizeEmail(user.Email);
        user.UserName = request.Email.Trim();
        user.NormalizedUserName = users.NormalizeName(user.UserName);
        user.IsActive = request.IsActive;
        var updated = await users.UpdateAsync(user);
        if (!updated.Succeeded) return BadRequest(new { errors = updated.Errors.Select(error => error.Description) });
        if (!request.IsActive) await users.UpdateSecurityStampAsync(user);

        return Ok(ToResponse(user));
    }

    private static CoachResponse ToResponse(ApplicationUser user) =>
        new(user.Id, user.FullName, user.Email ?? string.Empty, user.IsActive, user.CreatedAt);
}

public sealed record CreateCoachRequest(
    [Required, StringLength(160, MinimumLength = 2)] string FullName,
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, MinLength(10)] string TemporaryPassword);

public sealed record UpdateCoachRequest(
    [Required, StringLength(160, MinimumLength = 2)] string FullName,
    [Required, EmailAddress, StringLength(256)] string Email,
    bool IsActive);

public sealed record CoachResponse(string Id, string FullName, string Email, bool IsActive, DateTimeOffset CreatedAt);
