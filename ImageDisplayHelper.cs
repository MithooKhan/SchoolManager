using System;
using System.IO;
using System.Web;
using System.Web.UI;

namespace DigitalSchoolManager
{
    /// <summary>
    /// Converts image values stored either as database bytes or virtual file paths
    /// into browser-safe URLs and guarantees a valid fallback image.
    /// </summary>
    internal static class ImageDisplayHelper
    {
        private const string FallbackImage = "~/images/noimage.png";

        public static string GetImageUrl(Page page, object databaseValue, string preferredFolder)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            byte[] bytes = databaseValue as byte[];
            if (bytes != null && bytes.Length > 0)
            {
                return "data:" + GetMimeType(bytes) + ";base64," +
                       Convert.ToBase64String(bytes);
            }

            string storedPath = databaseValue == null || databaseValue == DBNull.Value
                ? string.Empty
                : Convert.ToString(databaseValue).Trim();

            if (string.IsNullOrWhiteSpace(storedPath))
                return page.ResolveUrl(FallbackImage);

            if (storedPath.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase) ||
                storedPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                storedPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                return storedPath;
            }

            string normalized = storedPath.Replace('\\', '/');
            string virtualPath;

            if (normalized.StartsWith("~/", StringComparison.Ordinal))
            {
                virtualPath = normalized;
            }
            else if (normalized.StartsWith("/", StringComparison.Ordinal))
            {
                virtualPath = "~" + normalized;
            }
            else if (normalized.IndexOf('/') >= 0 && normalized.IndexOf(':') < 0)
            {
                virtualPath = "~/" + normalized.TrimStart('/');
            }
            else
            {
                string safeFileName = Path.GetFileName(normalized);
                virtualPath = "~/" + preferredFolder.Trim('~', '/', '\\') + "/" + safeFileName;
            }

            try
            {
                string physicalPath = page.Server.MapPath(virtualPath);
                if (!File.Exists(physicalPath))
                    return page.ResolveUrl(FallbackImage);
            }
            catch (HttpException)
            {
                return page.ResolveUrl(FallbackImage);
            }

            return page.ResolveUrl(virtualPath);
        }

        private static string GetMimeType(byte[] bytes)
        {
            if (bytes.Length >= 8 &&
                bytes[0] == 0x89 && bytes[1] == 0x50 &&
                bytes[2] == 0x4E && bytes[3] == 0x47)
            {
                return "image/png";
            }

            if (bytes.Length >= 6 &&
                bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46)
            {
                return "image/gif";
            }

            if (bytes.Length >= 2 && bytes[0] == 0x42 && bytes[1] == 0x4D)
                return "image/bmp";

            return "image/jpeg";
        }
    }
}
