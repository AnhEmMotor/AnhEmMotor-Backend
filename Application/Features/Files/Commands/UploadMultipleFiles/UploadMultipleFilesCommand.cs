using Application.ApiContracts.File;
using Domain.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Files.Commands.UploadMultipleFiles;

public sealed record UploadMultipleFilesCommand(List<IFormFile> Files, string BaseUrl) : IRequest<(List<FileResponse>? Data, ErrorResponse? Error)>;
