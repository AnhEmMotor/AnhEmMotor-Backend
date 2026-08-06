using Application.Common.Models;
using Application.Interfaces.Repositories.Lead.Lead;
using Application.Interfaces.Repositories.Role;
using Application.Interfaces.Repositories.User;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using System.Text.RegularExpressions;

namespace Application.Features.Leads.Commands.SyncLeadsToUsers;

public class SyncLeadsToUsersCommandHandler(
    ILeadReadRepository leadReadRepository,
    IUserReadRepository userReadRepository,
    IUserCreateRepository userCreateRepository,
    IRoleReadRepository roleReadRepository,
    IRoleInsertRepository roleInsertRepository) : IRequestHandler<SyncLeadsToUsersCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SyncLeadsToUsersCommand request, CancellationToken cancellationToken)
    {
        int count = 0;
        
        var customerRole = "Customer";
        if (!await roleReadRepository.IsRoleExistAsync(customerRole, cancellationToken).ConfigureAwait(false))
        {
            await roleInsertRepository.CreateAsync(new ApplicationRole { Name = customerRole }, cancellationToken).ConfigureAwait(false);
        }

        var leads = await leadReadRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        foreach (var lead in leads)
        {
            string email = string.IsNullOrWhiteSpace(lead.Email) ? "" : lead.Email.Trim();
            string phone = string.IsNullOrWhiteSpace(lead.PhoneNumber) ? "" : lead.PhoneNumber.Trim();

            string username = string.IsNullOrEmpty(phone) ? email : phone;
            if (!string.IsNullOrEmpty(username))
            {
                username = Regex.Replace(username, @"[^a-zA-Z0-9_\-\.@]", "");
            }
            if (string.IsNullOrEmpty(username))
            {
                username = $"customer_{lead.Id}";
            }

            var existingUser = await userReadRepository.FindUserByUsernameAsync(username, cancellationToken).ConfigureAwait(false);
            if (existingUser == null && !string.IsNullOrEmpty(email))
            {
                existingUser = await userReadRepository.FindUserByEmailAsync(email, cancellationToken).ConfigureAwait(false);
            }

            if (existingUser == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = username,
                    Email = string.IsNullOrEmpty(email) ? $"{username}@anhemmotor.com" : email,
                    FullName = lead.FullName,
                    PhoneNumber = phone,
                    Status = UserStatus.Active,
                    Gender = lead.Gender ?? GenderStatus.Other
                };

                var (succeeded, _) = await userCreateRepository.CreateUserAsync(newUser, "Khachhang@123", cancellationToken).ConfigureAwait(false);
                if (succeeded)
                {
                    await userCreateRepository.AddUserToRoleAsync(newUser, customerRole, cancellationToken).ConfigureAwait(false);
                    count++;
                }
            }
            else
            {
                var roles = await userReadRepository.GetUserRolesAsync(existingUser, cancellationToken).ConfigureAwait(false);
                if (!roles.Contains(customerRole))
                {
                    await userCreateRepository.AddUserToRoleAsync(existingUser, customerRole, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        return Result<int>.Success(count);
    }
}
