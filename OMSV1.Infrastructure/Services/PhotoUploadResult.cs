using System;

namespace OMSV1.Application.Dtos.Attachments;

public class PhotoUploadResult
{
    public required string FilePath { get; set; } // The relative or absolute URL of the uploaded file
}
