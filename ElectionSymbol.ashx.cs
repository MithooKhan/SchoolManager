using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.SessionState;

namespace DigitalSchoolManager
{
    public sealed class ElectionSymbol : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            int id;if(!int.TryParse(context.Request.QueryString["id"],out id)||id<=0){context.Response.StatusCode=404;return;}
            const string sql=@"SELECT TOP(1)c.SymbolImageData,c.SymbolContentType FROM dbo.StudentUnionCandidates c
INNER JOIN dbo.StudentUnionElections e ON e.ElectionID=c.ElectionID
WHERE c.CandidateID=@ID AND c.IsPublished=1
  AND (@IsAdmin=1 OR e.Status IN(N'CandidatesPublished',N'VotingOpen',N'VotingClosed',N'ResultsPublished'));";
            try{using(SqlConnection c=new SqlConnection(ConfigurationManager.ConnectionStrings["SchoolDB"].ConnectionString))using(SqlCommand q=new SqlCommand(sql,c))
            {q.Parameters.Add("@ID",SqlDbType.Int).Value=id;q.Parameters.Add("@IsAdmin",SqlDbType.Bit).Value=SystemUserSecurity.IsAdministrator(context);c.Open();using(SqlDataReader r=q.ExecuteReader()){if(r.Read()&&r["SymbolImageData"]!=DBNull.Value){context.Response.ContentType=Convert.ToString(r["SymbolContentType"]);if(string.IsNullOrWhiteSpace(context.Response.ContentType))context.Response.ContentType="image/jpeg";context.Response.Cache.SetCacheability(HttpCacheability.Public);context.Response.BinaryWrite((byte[])r["SymbolImageData"]);return;}}}}catch(Exception ex){System.Diagnostics.Trace.TraceWarning("[ElectionSymbol] "+ex.Message);}
            context.Response.Redirect("~/images/noimage.png",false);
        }
        public bool IsReusable{get{return false;}}
    }
}
