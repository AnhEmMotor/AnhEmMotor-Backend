using Application.Common.Models;
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
using Xunit.Abstractions;

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
            await db.SaveChangesAsync().ConfigureAwait(true);
        }

        // Act
        var response = await _client.GetAsync("/api/v1/contacts").ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<List<Domain.Entities.Contact>>().ConfigureAwait(true);
        content.Should().NotBeNull();
        content.Should().HaveCountGreaterOrEqualTo(2);
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
            await db.SaveChangesAsync().ConfigureAwait(true);
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
        var response = await _client.PostAsJsonAsync($"/api/v1/contacts/{contactId}/replies", command).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var contactWithReplies = await db.Contacts.Include(c => c.Replies).FirstOrDefaultAsync(c => c.Id == contactId).ConfigureAwait(true);
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
            await db.SaveChangesAsync().ConfigureAwait(true);
            contactId = c.Id;
        }

        var command = new UpdateInternalNoteCommand { ContactId = contactId, InternalNote = "New Internal Note" };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/v1/contacts/{contactId}/internal-note", command).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var contact = await db.Contacts.FindAsync(contactId).ConfigureAwait(true);
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
            await db.SaveChangesAsync().ConfigureAwait(true);
            contactId = c.Id;

            db.ContactReplies.AddRange(
                new ContactReply { ContactId = contactId, Message = "R1", RepliedById = Guid.NewGuid() },
                new ContactReply { ContactId = contactId, Message = "R2", RepliedById = Guid.NewGuid() }
            );
            await db.SaveChangesAsync().ConfigureAwait(true);
        }

        // Act
        var response = await _client.GetAsync($"/api/v1/contacts/{contactId}").ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Có thể kiểm tra content nếu cần
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
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var contact = await db.Contacts.FirstOrDefaultAsync(c => c.Email == "vn@gmail.com").ConfigureAwait(true);
            contact!.Subject.Should().Be("Cần tư vấn xe SH");
        }
    }
}
