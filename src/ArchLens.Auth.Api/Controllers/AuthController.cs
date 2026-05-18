using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ArchLens.Auth.Application.UseCases.Auth.Commands.DeleteAccount;
using ArchLens.Auth.Application.UseCases.Auth.Commands.Login;
using ArchLens.Auth.Application.UseCases.Auth.Commands.Register;
using ArchLens.Auth.Application.UseCases.Auth.Queries.ExportUserData;
using ArchLens.Auth.Domain.Entities.UserEntities;
using ArchLens.Auth.Domain.Interfaces.UserInterfaces;
using ArchLens.SharedKernel.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ArchLens.Auth.Api.Controllers;

[ApiController]
[Route("auth")]
[EnableRateLimiting("auth-strict")]
public sealed class AuthController(IMediator mediator, IUserRepository userRepository, IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(null, result.Value)
            : BadRequest(new { error = result.Error.Code, message = result.Error.Description });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : Unauthorized(new { error = result.Error.Code, message = result.Error.Description });
    }

    [HttpGet("me/data")]
    [Authorize]
    public async Task<IActionResult> ExportMyData(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await mediator.Send(new ExportUserDataQuery(userId.Value), ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error.Code, message = result.Error.Description });
    }

    [HttpDelete("me")]
    [Authorize]
    public async Task<IActionResult> DeleteMyAccount(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await mediator.Send(new DeleteAccountCommand(userId.Value), ct);
        return result.IsSuccess
            ? NoContent()
            : NotFound(new { error = result.Error.Code, message = result.Error.Description });
    }

    [HttpPost("seed-admin")]
    [EnableRateLimiting("auth-strict")]
    public async Task<IActionResult> SeedAdmin([FromBody] SeedAdminRequest request, CancellationToken ct)
    {
        var seedKey = Environment.GetEnvironmentVariable("ADMIN_SEED_KEY")
            ?? throw new InvalidOperationException("ADMIN_SEED_KEY environment variable is required.");
        var seedKeyBytes = Encoding.UTF8.GetBytes(seedKey);
        var requestKeyBytes = Encoding.UTF8.GetBytes(request.SeedKey ?? string.Empty);
        if (!CryptographicOperations.FixedTimeEquals(seedKeyBytes, requestKeyBytes))
            return Unauthorized(new { error = "InvalidSeedKey", message = "Invalid seed key." });

        var user = await userRepository.GetByUsernameAsync(request.Username, ct);
        if (user is null)
            return NotFound(new { error = "UserNotFound", message = "User not found." });

        user.PromoteToAdmin();
        await userRepository.UpdateAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Ok(new { message = $"User '{request.Username}' promoted to Admin." });
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

public sealed record SeedAdminRequest(string Username, string SeedKey);
