using ImageMagick;

namespace BackEnd.Services;

public sealed class HeicImageConverter
{
    private static readonly HashSet<string> HeicExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".heic",
        ".heif"
    };

    private static readonly HashSet<string> HeicContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/heic",
        "image/heif",
        "image/heic-sequence",
        "image/heif-sequence"
    };

    public bool IsHeicImage(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        return HeicExtensions.Contains(extension) || HeicContentTypes.Contains(file.ContentType);
    }

    public async Task<(Stream Stream, string FileName, string ContentType)> PrepareForUploadAsync(IFormFile file)
    {
        if (!IsHeicImage(file))
        {
            return (file.OpenReadStream(), Path.GetFileName(file.FileName), string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
        }

        await using var inputStream = file.OpenReadStream();
        using var image = new MagickImage(inputStream);
        image.Format = MagickFormat.Jpeg;
        image.Quality = 90;

        var outputStream = new MemoryStream();
        await image.WriteAsync(outputStream, MagickFormat.Jpeg);
        outputStream.Position = 0;

        var baseName = Path.GetFileNameWithoutExtension(file.FileName);
        var fileName = string.IsNullOrWhiteSpace(baseName) ? $"{Guid.NewGuid():N}.jpg" : $"{baseName}.jpg";

        return (outputStream, fileName, "image/jpeg");
    }
}