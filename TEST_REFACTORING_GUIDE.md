# Hướng dẫn cập nhật Unit Tests sau khi refactor

## Tổng quan
Sau khi refactor để loại bỏ `UserManager<ApplicationUser>` và `RoleManager<ApplicationRole>` khỏi Application layer, các unit tests cần được cập nhật để mock các repository interfaces mới thay vì mock Identity classes.

## Patterns cần thay đổi

### 1. Setup Mock Objects

**Trước:**
```csharp
private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

public MyTest()
{
    var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
    _userManagerMock = new Mock<UserManager<ApplicationUser>>(
        userStoreMock.Object, null, null, null, null, null, null, null, null);
}
```

**Sau:**
```csharp
private readonly Mock<IUserReadRepository> _userReadRepositoryMock;
private readonly Mock<IUserUpdateRepository> _userUpdateRepositoryMock;
private readonly Mock<IUserCreateRepository> _userCreateRepositoryMock;
private readonly Mock<IUserDeleteRepository> _userDeleteRepositoryMock;

public MyTest()
{
    _userReadRepositoryMock = new Mock<IUserReadRepository>();
    _userUpdateRepositoryMock = new Mock<IUserUpdateRepository>();
    _userCreateRepositoryMock = new Mock<IUserCreateRepository>();
    _userDeleteRepositoryMock = new Mock<IUserDeleteRepository>();
}
```

### 2. Mock FindByIdAsync

**Trước:**
```csharp
_userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
    .ReturnsAsync(user);
```

**Sau:**
```csharp
_userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
    .ReturnsAsync(user);
```

### 3. Mock UpdateAsync

**Trước:**
```csharp
_userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
    .ReturnsAsync(IdentityResult.Success);
```

**Sau:**
```csharp
_userUpdateRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync((true, Array.Empty<string>()));
```

### 4. Mock ChangePasswordAsync

**Trước:**
```csharp
_userManagerMock.Setup(x => x.ChangePasswordAsync(user, "oldPass", "newPass"))
    .ReturnsAsync(IdentityResult.Success);
```

**Sau:**
```csharp
_userUpdateRepositoryMock.Setup(x => x.ChangePasswordAsync(user, "oldPass", "newPass", It.IsAny<CancellationToken>()))
    .ReturnsAsync((true, Array.Empty<string>()));
```

### 5. Mock GetRolesAsync

**Trước:**
```csharp
_userManagerMock.Setup(x => x.GetRolesAsync(user))
    .ReturnsAsync(new List<string> { "Admin" });
```

**Sau:**
```csharp
_userReadRepositoryMock.Setup(x => x.GetUserRolesAsync(user, It.IsAny<CancellationToken>()))
    .ReturnsAsync(new List<string> { "Admin" });
```

### 6. Mock RoleExistsAsync

**Trước:**
```csharp
_roleManagerMock.Setup(x => x.RoleExistsAsync("Admin"))
    .ReturnsAsync(true);
```

**Sau:**
```csharp
_userManagerReadRepositoryMock.Setup(x => x.RoleExistsAsync("Admin", It.IsAny<CancellationToken>()))
    .ReturnsAsync(true);
```

### 7. Mock GetUsersInRoleAsync

**Trước:**
```csharp
_userManagerMock.Setup(x => x.GetUsersInRoleAsync("Admin"))
    .ReturnsAsync(new List<ApplicationUser> { user });
```

**Sau:**
```csharp
_userReadRepositoryMock.Setup(x => x.GetUsersInRoleAsync("Admin", It.IsAny<CancellationToken>()))
    .ReturnsAsync(new List<ApplicationUser> { user });
```

### 8. Mock CreateAsync

**Trước:**
```csharp
_userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "password"))
    .ReturnsAsync(IdentityResult.Success);
```

**Sau:**
```csharp
_userCreateRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<ApplicationUser>(), "password", It.IsAny<CancellationToken>()))
    .ReturnsAsync((true, Array.Empty<string>()));
```

