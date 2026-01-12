using Application.ApiContracts.Auth.Requests;
using Application.ApiContracts.Auth.Responses;
using Application.Common.Constants;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Mapster;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    ITokenManagerService tokenService,
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository,
    IHttpTokenAccessorService httpTokenAccessor) // Vẫn giữ để set cookie response
    : IRequestHandler<RefreshTokenCommand, Result<GetAccessTokenFromRefreshTokenResponse>>
{
    public async Task<Result<GetAccessTokenFromRefreshTokenResponse>> Handle(
        RefreshTokenCommand request, // Dùng request, không dùng accessor để get
        CancellationToken cancellationToken)
    {
        // 1. Validation Logic: Đã bị loại bỏ nhờ FluentValidation pipeline
        // Không còn check string.IsNullOrEmpty(request.RefreshToken)

        // 2. Database Logic (Bắt buộc ở lại Handler)
        var user = await userReadRepository.GetByRefreshTokenAsync(request.RefreshToken!, cancellationToken)
            .ConfigureAwait(false);

        if (user == null)
        {
            // Đây là Logic check data tồn tại, Validator KHÔNG làm được
            return Error.Unauthorized("Invalid refresh token.");
        }

        // 3. Domain Logic (Bắt buộc ở lại Handler)
        if (user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
        {
            return Error.Unauthorized("Refresh token has expired. Please login again.");
        }

        if (user.Status != "Active" || user.DeletedAt != null)
        {
            return Error.Forbidden("Account is not available.");
        }

        // 4. Business Logic phức tạp (Check khớp token cũ)
        if (!string.IsNullOrEmpty(request.AccessToken))
        {
            var oldStatusClaim = tokenService.GetClaimFromToken(request.AccessToken, ClaimJWTPayload.Status);

            // Logic so sánh trạng thái user trong token cũ và DB
            if (!string.IsNullOrEmpty(oldStatusClaim) &&
                !string.Equals(oldStatusClaim, user.Status, StringComparison.OrdinalIgnoreCase))
            {
                return Error.Unauthorized("User status has changed. Please login again.");
            }
        }

        // 5. Execution Logic (Tạo token mới)
        var userDto = user.Adapt<UserAuthDTO>();
        var accessExpiryMinutes = tokenService.GetAccessTokenExpiryMinutes();
        var refreshExpiryDays = tokenService.GetRefreshTokenExpiryDays();
        var now = DateTimeOffset.UtcNow;

        var accessTokenExpiresAt = now.AddMinutes(accessExpiryMinutes);
        var refreshTokenExpiresAt = now.AddDays(refreshExpiryDays);

        var newAccessToken = tokenService.CreateAccessToken(userDto, accessTokenExpiresAt);
        var newRefreshToken = tokenService.CreateRefreshToken();

        // 6. Side Effect (Update DB)
        await userUpdateRepository.UpdateRefreshTokenAsync(
            user.Id,
            newRefreshToken,
            refreshTokenExpiresAt,
            cancellationToken)
            .ConfigureAwait(false);

        // 7. Side Effect (Set Cookie - Output)
        httpTokenAccessor.SetRefreshTokenToCookie(newRefreshToken, refreshTokenExpiresAt);

        return new GetAccessTokenFromRefreshTokenResponse
        {
            AccessToken = newAccessToken,
            ExpiresAt = accessTokenExpiresAt
        };
    }
}