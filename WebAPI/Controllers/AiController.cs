using Infrastructure.Services.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using Infrastructure.DBContexts;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController(
    IAiAgentClient aiAgentClient,
    ApplicationDBContext dbContext,
    RoleManager<ApplicationRole> roleManager) : ControllerBase
{
    [HttpPost("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromBody] AiSearchRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var response = await aiAgentClient.ChatSearchAsync(request.Keyword, userId);
        return Ok(response);
    }

    /// <summary>
    /// WARNING: API này chỉ được sử dụng cho mục đích kiểm thử (testing) việc tích hợp với AI Sidecar.
    /// Vui lòng không sử dụng trong môi trường Production.
    /// </summary>
    [HttpPost("test-roles")]
    [Authorize]
    public async Task<IActionResult> TestRoles()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        
        // Lấy danh sách ID của các Roles người dùng đang có
        var roleIds = await roleManager.Roles
            .Where(r => r.Name != null && userRoles.Contains(r.Name))
            .Select(r => r.Id)
            .ToListAsync();

        // Lấy tất cả các Permission của các Role đó
        var permissions = await dbContext.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId) && rp.Permission != null)
            .Select(rp => rp.Permission!.Name)
            .Distinct()
            .ToListAsync();
        
        var response = await aiAgentClient.TestRoleAsync(userId, permissions.ToArray());
        return Ok(response);
    }
}

public class AiSearchRequest
{
    public string Keyword { get; set; } = string.Empty;
}