### 9. Mock AddToRoleAsync / AddToRolesAsync

**Trước:**
```csharp
_userManagerMock.Setup(x => x.AddToRoleAsync(user, "Admin"))
    .ReturnsAsync(IdentityResult.Success);
```

**Sau:**
```csharp
_userCreateRepositoryMock.Setup(x => x.AddUserToRoleAsync(user, "Admin", It.IsAny<CancellationToken>()))
    .ReturnsAsync((true, Array.Empty<string>()));
```

### 10. Mock RemoveFromRolesAsync

**Trước:**
```csharp
_userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, roles))
    .ReturnsAsync(IdentityResult.Success);
```

**Sau:**
```csharp
_userUpdateRepositoryMock.Setup(x => x.RemoveUserFromRolesAsync(user, roles, It.IsAny<CancellationToken>()))
    .ReturnsAsync((true, Array.Empty<string>()));
```

### 11. Handler Constructors

**Trước:**
```csharp
var handler = new UpdateCurrentUserCommandHandler(_userManagerMock.Object);
```

**Sau:**
```csharp
var handler = new UpdateCurrentUserCommandHandler(
    _userReadRepositoryMock.Object, 
    _userUpdateRepositoryMock.Object);
```

### 12. Verify Calls

**Trước:**
```csharp
_userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
```

**Sau:**
```csharp
_userUpdateRepositoryMock.Verify(x => x.UpdateUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()), Times.Once);
```

## Ví dụ hoàn chỉnh

**File User.cs - Test cập nhật thông tin người dùng**

```csharp
[Fact(DisplayName = "USER_005 - Cập nhật thông tin người dùng thành công")]
public async Task UpdateCurrentUser_Success_ReturnsUpdatedUser()
{
    // Arrange
    var userId = Guid.NewGuid();
    var user = new ApplicationUser
    {
        Id = userId,
        UserName = "testuser",
        Email = "test@example.com",
        FullName = "Old Name",
        Gender = GenderStatus.Male,
        PhoneNumber = "0123456789",
        Status = UserStatus.Active,
        DeletedAt = null
    };

    _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(user);
    _userReadRepositoryMock.Setup(x => x.GetUserRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<string>());
    _userUpdateRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((true, Array.Empty<string>()));

    var handler = new UpdateCurrentUserCommandHandler(
        _userReadRepositoryMock.Object,
        _userUpdateRepositoryMock.Object);
        
    var request = new UpdateUserRequest
    {
        FullName = "New Name",
        Gender = GenderStatus.Female,
        PhoneNumber = "0987654321"
    };
    var command = new UpdateCurrentUserCommand(userId.ToString(), request);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.FullName.Should().Be("New Name");
    result.Gender.Should().Be(GenderStatus.Female);
    result.PhoneNumber.Should().Be("0987654321");
    
    _userUpdateRepositoryMock.Verify(x => x.UpdateUserAsync(
        It.Is<ApplicationUser>(u => u.FullName == "New Name"), 
        It.IsAny<CancellationToken>()), Times.Once);
}
```

## Files cần cập nhật

1. **UnitTests/Auth.cs** - ✅ ĐÃ SỬA
2. **UnitTests/User.cs** - CẦN SỬA (~600 dòng, nhiều tests)
3. **UnitTests/UserManager.cs** - CẦN SỬA (~180 dòng)
4. **ControllerTests/** - CẦN REVIEW
5. **IntegrationTests/** - CẦN REVIEW

## Khuyến nghị

Do số lượng tests lớn, bạn có thể:
1. Tạm comment out các tests đang fail để project build được
2. Từ từ uncomment và fix từng test một theo pattern trên
3. Hoặc sử dụng find/replace với regex trong IDE để thay thế hàng loạt

Bạn có muốn tôi tiếp tục fix tất cả tests hay bạn muốn tự làm dựa trên hướng dẫn này?
