using Application.Common.Models;
using MediatR;

namespace Application.Features.Contacts.Commands.UploadCv
{
    public class UploadCvCommand : IRequest<Result<string>>
    {
        public Stream FileContent { get; set; } = null!;

        public string FileName { get; set; } = string.Empty;
    }
}
