using Application.Common.Models;
using Application.Features.Contacts.Commands.CreateContact;
using Application.Features.Contacts.Commands.CreateContactReply;
using Application.Features.Contacts.Commands.UpdateInternalNote;
using Domain.Constants.Permission;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests;

public class Contact : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public Contact(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _output = output;
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(true);
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "CONT_003 - Lấy toàn bộ danh sách liên hệ")]
    public async Task GetContacts_ReturnsAllContacts()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Contacts.AddRange(
                new Domain.Entities.Contact { FullName = "C1", Email = "c1@gmail.com", PhoneNumber = "1", Subject = "S1", Message = "M1" },
                new Domain.Entities.Contact { FullName = "C2", Email = "c2@gmail.com", PhoneNumber = "2", Subject = "S2", Message = "M2" }
            );
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        // Act
        var response = await _client.GetAsync("/api/v1/contacts", TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<List<Domain.Entities.Contact>>(TestContext.Current.CancellationToken).ConfigureAwait(true);
        content.Should().NotBeNull();
        content.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact(DisplayName = "CONT_004 - Quản trị viên phản hồi liên hệ thành công")]
    public async Task CreateReply_ValidRequest_SavesReply()
    {
        // Arrange
        int contactId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var c = new Domain.Entities.Contact { FullName = "Reply Target", Email = "target@gmail.com", PhoneNumber = "123", Subject = "S", Message = "M" };
            db.Contacts.Add(c);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            contactId = c.Id;
        }

        // Authenticate as Admin
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"admin_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "Password123!",
            [PermissionsList.Contacts.Reply],
            CancellationToken.None)
            .ConfigureAwait(true);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, "Password123!", CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var command = new CreateContactReplyCommand { ContactId = contactId, Message = "Admin Reply", MarkAsProcessed = true };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/contacts/reply", command).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var contactWithReplies = await db.Contacts.Include(c => c.Replies).FirstOrDefaultAsync(c => c.Id == contactId, TestContext.Current.CancellationToken).ConfigureAwait(true);
            contactWithReplies!.Replies.Should().HaveCount(1);
            contactWithReplies.Replies.First().Message.Should().Be("Admin Reply");
            contactWithReplies.Status.Should().Be("Processed");
        }
    }

    [Fact(DisplayName = "CONT_009 - Cập nhật ghi chú nội bộ cho khách hàng")]
    public async Task UpdateInternalNote_ValidRequest_UpdatesNote()
    {
        // Arrange
        int contactId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var c = new Domain.Entities.Contact { FullName = "Note Target", Email = "note@gmail.com", PhoneNumber = "123", Subject = "S", Message = "M" };
            db.Contacts.Add(c);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            contactId = c.Id;
        }

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var command = new UpdateInternalNoteCommand { ContactId = contactId, InternalNote = "New Internal Note" };

        // Act
        var response = await _client.PatchAsJsonAsync("/api/v1/contacts/internal-note", command, TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var contact = await db.Contacts.FirstOrDefaultAsync(x => x.Id == contactId, TestContext.Current.CancellationToken).ConfigureAwait(true);
            contact!.InternalNote.Should().Be("New Internal Note");
        }
    }

    [Fact(DisplayName = "CONT_011 - Kiểm tra quan hệ 1-N giữa Liên hệ và Phản hồi")]
    public async Task GetContact_WithMultipleReplies_ReturnsCorrectReplies()
    {
        // Arrange
        int contactId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var c = new Domain.Entities.Contact { FullName = "Multi Reply", Email = "multi@gmail.com", PhoneNumber = "123", Subject = "S", Message = "M" };
            db.Contacts.Add(c);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            contactId = c.Id;

            var user = new ApplicationUser { UserName = "SeedUser", Email = "seed@gmail.com", FullName = "Seed User" };
            db.Users.Add(user);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            
            db.ContactReplies.AddRange(
                new ContactReply { ContactId = contactId, Message = "R1", RepliedById = user.Id },
                new ContactReply { ContactId = contactId, Message = "R2", RepliedById = user.Id }
            );
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }

        // Act
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [],
            CancellationToken.None)
            .ConfigureAwait(true);
        
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.GetAsync($"/api/v1/contacts", TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<List<Domain.Entities.Contact>>(TestContext.Current.CancellationToken).ConfigureAwait(true);
        content.Should().NotBeNull();
        var contact = content!.FirstOrDefault(c => c.Id == contactId);
        contact.Should().NotBeNull();
        contact!.Replies.Should().HaveCount(2);
    }

    [Fact(DisplayName = "CONT_015 - Đảm bảo tính nhất quán của Subject trong liên hệ")]
    public async Task CreateContact_VietnameseSubject_SavesCorrectly()
    {
        // Arrange
        var command = new CreateContactCommand 
        { 
            FullName = "VN User", 
            Email = "vn@gmail.com", 
            PhoneNumber = "123", 
            Subject = "Cần tư vấn xe SH", 
            Message = "Tin nhắn có dấu" 
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/contacts", command).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var contact = await db.Contacts.FirstOrDefaultAsync(c => c.Email == "vn@gmail.com", TestContext.Current.CancellationToken).ConfigureAwait(true);
        contact!.Subject.Should().Be("Cần tư vấn xe SH");
    }
}
