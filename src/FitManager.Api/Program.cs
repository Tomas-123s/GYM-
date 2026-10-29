using System.Text;
using FitManager.Api.Auth;
using FitManager.Infrastructure;
using FitManager.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration is required.");
if (Encoding.UTF8.GetByteCount(jwt.SigningKey) < 32)
    throw new InvalidOperationException("Jwt:SigningKey must contain at least 32 bytes; set it through user-secrets or an environment variable.");
if (string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience))
    throw new InvalidOperationException("Jwt:Issuer and Jwt:Audience are required.");
if (jwt.LifetimeMinutes is < 1 or > 1440)
    throw new InvalidOperationException("Jwt:LifetimeMinutes must be between 1 and 1440.");

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddSingleton<TokenIssuer>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.GetUserAsync(context.Principal!);
                var presentedStamp = context.Principal?.FindFirst("security_stamp")?.Value;
                if (user is null || !user.IsActive || !string.Equals(user.SecurityStamp, presentedStamp, StringComparison.Ordinal))
                    context.Fail("The account is inactive or the token has been revoked.");
            }
        };
    });
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());
builder.Services.AddProblemDetails();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
if (app.Environment.IsDevelopment()) app.MapOpenApi().AllowAnonymous();

if (builder.Configuration.GetValue<bool>("Seed:Admin:Enabled"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "Admin", "Coach" })
        if (!await roleManager.RoleExistsAsync(role))
            (await roleManager.CreateAsync(new IdentityRole(role))).SucceedOrThrow();

    var email = builder.Configuration["Seed:Admin:Email"];
    var password = builder.Configuration["Seed:Admin:Password"];
    var fullName = builder.Configuration["Seed:Admin:FullName"];
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName))
        throw new InvalidOperationException("Seed:Admin:Email, Password, and FullName are required when Seed:Admin:Enabled is true.");
    if (await userManager.FindByEmailAsync(email) is null)
    {
        var admin = new ApplicationUser { UserName = email, Email = email, FullName = fullName, IsActive = true, EmailConfirmed = true };
        var created = await userManager.CreateAsync(admin, password);
        if (!created.Succeeded) throw new InvalidOperationException(string.Join("; ", created.Errors.Select(error => error.Description)));
        var assigned = await userManager.AddToRoleAsync(admin, "Admin");
        if (!assigned.Succeeded) throw new InvalidOperationException(string.Join("; ", assigned.Errors.Select(error => error.Description)));
    }
}

app.Run();

public partial class Program;

internal static class IdentityResultExtensions
{
    public static void SucceedOrThrow(this IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
    }
}
