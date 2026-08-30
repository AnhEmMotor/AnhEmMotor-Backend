using Application.Common.Models;
using Application.Interfaces.Repositories.Lead.Lead;
using Application.Interfaces.Repositories.Role;
using Application.Interfaces.Repositories.User;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using System;
using System.Text.RegularExpressions;

namespace Application.Features.Leads.Commands.CreateLead
{
    public class CreateLeadCommandHandler(
        ILeadInsertRepository leadInsertRepository,
        ILeadReadRepository leadReadRepository,
        IUserReadRepository userReadRepository,
        IUserCreateRepository userCreateRepository,
        IRoleReadRepository roleReadRepository,
        IRoleInsertRepository roleInsertRepository) : IRequestHandler<CreateLeadCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.IdentificationNumber))
            {
                var existingLead = await leadReadRepository.GetByIdentificationNumberAsync(
                    request.IdentificationNumber,
                    cancellationToken)
                    .ConfigureAwait(false);
                if (existingLead != null)
                {
                    return Result<int>.Failure("Identification number already exists.");
                }
            }
            var lead = new Lead
            {
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                IdentificationNumber = request.IdentificationNumber,
                Birthday = request.Birthday,
                Gender = request.Gender,
                Source = request.Source,
                Status = request.Status,
                InterestedVehicle = request.InterestedVehicle,
                AddressDetail = request.AddressDetail,
                Ward = request.Ward,
                Province = request.Province,
                District = request.District,
                Score = request.Score,
                Notes = request.Notes,
                Priority = request.Priority,
                IsVerified = request.IsVerified
            };
            await leadInsertRepository.AddAsync(lead, cancellationToken).ConfigureAwait(false);
            string email = string.IsNullOrWhiteSpace(request.Email) ? string.Empty : request.Email.Trim();
            string phone = string.IsNullOrWhiteSpace(request.PhoneNumber) ? string.Empty : request.PhoneNumber.Trim();
            string username = string.IsNullOrEmpty(phone) ? email : phone;
            if (!string.IsNullOrEmpty(username))
            {
                username = Regex.Replace(username, @"[^a-zA-Z0-9_\-\.@]", string.Empty);
            }
            if (string.IsNullOrEmpty(username))
            {
                username = $"customer_{lead.Id}";
            }
            var existingUser = await userReadRepository.FindUserByUsernameAsync(username, cancellationToken)
                .ConfigureAwait(false);
            if (existingUser == null && !string.IsNullOrEmpty(email))
            {
                existingUser = await userReadRepository.FindUserByEmailAsync(email, cancellationToken)
                    .ConfigureAwait(false);
            }
            var customerRole = "Customer";
            if (!await roleReadRepository.IsRoleExistAsync(customerRole, cancellationToken).ConfigureAwait(false))
            {
                await roleInsertRepository.CreateAsync(new ApplicationRole { Name = customerRole }, cancellationToken)
                    .ConfigureAwait(false);
            }
            if (existingUser == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = username,
                    Email = string.IsNullOrEmpty(email) ? $"{username}@anhemmotor.com" : email,
                    FullName = request.FullName,
                    PhoneNumber = phone,
                    Status = UserStatus.Active,
                    Gender = request.Gender ?? GenderStatus.Other
                };
                var (succeeded, _) = await userCreateRepository.CreateUserAsync(
                    newUser,
                    "Khachhang@123",
                    cancellationToken)
                    .ConfigureAwait(false);
                if (succeeded)
                {
                    await userCreateRepository.AddUserToRoleAsync(newUser, customerRole, cancellationToken)
                        .ConfigureAwait(false);
                }
            } else
            {
                var roles = await userReadRepository.GetUserRolesAsync(existingUser, cancellationToken)
                    .ConfigureAwait(false);
                if (!roles.Contains(customerRole))
                {
                    await userCreateRepository.AddUserToRoleAsync(existingUser, customerRole, cancellationToken)
                        .ConfigureAwait(false);
                }
            }
            return lead.Id;
        }
    }
}
