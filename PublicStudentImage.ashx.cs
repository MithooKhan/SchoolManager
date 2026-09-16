using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.SessionState;

namespace DigitalSchoolManager
{
    public sealed class PublicStudentImage : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            int id,allowed;
            if(!int.TryParse(context.Request.QueryString["id"],out id)||!int.TryParse(Convert.ToString(context.Session["StudentProfileAccessID"]),out allowed)||id!=allowed)
            {context.Response.StatusCode=403;return;}
            try
            {
                using(SqlConnection c=new SqlConnection(ConfigurationManager.ConnectionStrings["SchoolDB"].ConnectionString))
                using(SqlCommand q=new SqlCommand("SELECT TOP(1) StudentImageData,StudentImageContentType FROM dbo.Students WHERE StudentID=@ID",c))
                {q.Parameters.Add("@ID",SqlDbType.Int).Value=id;c.Open();using(SqlDataReader r=q.ExecuteReader()){if(r.Read()&&r["StudentImageData"]!=DBNull.Value){byte[] data=(byte[])r["StudentImageData"];context.Response.ContentType=Convert.ToString(r["StudentImageContentType"]);if(string.IsNullOrWhiteSpace(context.Response.ContentType))context.Response.ContentType="image/jpeg";context.Response.Cache.SetCacheability(HttpCacheability.Private);context.Response.BinaryWrite(data);return;}}}
            }catch(Exception ex){System.Diagnostics.Trace.TraceWarning("[PublicStudentImage] "+ex.Message);}
            string fallback=context.Server.MapPath("~/images/noimage.png");context.Response.ContentType="image/png";if(File.Exists(fallback))context.Response.WriteFile(fallback);
        }
        public bool IsReusable{get{return false;}}
    }
}
