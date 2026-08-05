using System.IdentityModel.Tokens.Jwt;
using Application.Interfaces.Services;
using FluentAssertions;
using Infrastructure.Services.Ai.Runs;
using Moq;

namespace UnitTests;

public class ChatRunExecutorTokenRefresh
{
    private static string TokenExpiringIn(TimeSpan span)
    {
        var jwt = new JwtSecurityToken(expires: DateTime.UtcNow.Add(span));
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    [Fact(DisplayName = "17.9 - Token còn nhiều thời gian thì không mint lại")]
    public void EnsureFreshToken_ConNhieuThoiGian_KhongMintLai()
    {
        var token = TokenExpiringIn(TimeSpan.FromMinutes(30));
        var tokenManagerMock = new Mock<ITokenManagerService>();

        var result = ChatRunExecutor.EnsureFreshToken(token, tokenManagerMock.Object, TimeSpan.FromMinutes(5));

        result.Should().Be(token);
        tokenManagerMock.Verify(
            x => x.RefreshAccessToken(It.IsAny<string>(), It.IsAny<DateTimeOffset>()), Times.Never);
    }

    [Fact(DisplayName = "17.9 - Token còn dưới ngưỡng thì mint lại trước khi gọi sidecar")]
    public void EnsureFreshToken_ConDuoiNguong_MintLaiTruocKhiGoiSidecar()
    {
        var token = TokenExpiringIn(TimeSpan.FromSeconds(30));
        var refreshed = TokenExpiringIn(TimeSpan.FromMinutes(15));
        var tokenManagerMock = new Mock<ITokenManagerService>();
        tokenManagerMock.Setup(x => x.GetAccessTokenExpiryMinutes()).Returns(15);
        tokenManagerMock
            .Setup(x => x.RefreshAccessToken(token, It.IsAny<DateTimeOffset>()))
            .Returns(refreshed);

        var result = ChatRunExecutor.EnsureFreshToken(token, tokenManagerMock.Object, TimeSpan.FromMinutes(5));

        result.Should().Be(refreshed);
        tokenManagerMock.Verify(
            x => x.RefreshAccessToken(token, It.IsAny<DateTimeOffset>()), Times.Once);
    }
}
