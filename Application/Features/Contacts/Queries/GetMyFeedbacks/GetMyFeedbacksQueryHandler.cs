using Application.ApiContracts.Contacts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Contact;
using Domain.Entities;
using Domain.Primitives;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Contacts.Queries.GetMyFeedbacks;

public class GetMyFeedbacksQueryHandler(
    ICustomerFeedbackRepository feedbackRepository) : IRequestHandler<GetMyFeedbacksQuery, Result<List<CustomerFeedbackResponse>>>
{
    public async Task<Result<List<CustomerFeedbackResponse>>> Handle(
        GetMyFeedbacksQuery request,
        CancellationToken cancellationToken)
    {
        var allFeedbacks = await feedbackRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var myFeedbacks = allFeedbacks
            .Where(f => !string.IsNullOrEmpty(f.PhoneNumber) && f.PhoneNumber == request.PhoneNumber)
            .OrderByDescending(f => f.CreatedAt)
            .ToList();

        // Seed data if none exists
        if (!myFeedbacks.Any() && !string.IsNullOrEmpty(request.PhoneNumber))
        {
            var seed1 = new CustomerFeedback
            {
                CustomerName = request.CustomerName,
                PhoneNumber = request.PhoneNumber,
                FeedbackArea = "Service",
                Rating = 5,
                Content = "Chất lượng bảo dưỡng xe tại showroom rất tốt. Nhân viên nhiệt tình.",
                Status = "Resolved",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                Contact = new Contact
                {
                    FullName = request.CustomerName,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    Subject = "Phản hồi chất lượng dịch vụ",
                    Message = "Chất lượng bảo dưỡng xe tại showroom rất tốt. Nhân viên nhiệt tình.",
                    Status = "Resolved",
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    Replies = new List<ContactReply>
                    {
                        new ContactReply
                        {
                            Message = "Cảm ơn quý khách đã tin tưởng và sử dụng dịch vụ bảo dưỡng tại AnhEmMotor. Rất mong được tiếp tục phục vụ quý khách trong thời gian tới.",
                            IsInternal = false,
                            CreatedAt = DateTime.UtcNow.AddDays(-14)
                        }
                    }
                }
            };

            var seed2 = new CustomerFeedback
            {
                CustomerName = request.CustomerName,
                PhoneNumber = request.PhoneNumber,
                FeedbackArea = "Facility",
                Rating = 4,
                Content = "Khu vực ngồi chờ rửa xe hơi nóng, cần thêm quạt hoặc máy lạnh.",
                Status = "Resolved",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                Contact = new Contact
                {
                    FullName = request.CustomerName,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    Subject = "Góp ý khu vực chờ",
                    Message = "Khu vực ngồi chờ rửa xe hơi nóng, cần thêm quạt hoặc máy lạnh.",
                    Status = "Resolved",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    Replies = new List<ContactReply>
                    {
                        new ContactReply
                        {
                            Message = "Ban showroom AnhEmMotor Biên Hòa xin gửi lời cảm ơn và tiếp thu ý kiến đóng góp chân thành của bạn. Chúng tôi đã xử lý và lắp thêm hệ thống quạt mát tại khu vực chờ.",
                            IsInternal = false,
                            CreatedAt = DateTime.UtcNow.AddDays(-4)
                        }
                    }
                }
            };

            await feedbackRepository.AddAsync(seed1, cancellationToken).ConfigureAwait(false);
            await feedbackRepository.AddAsync(seed2, cancellationToken).ConfigureAwait(false);

            myFeedbacks.Add(seed1);
            myFeedbacks.Add(seed2);
            myFeedbacks = myFeedbacks.OrderByDescending(f => f.CreatedAt).ToList();
        }

        var responses = myFeedbacks.Select(f => new CustomerFeedbackResponse
        {
            Id = f.Id,
            ContactId = f.ContactId,
            Rating = f.Rating,
            FeedbackArea = f.FeedbackArea,
            CustomerName = f.CustomerName,
            PhoneNumber = f.PhoneNumber,
            Content = f.Content,
            Status = f.Status,
            CreatedAt = f.CreatedAt,
            Contact = f.Contact == null ? null : new ContactBasicResponse
            {
                Id = f.Contact.Id,
                FullName = f.Contact.FullName,
                Email = f.Contact.Email,
                PhoneNumber = f.Contact.PhoneNumber,
                InternalNote = f.Contact.InternalNote,
                CreatedAt = f.Contact.CreatedAt,
                Replies = f.Contact.Replies.Select(r => new ContactReplyResponse
                {
                    Id = r.Id,
                    ContactId = r.ContactId,
                    Message = r.Message,
                    RepliedById = r.RepliedById,
                    RepliedByName = r.RepliedBy?.FullName,
                    IsInternal = r.IsInternal,
                    CreatedAt = r.CreatedAt
                }).ToList()
            }
        }).ToList();

        return Result<List<CustomerFeedbackResponse>>.Success(responses);
    }
}
