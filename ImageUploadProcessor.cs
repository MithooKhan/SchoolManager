using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using FileUpload = System.Web.UI.WebControls.FileUpload;

namespace DigitalSchoolManager
{
    /// <summary>
    /// Validates uploaded photographs and scanned images, corrects camera
    /// orientation, and produces a database-ready JPEG no larger than 1 MB.
    /// </summary>
    internal static class ImageUploadProcessor
    {
        internal const int MaximumStoredImageBytes = 1024 * 1024;
        internal const int MaximumSourceImageBytes = 25 * 1024 * 1024;
        private const long MaximumSourcePixels = 80000000L;

        internal static ProcessedImage ReadAndOptimize(
            FileUpload upload,
            int maximumWidth,
            int maximumHeight)
        {
            if (upload == null || !upload.HasFile)
                return null;

            return ReadAndOptimize(upload.PostedFile, maximumWidth, maximumHeight);
        }

        internal static ProcessedImage ReadAndOptimize(
            HttpPostedFile upload,
            int maximumWidth,
            int maximumHeight)
        {
            if (upload == null || upload.ContentLength <= 0)
                return null;

            int contentLength = upload.ContentLength;
            if (contentLength <= 0)
                throw new InvalidOperationException("The selected image is empty.");
            if (contentLength > MaximumSourceImageBytes)
                throw new InvalidOperationException("The source image must be no larger than 25 MB.");

            string originalFileName = Path.GetFileName(upload.FileName ?? string.Empty);
            string extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            if (!IsCompressibleImageExtension(extension))
                throw new InvalidOperationException("Upload a JPG, JPEG, PNG, GIF, or BMP image only.");

            byte[] sourceData;
            if (upload.InputStream.CanSeek)
                upload.InputStream.Position = 0;

            using (BinaryReader reader = new BinaryReader(upload.InputStream))
            {
                sourceData = reader.ReadBytes(contentLength);
            }

            if (sourceData.Length != contentLength)
                throw new InvalidOperationException("The complete image could not be read.");

            return OptimizeBytes(sourceData, originalFileName, maximumWidth, maximumHeight);
        }

        internal static ProcessedImage OptimizeBytes(
            byte[] sourceData,
            string originalFileName,
            int maximumWidth,
            int maximumHeight)
        {
            if (sourceData == null || sourceData.Length == 0)
                throw new InvalidOperationException("The image data is empty.");
            if (sourceData.Length > MaximumSourceImageBytes)
                throw new InvalidOperationException("The source image must be no larger than 25 MB.");
            if (maximumWidth <= 0 || maximumHeight <= 0)
                throw new ArgumentOutOfRangeException("maximumWidth", "Image dimensions must be positive.");

            try
            {
                using (MemoryStream input = new MemoryStream(sourceData, false))
                using (Image source = Image.FromStream(input, true, true))
                {
                    if ((long)source.Width * source.Height > MaximumSourcePixels)
                        throw new InvalidOperationException("The image dimensions are too large to process safely.");

                    ApplyExifOrientation(source);

                    int width;
                    int height;
                    CalculateDimensions(source.Width, source.Height, maximumWidth, maximumHeight, out width, out height);

                    long[] qualities = { 88L, 80L, 72L, 64L, 56L, 48L, 40L, 32L, 25L };
                    for (int resizePass = 0; resizePass < 7; resizePass++)
                    {
                        using (Bitmap rendered = RenderJpegCanvas(source, width, height))
                        {
                            foreach (long quality in qualities)
                            {
                                byte[] encoded = EncodeJpeg(rendered, quality);
                                if (encoded.Length <= MaximumStoredImageBytes)
                                {
                                    return new ProcessedImage
                                    {
                                        Data = encoded,
                                        ContentType = "image/jpeg",
                                        FileName = BuildJpegFileName(originalFileName),
                                        OriginalByteCount = sourceData.LongLength,
                                        WasOptimized = sourceData.Length > MaximumStoredImageBytes ||
                                                       encoded.Length < sourceData.Length
                                    };
                                }
                            }
                        }

                        width = Math.Max(320, (int)Math.Round(width * .82));
                        height = Math.Max(320, (int)Math.Round(height * .82));
                    }
                }
            }
            catch (ArgumentException)
            {
                throw new InvalidOperationException("The selected file is not a valid supported image.");
            }
            catch (OutOfMemoryException)
            {
                throw new InvalidOperationException("The selected image is too large or is not a valid supported image.");
            }

            throw new InvalidOperationException("The image could not be reduced below the 1 MB database limit.");
        }

