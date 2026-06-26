namespace WebAPI.Models;

/// <summary>
/// DTO for file upload via multipart/form-data.
/// </summary>
public class UploadDisbursementEvidenceForm
{
    /// <summary>
    /// The file to upload.
    /// </summary>
    public IFormFile? File { get; set; }
}
