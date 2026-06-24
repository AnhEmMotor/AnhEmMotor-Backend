using Application.ApiContracts.Products.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Products.Commands.UploadProductContentImage;

public class UploadProductContentImageCommand : IRequest<UploadProductContentImageResponse>
{
    public IFormFile File { get; set; } = null!;
}
