namespace Application.Features.Files.Commands.UploadManyImage
{
    public sealed record FileUploadDto(Stream FileContent, string FileName);
}
