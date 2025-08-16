namespace Homie.Models
{
    public class FileUploadSettings
    {
        public int MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024; // 5 MB по умолчанию
        public string[] AllowedImageTypes { get; set; } = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp" };
    }
}
