using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;

namespace DigitalSchoolManager
{
    internal static class PublicDashboardService
    {
        private static string ConnectionString { get { return ConfigurationManager.ConnectionStrings["SchoolDB"].ConnectionString; } }

        internal static void EnsureSchema()
        {
            string path = HttpContext.Current.Server.MapPath("~/PublicDashboard_Database_Update.sql");
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(File.ReadAllText(path), connection))
            { command.CommandTimeout = 120; connection.Open(); command.ExecuteNonQuery(); }
        }

        internal static DataRow GetProfile()
        {
            DataTable table = Fill(@"SELECT p.ProfileID,p.PrincipalTeacherID,p.PrincipalDisplayName,p.PrincipalDesignation,p.PrincipalMessage,p.SchoolIntroduction,p.SchoolHistory,
COALESCE(NULLIF(LTRIM(RTRIM(p.PrincipalDisplayName)),N''),NULLIF(LTRIM(RTRIM(t.Name)),N''),N'School Head / Principal') AS PrincipalName,
COALESCE(NULLIF(LTRIM(RTRIM(p.PrincipalDesignation)),N''),NULLIF(LTRIM(RTRIM(v.Description)),N''),N'Principal') AS EffectiveDesignation
FROM dbo.PublicSchoolProfile p LEFT JOIN dbo.Teachers t ON t.teacherid=p.PrincipalTeacherID
LEFT JOIN dbo.TeachingVacancyPosition v ON v.PostID=t.postID WHERE p.ProfileID=1;");
            if (table.Rows.Count == 0) throw new InvalidOperationException("The public school profile is not initialized.");
            return table.Rows[0];
        }

        internal static void SaveProfile(int? teacherId, string displayName, string designation, string message, string introduction, string history, int? userId)
        {
            introduction = Required(introduction, "school introduction", 2000);
            history = Required(history, "school history", 3000);
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(@"UPDATE dbo.PublicSchoolProfile SET PrincipalTeacherID=@TeacherID,PrincipalDisplayName=@DisplayName,
PrincipalDesignation=@Designation,PrincipalMessage=@Message,SchoolIntroduction=@Introduction,SchoolHistory=@History,
UpdatedByUserID=@UserID,UpdatedAtUtc=SYSUTCDATETIME() WHERE ProfileID=1;", connection))
            {
                command.Parameters.Add("@TeacherID", SqlDbType.Int).Value = teacherId.HasValue ? (object)teacherId.Value : DBNull.Value;
                NullableText(command, "@DisplayName", 150, displayName); NullableText(command, "@Designation", 120, designation);
                NullableText(command, "@Message", 1200, message); command.Parameters.Add("@Introduction", SqlDbType.NVarChar, 2000).Value = introduction;
                command.Parameters.Add("@History", SqlDbType.NVarChar, 3000).Value = history;
                command.Parameters.Add("@UserID", SqlDbType.Int).Value = userId.HasValue ? (object)userId.Value : DBNull.Value;
                connection.Open(); command.ExecuteNonQuery();
            }
        }

        internal static DataTable GetTeacherOptions()
        {
            return Fill(@"SELECT t.teacherid,t.Name + COALESCE(N' - ' + NULLIF(v.Description,N''),N'') AS DisplayName
FROM dbo.Teachers t LEFT JOIN dbo.TeachingVacancyPosition v ON v.PostID=t.postID
WHERE ISNULL(t.IsActive,1)=1 ORDER BY t.Name;");
        }

        internal static DataRow GetPublicStats()
        {
            return Fill(@"SELECT
(SELECT COUNT(*) FROM dbo.Students WHERE LTRIM(RTRIM(ISNULL(Isactive,N'Active'))) IN (N'Active',N'True',N'1')) AS TotalStudents,
(SELECT COUNT(*) FROM dbo.Teachers WHERE ISNULL(IsActive,1)=1) AS TotalTeachers,
(SELECT COUNT(*) FROM dbo.Classes) AS TotalClasses;").Rows[0];
        }

        internal static DataTable GetUpcomingExams()
        {
            return Fill(@"SELECT TOP(6) ExamID,ExamName,StartDate,EndDate FROM dbo.Exams
WHERE EndDate>=CAST(GETDATE() AS date) ORDER BY StartDate,ExamID;");
        }

        internal static DataTable GetPublishedAnnouncements()
        {
            return Fill(@"SELECT TOP(12) AnnouncementID,Title,AnnouncementCategory,Summary,AnnouncementBody,EventDate,PublishedAtUtc,IsPinned,
MediaType,MediaProvider,MediaUrl,MediaEmbedUrl,MediaRenderMode,MediaTitle,
CASE WHEN CoverImageData IS NULL THEN 0 ELSE 1 END AS HasImage
FROM dbo.SchoolAnnouncements WHERE IsPublished=1
ORDER BY IsPinned DESC,COALESCE(EventDate,CONVERT(date,PublishedAtUtc)) DESC,AnnouncementID DESC;");
        }

        internal static DataTable SearchAnnouncements(string search)
        {
            return Fill(@"SELECT AnnouncementID,Title,AnnouncementCategory,Summary,EventDate,PublishedAtUtc,IsPublished,IsPinned,
MediaType,MediaProvider,MediaTitle,
CASE WHEN CoverImageData IS NULL THEN 0 ELSE 1 END AS HasImage
FROM dbo.SchoolAnnouncements WHERE @Search=N'' OR Title LIKE @Pattern OR AnnouncementCategory LIKE @Pattern OR Summary LIKE @Pattern OR ISNULL(MediaTitle,N'') LIKE @Pattern OR ISNULL(MediaProvider,N'') LIKE @Pattern
ORDER BY UpdatedAtUtc DESC,AnnouncementID DESC;", Text("@Search", 180, search), Text("@Pattern", 200, "%" + (search ?? string.Empty).Trim() + "%"));
        }

        internal static DataRow GetAnnouncement(int announcementId)
        {
            DataTable table = Fill("SELECT * FROM dbo.SchoolAnnouncements WHERE AnnouncementID=@ID;", Int("@ID", announcementId));
            if (table.Rows.Count == 0) throw new InvalidOperationException("The selected announcement was not found.");
            return table.Rows[0];
        }

        internal static int SaveAnnouncement(SchoolAnnouncementInput input, ProcessedImage image)
        {
            input.Title = Required(input.Title, "announcement title", 220);
            input.AnnouncementCategory = Required(input.AnnouncementCategory, "announcement category", 60);
            input.Summary = Required(input.Summary, "short summary", 600);
            input.AnnouncementBody = Required(input.AnnouncementBody, "announcement details", 4000);
            PublicMediaReference media = ResolveMedia(input.MediaType, input.MediaUrl, input.MediaTitle);
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                if (input.AnnouncementID > 0)
                {
                    using (SqlCommand command = new SqlCommand(@"UPDATE dbo.SchoolAnnouncements SET Title=@Title,AnnouncementCategory=@Category,Summary=@Summary,
AnnouncementBody=@Body,EventDate=@EventDate,IsPublished=@Published,IsPinned=@Pinned,
MediaType=@MediaType,MediaProvider=@MediaProvider,MediaUrl=@MediaUrl,MediaEmbedUrl=@MediaEmbedUrl,MediaRenderMode=@MediaRenderMode,MediaTitle=@MediaTitle,
PublishedAtUtc=CASE WHEN @Published=1 AND PublishedAtUtc IS NULL THEN SYSUTCDATETIME() WHEN @Published=0 THEN NULL ELSE PublishedAtUtc END,
CoverImageData=CASE WHEN @HasImage=1 THEN @ImageData ELSE CoverImageData END,
CoverImageContentType=CASE WHEN @HasImage=1 THEN @ImageType ELSE CoverImageContentType END,
CoverImageFileName=CASE WHEN @HasImage=1 THEN @ImageName ELSE CoverImageFileName END,UpdatedAtUtc=SYSUTCDATETIME()
WHERE AnnouncementID=@ID;", connection))
                    { AddAnnouncementParameters(command, input, image, media); command.Parameters.Add("@ID", SqlDbType.Int).Value = input.AnnouncementID; if (command.ExecuteNonQuery() == 0) throw new InvalidOperationException("The selected announcement was not found."); return input.AnnouncementID; }
                }
                using (SqlCommand command = new SqlCommand(@"INSERT dbo.SchoolAnnouncements(Title,AnnouncementCategory,Summary,AnnouncementBody,EventDate,IsPublished,IsPinned,PublishedAtUtc,
CoverImageData,CoverImageContentType,CoverImageFileName,MediaType,MediaProvider,MediaUrl,MediaEmbedUrl,MediaRenderMode,MediaTitle,CreatedByUserID)
VALUES(@Title,@Category,@Summary,@Body,@EventDate,@Published,@Pinned,CASE WHEN @Published=1 THEN SYSUTCDATETIME() ELSE NULL END,
@ImageData,@ImageType,@ImageName,@MediaType,@MediaProvider,@MediaUrl,@MediaEmbedUrl,@MediaRenderMode,@MediaTitle,@UserID); SELECT CAST(SCOPE_IDENTITY() AS int);", connection))
                { AddAnnouncementParameters(command, input, image, media); input.AnnouncementID = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture); return input.AnnouncementID; }
            }
        }

        internal static string BuildPublicMediaMarkup(string mediaType, string mediaUrl, string mediaTitle)
        {
            PublicMediaReference media;
            try { media = ResolveMedia(mediaType, mediaUrl, mediaTitle); }
            catch { return string.Empty; }
            if (media == null) return string.Empty;

            string source = HttpUtility.HtmlAttributeEncode(media.EmbedUrl);
            string title = HttpUtility.HtmlAttributeEncode(media.Title);
            string provider = HttpUtility.HtmlEncode(media.Provider);
            if (media.RenderMode == "Html5Audio")
                return "<div class=\"public-announcement-media public-announcement-audio\"><div class=\"public-media-caption\"><span>Audio</span><strong>" + title + "</strong><small>" + provider + "</small></div><audio controls preload=\"metadata\"><source src=\"" + source + "\" />Your browser cannot play this audio.</audio></div>";
            if (media.RenderMode == "Html5Video")
                return "<div class=\"public-announcement-media public-announcement-video\"><video controls preload=\"metadata\" playsinline><source src=\"" + source + "\" />Your browser cannot play this video.</video><div class=\"public-media-caption compact\"><strong>" + title + "</strong><small>" + provider + "</small></div></div>";

            string mediaClass = media.RenderMode == "IframeAudio" ? " public-announcement-audio-frame" : string.Empty;
            string allow = media.RenderMode == "IframeVideo" ? " allow=\"accelerometer; autoplay; encrypted-media; picture-in-picture\" allowfullscreen" : " allow=\"autoplay\"";
            return "<div class=\"public-announcement-media" + mediaClass + "\"><iframe src=\"" + source + "\" title=\"" + title + "\" loading=\"lazy\" referrerpolicy=\"strict-origin-when-cross-origin\"" + allow + "></iframe><div class=\"public-media-caption compact\"><strong>" + title + "</strong><small>" + provider + "</small></div></div>";
        }

        internal static void SetAnnouncementPublished(int announcementId, bool published)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(@"UPDATE dbo.SchoolAnnouncements SET IsPublished=@Published,
PublishedAtUtc=CASE WHEN @Published=1 THEN COALESCE(PublishedAtUtc,SYSUTCDATETIME()) ELSE NULL END,UpdatedAtUtc=SYSUTCDATETIME() WHERE AnnouncementID=@ID;", connection))
            { command.Parameters.Add("@Published", SqlDbType.Bit).Value = published; command.Parameters.Add("@ID", SqlDbType.Int).Value = announcementId; connection.Open(); if (command.ExecuteNonQuery() == 0) throw new InvalidOperationException("The selected announcement was not found."); }
        }

        internal static PublicImageData GetPrincipalImage()
        {
            DataTable table = Fill(@"SELECT TOP(1) t.image AS ImageData FROM dbo.PublicSchoolProfile p
INNER JOIN dbo.Teachers t ON t.teacherid=p.PrincipalTeacherID WHERE p.ProfileID=1 AND t.image IS NOT NULL;");
            return ReadImageRow(table, "ImageData", null);
        }

        internal static PublicImageData GetAnnouncementImage(int announcementId)
        {
            DataTable table = Fill("SELECT CoverImageData,CoverImageContentType FROM dbo.SchoolAnnouncements WHERE AnnouncementID=@ID AND IsPublished=1;", Int("@ID", announcementId));
            return ReadImageRow(table, "CoverImageData", "CoverImageContentType");
        }

        private static PublicImageData ReadImageRow(DataTable table, string dataColumn, string typeColumn)
        {
            if (table.Rows.Count == 0 || table.Rows[0][dataColumn] == DBNull.Value) return null;
            byte[] data = (byte[])table.Rows[0][dataColumn];
            string contentType = typeColumn == null || table.Rows[0][typeColumn] == DBNull.Value ? DetectImageType(data) : Convert.ToString(table.Rows[0][typeColumn], CultureInfo.InvariantCulture);
            return new PublicImageData { Data = data, ContentType = string.IsNullOrWhiteSpace(contentType) ? "image/jpeg" : contentType };
        }

        private static void AddAnnouncementParameters(SqlCommand command, SchoolAnnouncementInput input, ProcessedImage image, PublicMediaReference media)
        {
            command.Parameters.Add("@Title", SqlDbType.NVarChar, 220).Value = input.Title; command.Parameters.Add("@Category", SqlDbType.NVarChar, 60).Value = input.AnnouncementCategory;
            command.Parameters.Add("@Summary", SqlDbType.NVarChar, 600).Value = input.Summary; command.Parameters.Add("@Body", SqlDbType.NVarChar, 4000).Value = input.AnnouncementBody;
            command.Parameters.Add("@EventDate", SqlDbType.Date).Value = input.EventDate.HasValue ? (object)input.EventDate.Value.Date : DBNull.Value;
            command.Parameters.Add("@Published", SqlDbType.Bit).Value = input.IsPublished; command.Parameters.Add("@Pinned", SqlDbType.Bit).Value = input.IsPinned;
            command.Parameters.Add("@HasImage", SqlDbType.Bit).Value = image != null;
            command.Parameters.Add("@ImageData", SqlDbType.VarBinary, -1).Value = image == null ? (object)DBNull.Value : image.Data;
            command.Parameters.Add("@ImageType", SqlDbType.NVarChar, 100).Value = image == null ? (object)DBNull.Value : image.ContentType;
            command.Parameters.Add("@ImageName", SqlDbType.NVarChar, 260).Value = image == null ? (object)DBNull.Value : image.FileName;
            command.Parameters.Add("@MediaType", SqlDbType.NVarChar, 20).Value = media == null ? (object)DBNull.Value : media.MediaType;
            command.Parameters.Add("@MediaProvider", SqlDbType.NVarChar, 30).Value = media == null ? (object)DBNull.Value : media.Provider;
            command.Parameters.Add("@MediaUrl", SqlDbType.NVarChar, 1000).Value = media == null ? (object)DBNull.Value : media.OriginalUrl;
            command.Parameters.Add("@MediaEmbedUrl", SqlDbType.NVarChar, 1200).Value = media == null ? (object)DBNull.Value : media.EmbedUrl;
            command.Parameters.Add("@MediaRenderMode", SqlDbType.NVarChar, 30).Value = media == null ? (object)DBNull.Value : media.RenderMode;
            command.Parameters.Add("@MediaTitle", SqlDbType.NVarChar, 200).Value = media == null ? (object)DBNull.Value : media.Title;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = input.UserID.HasValue ? (object)input.UserID.Value : DBNull.Value;
        }

        private static PublicMediaReference ResolveMedia(string mediaType, string mediaUrl, string mediaTitle)
        {
            mediaType = (mediaType ?? string.Empty).Trim();
            mediaUrl = (mediaUrl ?? string.Empty).Trim();
            mediaTitle = (mediaTitle ?? string.Empty).Trim();
            if (mediaType.Length == 0 || mediaType.Equals("None", StringComparison.OrdinalIgnoreCase)) return null;
            if (!mediaType.Equals("Video", StringComparison.OrdinalIgnoreCase) && !mediaType.Equals("Audio", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Select Video, Audio, or No media.");
            if (mediaUrl.Length == 0) throw new InvalidOperationException("Enter the public media link.");
            if (mediaUrl.Length > 1000) throw new InvalidOperationException("The media link is too long.");
            if (mediaTitle.Length > 200) throw new InvalidOperationException("The media title is too long.");

            Uri uri;
            if (!Uri.TryCreate(mediaUrl, UriKind.Absolute, out uri) || !uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Enter a complete HTTPS media link.");

            bool video = mediaType.Equals("Video", StringComparison.OrdinalIgnoreCase);
            PublicMediaReference media = new PublicMediaReference { MediaType = video ? "Video" : "Audio", OriginalUrl = uri.AbsoluteUri, Title = mediaTitle };
            if (HostIs(uri, "youtube.com") || HostIs(uri, "youtube-nocookie.com") || HostIs(uri, "youtu.be"))
            {
                if (!video) throw new InvalidOperationException("YouTube links must use the Video media type.");
                string id = YouTubeId(uri);
                if (!Regex.IsMatch(id ?? string.Empty, "^[A-Za-z0-9_-]{6,20}$")) throw new InvalidOperationException("The YouTube video link is not valid.");
                media.Provider = "YouTube"; media.RenderMode = "IframeVideo"; media.EmbedUrl = "https://www.youtube-nocookie.com/embed/" + id;
            }
            else if (HostIs(uri, "facebook.com") || HostIs(uri, "fb.watch"))
            {
                if (!video) throw new InvalidOperationException("Facebook media links must use the Video media type.");
                media.Provider = "Facebook"; media.RenderMode = "IframeVideo"; media.EmbedUrl = "https://www.facebook.com/plugins/video.php?href=" + HttpUtility.UrlEncode(uri.AbsoluteUri) + "&show_text=false&width=900";
            }
            else if (HostIs(uri, "vimeo.com"))
            {
                if (!video) throw new InvalidOperationException("Vimeo links must use the Video media type.");
                Match match = Regex.Match(uri.AbsolutePath, @"/(?:video/)?([0-9]{5,15})(?:/|$)");
                if (!match.Success) throw new InvalidOperationException("The Vimeo video link is not valid.");
                media.Provider = "Vimeo"; media.RenderMode = "IframeVideo"; media.EmbedUrl = "https://player.vimeo.com/video/" + match.Groups[1].Value;
            }
            else if (HostIs(uri, "soundcloud.com"))
            {
                if (video) throw new InvalidOperationException("SoundCloud links must use the Audio media type.");
                media.Provider = "SoundCloud"; media.RenderMode = "IframeAudio"; media.EmbedUrl = "https://w.soundcloud.com/player/?url=" + HttpUtility.UrlEncode(uri.AbsoluteUri) + "&color=%230b6b3a&auto_play=false&hide_related=true&show_comments=false";
            }
            else if (HostIs(uri, "spotify.com"))
            {
                if (video) throw new InvalidOperationException("Spotify links must use the Audio media type.");
                string spotifyPath = uri.AbsolutePath.Trim('/');
                if (spotifyPath.StartsWith("embed/", StringComparison.OrdinalIgnoreCase)) spotifyPath = spotifyPath.Substring(6);
                if (!Regex.IsMatch(spotifyPath, @"^(track|episode|show|playlist|album)/[A-Za-z0-9]+$", RegexOptions.IgnoreCase)) throw new InvalidOperationException("The Spotify media link is not valid.");
                media.Provider = "Spotify"; media.RenderMode = "IframeAudio"; media.EmbedUrl = "https://open.spotify.com/embed/" + spotifyPath;
            }
            else
            {
                string extension = Path.GetExtension(uri.AbsolutePath).ToLowerInvariant();
                bool audioFile = extension == ".mp3" || extension == ".wav" || extension == ".ogg" || extension == ".m4a";
                bool videoFile = extension == ".mp4" || extension == ".webm" || extension == ".ogv";
                if ((!video && !audioFile) || (video && !videoFile))
                    throw new InvalidOperationException("This provider is not supported. Use YouTube, Facebook, Vimeo, SoundCloud, Spotify, or a direct HTTPS media-file link.");
                media.Provider = "Direct media"; media.RenderMode = video ? "Html5Video" : "Html5Audio"; media.EmbedUrl = uri.AbsoluteUri;
            }
            if (media.Title.Length == 0) media.Title = media.MediaType + " from " + media.Provider;
            return media;
        }

        private static bool HostIs(Uri uri, string root)
        {
            string host = uri.DnsSafeHost.TrimEnd('.');
            return host.Equals(root, StringComparison.OrdinalIgnoreCase) || host.EndsWith("." + root, StringComparison.OrdinalIgnoreCase);
        }

        private static string YouTubeId(Uri uri)
        {
            if (HostIs(uri, "youtu.be")) return uri.AbsolutePath.Trim('/').Split('/')[0];
            string path = uri.AbsolutePath.Trim('/');
            string[] parts = path.Split('/');
            if (parts.Length >= 2 && (parts[0].Equals("embed", StringComparison.OrdinalIgnoreCase) || parts[0].Equals("shorts", StringComparison.OrdinalIgnoreCase) || parts[0].Equals("live", StringComparison.OrdinalIgnoreCase))) return parts[1];
            return HttpUtility.ParseQueryString(uri.Query)["v"];
        }

        private static string DetectImageType(byte[] data)
        {
            if (data != null && data.Length >= 8 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47) return "image/png";
            if (data != null && data.Length >= 6 && data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46) return "image/gif";
            return "image/jpeg";
        }

        private static DataTable Fill(string sql, params SqlParameter[] parameters)
        {
            DataTable table = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            { if (parameters != null) command.Parameters.AddRange(parameters); adapter.Fill(table); }
            return table;
        }
        private static SqlParameter Text(string name, int size, string value) { return new SqlParameter(name, SqlDbType.NVarChar, size) { Value = (value ?? string.Empty).Trim() }; }
        private static SqlParameter Int(string name, int value) { return new SqlParameter(name, SqlDbType.Int) { Value = value }; }
        private static string Required(string value, string label, int max) { value = (value ?? string.Empty).Trim(); if (value.Length == 0) throw new InvalidOperationException("Enter the " + label + "."); if (value.Length > max) throw new InvalidOperationException("The " + label + " is too long."); return value; }
        private static void NullableText(SqlCommand command, string name, int size, string value) { value = (value ?? string.Empty).Trim(); if (value.Length > size) value = value.Substring(0, size); command.Parameters.Add(name, SqlDbType.NVarChar, size).Value = value.Length == 0 ? (object)DBNull.Value : value; }
    }

    internal sealed class SchoolAnnouncementInput
    {
        internal int AnnouncementID { get; set; } internal string Title { get; set; } internal string AnnouncementCategory { get; set; }
        internal string Summary { get; set; } internal string AnnouncementBody { get; set; } internal DateTime? EventDate { get; set; }
        internal bool IsPublished { get; set; } internal bool IsPinned { get; set; } internal int? UserID { get; set; }
        internal string MediaType { get; set; } internal string MediaUrl { get; set; } internal string MediaTitle { get; set; }
    }
    internal sealed class PublicImageData { internal byte[] Data { get; set; } internal string ContentType { get; set; } }
    internal sealed class PublicMediaReference
    {
        internal string MediaType { get; set; } internal string Provider { get; set; } internal string OriginalUrl { get; set; }
        internal string EmbedUrl { get; set; } internal string RenderMode { get; set; } internal string Title { get; set; }
    }
}
