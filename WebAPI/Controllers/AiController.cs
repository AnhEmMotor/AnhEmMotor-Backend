using Application.Features.Ai;
using Application.Interfaces.Repositories.Ai;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController(
    IAiSearchClient aiSearchClient,
    IAiTestRoleClient aiTestRoleClient,
    ApplicationDBContext dbContext,
    RoleManager<ApplicationRole> roleManager) : ControllerBase
{
    [HttpPost("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromBody] AiSearchRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var brandNames = await dbContext.Brands.Where(b => b.Name != null).Select(b => b.Name!).ToListAsync();
        var categoryNames = await dbContext.ProductCategories
            .Where(c => c.Name != null)
            .Select(c => c.Name!)
            .ToListAsync();
        var vehicleTypeNames = await dbContext.OptionValues
            .Include(ov => ov.Option)
            .Where(ov => ov.Option != null && ov.Option.Name == "VehicleType" && ov.Name != null)
            .Select(ov => ov.Name!)
            .ToListAsync();
        var aiResult = AiSearchRuleParser.TryParse(request.Keyword, brandNames, categoryNames, vehicleTypeNames);
        if (aiResult == null)
        {
            var response = await aiSearchClient.ChatSearchAsync(request.Keyword, userId);
            if (response.Result == null || response.Status != "success")
            {
                return BadRequest(new { Message = "Lỗi khi gọi AI Sidecar" });
            }
            aiResult = response.Result;
        }
        int? categoryId = null;
        if (!string.IsNullOrEmpty(aiResult.Category))
        {
            var cat = await dbContext.ProductCategories
                .FirstOrDefaultAsync(c => EF.Functions.Like(c.Name, $"%{aiResult.Category}%"));
            if (cat != null)
                categoryId = cat.Id;
        }
        int? brandId = null;
        if (!string.IsNullOrEmpty(aiResult.Brand))
        {
            var brand = await dbContext.Brands
                .FirstOrDefaultAsync(b => EF.Functions.Like(b.Name, $"%{aiResult.Brand}%"));
            if (brand != null)
                brandId = brand.Id;
        }
        int? optionValueId = null;
        if (!string.IsNullOrEmpty(aiResult.VehicleType))
        {
            var opt = await dbContext.OptionValues
                .Include(ov => ov.Option)
                .FirstOrDefaultAsync(
                    ov => ov.Option != null &&
                        ov.Option.Name == "VehicleType" &&
                        EF.Functions.Like(ov.Name, $"%{aiResult.VehicleType}%"));
            if (opt != null)
                optionValueId = opt.Id;
        }
        var queryParams = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(aiResult.Keyword))
            queryParams["search"] = aiResult.Keyword;
        if (categoryId.HasValue)
            queryParams["category_ids"] = categoryId.Value.ToString();
        if (brandId.HasValue)
            queryParams["brand_ids"] = brandId.Value.ToString();
        if (optionValueId.HasValue)
            queryParams["optionValueIds"] = optionValueId.Value.ToString();
        if (aiResult.PriceMin > 0)
            queryParams["minPrice"] = aiResult.PriceMin;
        if (aiResult.PriceMax > 0 && aiResult.PriceMax < 60000000)
            queryParams["maxPrice"] = aiResult.PriceMax;
        if (aiResult.Colors != null && aiResult.Colors.Any())
            queryParams["colors"] = string.Join(",", aiResult.Colors);
        return Ok(new { IsSuccess = true, RedirectUrl = "/products", QueryParams = queryParams });
    }

    /// <summary>
    /// WARNING: API này chỉ được sử dụng cho mục đích kiểm thử (testing) việc tích hợp với AI Sidecar. Vui lòng không
    /// sử dụng trong môi trường Production.
    /// </summary>
    [HttpPost("test-roles")]
    [Authorize]
    public async Task<IActionResult> TestRoles()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        var roleIds = await roleManager.Roles
            .Where(r => r.Name != null && userRoles.Contains(r.Name))
            .Select(r => r.Id)
            .ToListAsync();
        var permissions = await dbContext.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId) && rp.Permission != null)
            .Select(rp => rp.Permission!.Name)
            .Distinct()
            .ToListAsync();
        var response = await aiTestRoleClient.TestRoleAsync(userId, permissions.ToArray());
        return Ok(response);
    }
}

public class AiSearchRequest
{
    public string Keyword { get; set; } = string.Empty;
}
