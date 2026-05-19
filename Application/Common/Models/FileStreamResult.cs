namespace Application.Common.Models;

public record FileStreamResult(byte[] FileContents, string ContentType, string FileName);
