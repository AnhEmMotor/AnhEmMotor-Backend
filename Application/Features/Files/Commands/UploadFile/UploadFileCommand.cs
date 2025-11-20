using Application.ApiContracts.File;
using Domain.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Files.Commands.UploadFile;

public sealed record UploadFileCommand(IFormFile File, string BaseUrl) : IRequest<(FileResponse? Data, ErrorResponse? Error)>;
