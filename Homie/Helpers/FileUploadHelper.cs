using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Homie.Helpers
{
    /// <summary>
    /// SECURITY: Централизованная валидация загружаемых файлов для защиты от вредоносных загрузок
    /// </summary>
    public static class FileUploadHelper
    {
        // Максимальный размер файла в байтах (5 MB по умолчанию)
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        // Разрешенные MIME-типы для изображений
        private static readonly HashSet<string> AllowedImageMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/gif",
            "image/bmp",
            "image/webp"
        };

        // Разрешенные расширения файлов
        private static readonly HashSet<string> AllowedImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".bmp",
            ".webp"
        };

        /// <summary>
        /// Валидация загружаемого изображения
        /// </summary>
        /// <param name="file">Загружаемый файл</param>
        /// <param name="errorMessage">Сообщение об ошибке (если валидация не прошла)</param>
        /// <returns>True если файл валиден, иначе False</returns>
        public static bool ValidateImageFile(IFormFile file, out string errorMessage)
        {
            errorMessage = null;

            if (file == null || file.Length == 0)
            {
                errorMessage = "Файл не выбран или пуст";
                return false;
            }

            // 1. Проверка размера файла
            if (file.Length > MaxFileSizeBytes)
            {
                errorMessage = $"Размер файла превышает максимально допустимый ({MaxFileSizeBytes / (1024 * 1024)} МБ)";
                return false;
            }

            // 2. Проверка расширения файла
            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extension) || !AllowedImageExtensions.Contains(extension))
            {
                errorMessage = $"Недопустимое расширение файла. Разрешены: {string.Join(", ", AllowedImageExtensions)}";
                return false;
            }

            // 3. Проверка MIME-типа
            if (!AllowedImageMimeTypes.Contains(file.ContentType))
            {
                errorMessage = $"Недопустимый тип файла. Разрешены только изображения";
                return false;
            }

            // 4. Проверка на опасные символы в имени файла (защита от path traversal)
            if (file.FileName.Contains("..") || file.FileName.Contains("/") || file.FileName.Contains("\\"))
            {
                errorMessage = "Недопустимые символы в имени файла";
                return false;
            }

            // 5. SECURITY: Проверка магических байтов (сигнатуры файла) для предотвращения загрузки вредоносных файлов
            if (!ValidateFileSignature(file))
            {
                errorMessage = "Файл не является изображением (проверка сигнатуры не прошла)";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Генерация безопасного имени файла (защита от path traversal и других атак)
        /// </summary>
        /// <param name="originalFileName">Оригинальное имя файла</param>
        /// <returns>Безопасное имя файла</returns>
        public static string GenerateSafeFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var safeFileName = $"{Guid.NewGuid()}{extension}";
            return safeFileName;
        }

        /// <summary>
        /// Проверка сигнатуры файла (магические байты) для подтверждения типа файла
        /// </summary>
        private static bool ValidateFileSignature(IFormFile file)
        {
            // Словарь сигнатур файлов (первые байты)
            var imageSignatures = new Dictionary<string, List<byte[]>>
            {
                {
                    ".jpg", new List<byte[]>
                    {
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, // JPEG
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 }, // JPEG EXIF
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }  // JPEG SPIFF
                    }
                },
                {
                    ".jpeg", new List<byte[]>
                    {
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }
                    }
                },
                {
                    ".png", new List<byte[]>
                    {
                        new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
                    }
                },
                {
                    ".gif", new List<byte[]>
                    {
                        new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, // GIF87a
                        new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }  // GIF89a
                    }
                },
                {
                    ".bmp", new List<byte[]>
                    {
                        new byte[] { 0x42, 0x4D }
                    }
                },
                {
                    ".webp", new List<byte[]>
                    {
                        new byte[] { 0x52, 0x49, 0x46, 0x46 } // RIFF (WebP начинается с RIFF)
                    }
                }
            };

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!imageSignatures.ContainsKey(extension))
            {
                return false;
            }

            using (var reader = new BinaryReader(file.OpenReadStream()))
            {
                var signatures = imageSignatures[extension];
                var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                foreach (var signature in signatures)
                {
                    if (headerBytes.Take(signature.Length).SequenceEqual(signature))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Валидация обычного файла (не изображения)
        /// </summary>
        public static bool ValidateFile(IFormFile file, out string errorMessage, long maxSizeBytes = MaxFileSizeBytes)
        {
            errorMessage = null;

            if (file == null || file.Length == 0)
            {
                errorMessage = "Файл не выбран или пуст";
                return false;
            }

            if (file.Length > maxSizeBytes)
            {
                errorMessage = $"Размер файла превышает максимально допустимый ({maxSizeBytes / (1024 * 1024)} МБ)";
                return false;
            }

            // Проверка на опасные символы в имени файла
            if (file.FileName.Contains("..") || file.FileName.Contains("/") || file.FileName.Contains("\\"))
            {
                errorMessage = "Недопустимые символы в имени файла";
                return false;
            }

            // Блокировка опасных расширений
            var dangerousExtensions = new[] { ".exe", ".dll", ".bat", ".cmd", ".sh", ".ps1", ".vbs", ".js", ".jar", ".com", ".pif", ".msi" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (dangerousExtensions.Contains(extension))
            {
                errorMessage = "Загрузка исполняемых файлов запрещена";
                return false;
            }

            return true;
        }
    }
}