        internal static bool IsCompressibleImageExtension(string extension)
        {
            string value = (extension ?? string.Empty).ToLowerInvariant();
            return value == ".jpg" || value == ".jpeg" || value == ".png" ||
                   value == ".gif" || value == ".bmp";
        }

        private static void CalculateDimensions(
            int sourceWidth,
            int sourceHeight,
            int maximumWidth,
            int maximumHeight,
            out int width,
            out int height)
        {
            double scale = Math.Min(
                1D,
                Math.Min((double)maximumWidth / sourceWidth, (double)maximumHeight / sourceHeight));
            width = Math.Max(1, (int)Math.Round(sourceWidth * scale));
            height = Math.Max(1, (int)Math.Round(sourceHeight * scale));
        }

        private static Bitmap RenderJpegCanvas(Image source, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            bitmap.SetResolution(96F, 96F);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(source, new Rectangle(0, 0, width, height));
            }
            return bitmap;
        }

        private static byte[] EncodeJpeg(Bitmap bitmap, long quality)
        {
            ImageCodecInfo codec = null;
            foreach (ImageCodecInfo candidate in ImageCodecInfo.GetImageEncoders())
            {
                if (candidate.FormatID == ImageFormat.Jpeg.Guid)
                {
                    codec = candidate;
                    break;
                }
            }
            if (codec == null)
                throw new InvalidOperationException("The server JPEG encoder is unavailable.");

            using (MemoryStream output = new MemoryStream())
            using (EncoderParameters parameters = new EncoderParameters(1))
            {
                parameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                bitmap.Save(output, codec, parameters);
                return output.ToArray();
            }
        }

        private static void ApplyExifOrientation(Image image)
        {
            const int orientationPropertyId = 0x0112;
            if (Array.IndexOf(image.PropertyIdList, orientationPropertyId) < 0)
                return;

            try
            {
                int orientation = image.GetPropertyItem(orientationPropertyId).Value[0];
                switch (orientation)
                {
                    case 2: image.RotateFlip(RotateFlipType.RotateNoneFlipX); break;
                    case 3: image.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
                    case 4: image.RotateFlip(RotateFlipType.Rotate180FlipX); break;
                    case 5: image.RotateFlip(RotateFlipType.Rotate90FlipX); break;
                    case 6: image.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
                    case 7: image.RotateFlip(RotateFlipType.Rotate270FlipX); break;
                    case 8: image.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
                }
                image.RemovePropertyItem(orientationPropertyId);
            }
            catch
            {
                // Missing or malformed camera metadata must not block a valid image.
            }
        }

        private static string BuildJpegFileName(string originalFileName)
        {
            string stem = Path.GetFileNameWithoutExtension(originalFileName ?? string.Empty);
            if (string.IsNullOrWhiteSpace(stem))
                stem = "uploaded-image";
            if (stem.Length > 220)
                stem = stem.Substring(0, 220);
            return stem + ".jpg";
        }
    }

    internal sealed class ProcessedImage
    {
        internal byte[] Data { get; set; }
        internal string ContentType { get; set; }
        internal string FileName { get; set; }
        internal long OriginalByteCount { get; set; }
        internal bool WasOptimized { get; set; }
    }
}
