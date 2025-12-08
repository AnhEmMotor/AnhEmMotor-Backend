using Application.ApiContracts.Auth.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    IIdentityService identityService,
    ITokenManagerService tokenService,
    IHttpTokenAccessorService httpTokenAccessor)
    : IRequestHandler<RefreshTokenCommand, GetAccessTokenFromRefreshTokenResponse>
{
    public async Task<GetAccessTokenFromRefreshTokenResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Lấy Token từ Cookie
        var currentRefreshToken = httpTokenAccessor.GetRefreshTokenFromCookie();
        if (string.IsNullOrEmpty(currentRefreshToken))
        {
            throw new UnauthorizedException("Refresh token is missing.");
        }

        // 2. Validate Token & User (Logic check status/deleted/expired đẩy hết vào đây)
        var user = await identityService.GetUserByRefreshTokenAsync(currentRefreshToken);

        // 3. Security Check: So sánh Status trong Access Token cũ (nếu có) với Status hiện tại
        // Để ngăn chặn trường hợp user bị lock nhưng vẫn dùng Access Token cũ để request
        var authHeader = httpTokenAccessor.GetAuthorizationValueFromHeader();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var oldAccessToken = authHeader["Bearer ".Length..];
            var oldStatusClaim = tokenService.GetClaimFromToken(oldAccessToken, "status");

            if (!string.IsNullOrEmpty(oldStatusClaim) &&
                !string.Equals(oldStatusClaim, user.Status, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedException("User status has changed. Please login again.");
            }
        }

        // 4. Tính toán thời gian (Đồng bộ tuyệt đối)
        var accessExpiryMinutes = tokenService.GetAccessTokenExpiryMinutes();
        var refreshExpiryDays = tokenService.GetRefreshTokenExpiryDays();

        var now = DateTimeOffset.UtcNow;
        var accessTokenExpiresAt = now.AddMinutes(accessExpiryMinutes);
        var refreshTokenExpiresAt = now.AddDays(refreshExpiryDays);

        // 5. Tạo Token mới
        var newAccessToken = await tokenService.CreateAccessTokenAsync(user, accessTokenExpiresAt, cancellationToken);
        var newRefreshToken = tokenService.CreateRefreshToken();

        // 6. Lưu xuống DB
        await identityService.UpdateRefreshTokenAsync(user.Id, newRefreshToken, refreshTokenExpiresAt);

        // 7. Lưu xuống Cookie (Dùng đúng thời gian của DB)
        httpTokenAccessor.SetRefreshTokenFromCookie(newRefreshToken, refreshTokenExpiresAt);

        return new GetAccessTokenFromRefreshTokenResponse
        {
            AccessToken = newAccessToken,
            ExpiresAt = accessTokenExpiresAt
        };
    }
}