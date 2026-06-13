using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Contact;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Contacts.Commands.CreateJobApplication;

public class CreateJobApplicationCommandHandler(
    IJobApplicationRepository jobApplicationRepository,
    IContactInsertRepository contactInsertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateJobApplicationCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var contact = new Contact
        {
            FullName = request.Request.FullName,
            Email = request.Request.Email,
            PhoneNumber = request.Request.PhoneNumber,
            Subject = $"Ứng tuyển: {request.Request.AppliedPosition}",
            Message = request.Request.CoverLetter ?? string.Empty,
            Status = "Pending"
        };
        contactInsertRepository.Add(contact);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var application = new JobApplication
        {
            ContactId = contact.Id,
            FullName = request.Request.FullName,
            Email = request.Request.Email,
            PhoneNumber = request.Request.PhoneNumber,
            AppliedPosition = request.Request.AppliedPosition,
            CvFileUrl = request.Request.CvFileUrl,
            CoverLetter = request.Request.CoverLetter,
            Status = AppStatus.New
        };
        await jobApplicationRepository.AddAsync(application, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Success(application.Id);
    }
}
