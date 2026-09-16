using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace DigitalSchoolManager
{
    internal static class BudgetReportBuilder
    {
        internal static string Build(DataSet data)
        {
            DataRow header = data.Tables[0].Rows[0];
            DataTable staff = data.Tables[1];
            DataTable allowances = data.Tables[2];
            DataTable objects = data.Tables[3];
            DataTable posts = data.Tables[4];
            DataTable scales = data.Tables[5];
            var html = new StringBuilder(131072);
            html.Append(BuildFront(header));
            html.Append(BuildSummary(header, staff, allowances, objects));
            html.Append(BuildPay(header, staff, allowances));
            html.Append(BuildBdo3(header, staff));
            html.Append(BuildBdo4(header, staff, allowances));
            html.Append(BuildStrengthForm("414-BDC-2", "ESTABLISHMENT STRENGTH BY FUNCTION", header, staff, posts));
            html.Append(BuildEstablishmentForm("414-BDC-3", header, staff, allowances, posts));
            html.Append(BuildStrengthForm("414-BDC-4", "ESTABLISHMENT STRENGTH BY DESIGNATION", header, staff, posts));
            html.Append(BuildEstablishmentForm("414-BDC-5", header, staff, allowances, posts));
            html.Append(BuildBm10(header, staff, scales));
            return html.ToString();
        }

        private static string BuildFront(DataRow h)
        {
            var b = StartForm("Front 414", "budget-front-form");
            b.Append("<div class='budget-front-inner'>").Append(Logo()).Append("<span>Government of the Punjab</span><h1>BUDGET ESTIMATE</h1><h2>FOR THE FINANCIAL YEAR ")
             .Append(E(Fiscal(h))).Append("</h2><div class='budget-front-rule'></div><h3>").Append(E(S(h,"SchoolName"))).Append("</h3><p>Cost Center <strong>")
             .Append(E(S(h,"CostCenterCode"))).Append("</strong></p><div class='budget-front-file'><span>Permanent budget file</span><strong>").Append(E(S(h,"BudgetName"))).Append("</strong><small>")
             .Append(E(S(h,"Status"))).Append("</small></div></div>");
            return EndForm(b);
        }

        private static string BuildSummary(DataRow h, DataTable staff, DataTable allowances, DataTable objects)
        {
            decimal pay = staff.AsEnumerable().Sum(AnnualPay);
            decimal allowance = allowances.AsEnumerable().Sum(r => D(r,"MonthlyAmount") * 12m);
            decimal other = objects.AsEnumerable().Sum(r => D(r,"ProposedBudget"));
            var b = StartForm("Summary 414", "budget-summary-form");
            AppendOfficialHeader(b, h, "Summary 414", "FUNCTIONAL CUM OBJECT CLASSIFICATION AND PARTICULARS OF THE SCHEME");
            b.Append("<table class='official-budget-table'><thead><tr><th>Object / Post Code</th><th>Functional Classification and Particulars</th><th>BPS</th><th>No. of Posts</th><th>Previous Budget</th><th>Current Revised</th><th>Budget Estimate ")
             .Append(E(Fiscal(h))).Append("</th></tr></thead><tbody>");
            AppendSummaryRow(b,"A01","TOTAL EMPLOYEES RELATED EXPENSES","",staff.Rows.Count,0,0,pay+allowance,true);
            AppendSummaryRow(b,"A011","TOTAL PAY","",staff.Rows.Count,0,0,pay,true);
            AppendSummaryRow(b,"A011-1","TOTAL PAY OF OFFICERS","",CountBps(staff,16,true),0,0,SumPay(staff,16,true),true);
            AppendSummaryRow(b,"A01101","Total Basic Pay of Officers","",CountBps(staff,16,true),0,0,SumPay(staff,16,true),false);
            AppendPostSummaryRows(b, staff, 16, true, true);
            AppendSummaryRow(b,"A011-2","TOTAL PAY OF OTHER STAFF","",CountBps(staff,16,false),0,0,SumPay(staff,16,false),true);
            AppendSummaryRow(b,"A01151","Total Basic Pay of Other Staff","",CountBps(staff,16,false),0,0,SumPay(staff,16,false),false);
            AppendPostSummaryRows(b, staff, 16, false, true);
            AppendSummaryRow(b,"A012","TOTAL ALLOWANCES","",staff.Rows.Count,0,0,allowance,true);
            foreach (IGrouping<string,DataRow> group in allowances.AsEnumerable().GroupBy(r => S(r,"AllowanceCode")))
            {
                DataRow first=group.First();
                AppendSummaryRow(b,S(first,"AllowanceCode"),S(first,"AllowanceName"),"","",0,0,group.Sum(r=>D(r,"MonthlyAmount")*12m),false);
            }
            decimal utilities=ObjectAmount(objects,"A03303","ProposedBudget");
            decimal travel=ObjectAmount(objects,"A03805","ProposedBudget");
            decimal leave=ObjectAmount(objects,"A04114","ProposedBudget");
            AppendSummaryRow(b,"A033","TOTAL UTILITIES","","",ObjectAmount(objects,"A03303","PreviousBudget"),ObjectAmount(objects,"A03303","CurrentRevised"),utilities,true);
            AppendObjectRow(b,objects,"A03303");
            AppendSummaryRow(b,"A038","TOTAL TRAVELLING ALLOWANCE","","",ObjectAmount(objects,"A03805","PreviousBudget"),ObjectAmount(objects,"A03805","CurrentRevised"),travel,true);
            AppendObjectRow(b,objects,"A03805");
            AppendSummaryRow(b,"A04","TOTAL PENSION","","",ObjectAmount(objects,"A04114","PreviousBudget"),ObjectAmount(objects,"A04114","CurrentRevised"),leave,true);
            AppendSummaryRow(b,"A041","PENSION","","",ObjectAmount(objects,"A04114","PreviousBudget"),ObjectAmount(objects,"A04114","CurrentRevised"),leave,true);
            AppendObjectRow(b,objects,"A04114"); AppendObjectRow(b,objects,"A06103");
            AppendSummaryRow(b,"","GRAND TOTAL","",staff.Rows.Count,objects.AsEnumerable().Sum(r=>D(r,"PreviousBudget")),objects.AsEnumerable().Sum(r=>D(r,"CurrentRevised")),pay+allowance+other,true);
            b.Append("</tbody></table>"); AppendSignatures(b); return EndForm(b);
        }

        private static string BuildPay(DataRow h, DataTable staff, DataTable allowances)
        {
            List<DataRow> heads = allowances.AsEnumerable().GroupBy(r=>Convert.ToInt32(r["AllowanceID"],CultureInfo.InvariantCulture)).Select(g=>g.First()).OrderBy(r=>S(r,"AllowanceCode")).ToList();
            var map = AllowanceMap(allowances);
            var b = StartForm("414-Pay", "budget-wide-form");
            AppendOfficialHeader(b,h,"414-Pay","REGULAR POSTS PAY WISE, SCALE WISE AND DESIGNATION WISE");
            b.Append("<table class='official-budget-table budget-allowance-matrix'><thead><tr><th>No.</th><th>Name of Officer / Official</th><th>Designation</th><th>BPS</th><th>Basic Pay</th>");
            foreach(DataRow a in heads) b.Append("<th>").Append(E(S(a,"AllowanceName"))).Append("<small>").Append(E(S(a,"AllowanceCode"))).Append("</small></th>");
            b.Append("<th>Total Monthly Pay</th></tr></thead><tbody>");
            int index=0;
            foreach(DataRow row in staff.Rows)
            {
                index++; decimal total=D(row,"BasicPay");
                b.Append("<tr><td>").Append(index).Append("</td><td>").Append(E(S(row,"EmployeeName"))).Append("</td><td>").Append(E(S(row,"Designation"))).Append("</td><td>").Append(I(row,"BPS")).Append("</td><td class='money'>").Append(N(D(row,"BasicPay"))).Append("</td>");
                foreach(DataRow a in heads){ decimal value=GetAllowance(map,I(row,"BudgetStaffLineID"),I(a,"AllowanceID")); total+=value; b.Append("<td class='money'>").Append(N(value)).Append("</td>"); }
                b.Append("<td class='money total-cell'>").Append(N(total)).Append("</td></tr>");
            }
            b.Append("</tbody></table>"); AppendSignatures(b); return EndForm(b);
        }

        private static string BuildBdo3(DataRow h, DataTable staff)
        {
            var b=StartForm("414-BDO-3",""); AppendOfficialHeader(b,h,"FORM BDO-3","SCHEDULE OF ESTABLISHMENT - CALCULATION OF PAY OF OFFICERS / OTHER STAFF");
            b.Append("<div class='budget-rule-note'>[See Rules 25] &nbsp; | &nbsp; Financial Year ").Append(E(Fiscal(h))).Append("</div><table class='official-budget-table'><thead><tr><th>No.</th><th>Name</th><th>Post / Designation</th><th>BPS</th><th>Pay on 1 July</th><th>Increment</th><th>Pay on 1 January</th><th>First Six Months</th><th>Last Six Months</th><th>Total Provision</th></tr></thead><tbody>");
            int index=0; decimal grand=0;
            foreach(DataRow row in staff.Rows){index++;decimal basePay=D(row,"BasicPay"),inc=D(row,"IncrementRate"),first=basePay*6,last=(basePay+inc)*6,total=first+last;grand+=total;
                b.Append("<tr><td>").Append(index).Append("</td><td>").Append(E(S(row,"EmployeeName"))).Append("</td><td>").Append(E(S(row,"Designation"))).Append("</td><td>").Append(I(row,"BPS")).Append("</td><td class='money'>").Append(N(basePay)).Append("</td><td class='money'>").Append(N(inc)).Append("</td><td class='money'>").Append(N(basePay+inc)).Append("</td><td class='money'>").Append(N(first)).Append("</td><td class='money'>").Append(N(last)).Append("</td><td class='money'>").Append(N(total)).Append("</td></tr>");}
            b.Append("<tr class='grand-total'><td colspan='9'>GRAND TOTAL</td><td class='money'>").Append(N(grand)).Append("</td></tr></tbody></table>"); AppendSignatures(b); return EndForm(b);
        }

        private static string BuildBdo4(DataRow h, DataTable staff, DataTable allowances)
        {
            List<DataRow> heads=allowances.AsEnumerable().GroupBy(r=>I(r,"AllowanceID")).Select(g=>g.First()).OrderBy(r=>S(r,"AllowanceCode")).ToList();
            var map=AllowanceMap(allowances); var b=StartForm("414-BDO-4 (Allow)","budget-wide-form");
            AppendOfficialHeader(b,h,"FORM BDO-4","SCHEDULE OF ESTABLISHMENT - CALCULATION OF ALLOWANCES");
            b.Append("<div class='budget-rule-note'>[See Rules 25] &nbsp; | &nbsp; Financial Year ").Append(E(Fiscal(h))).Append("</div><table class='official-budget-table budget-allowance-matrix'><thead><tr><th>No.</th><th>Name of Officer / Teacher / Official</th><th>Designation</th><th>BS</th>");
            foreach(DataRow a in heads)b.Append("<th>").Append(E(S(a,"AllowanceName"))).Append("<small>").Append(E(S(a,"AllowanceCode"))).Append("</small></th>");
            b.Append("<th>Total Annual Allowances</th></tr></thead><tbody>"); int index=0;
            foreach(DataRow row in staff.Rows){index++;decimal total=0;b.Append("<tr><td>").Append(index).Append("</td><td>").Append(E(S(row,"EmployeeName"))).Append("</td><td>").Append(E(S(row,"Designation"))).Append("</td><td>").Append(I(row,"BPS")).Append("</td>");foreach(DataRow a in heads){decimal annual=GetAllowance(map,I(row,"BudgetStaffLineID"),I(a,"AllowanceID"))*12;total+=annual;b.Append("<td class='money'>").Append(N(annual)).Append("</td>");}b.Append("<td class='money total-cell'>").Append(N(total)).Append("</td></tr>");}
            b.Append("</tbody></table>"); AppendSignatures(b); return EndForm(b);
        }

        private static string BuildStrengthForm(string formName,string title,DataRow h,DataTable staff,DataTable posts)
        {
            var b=StartForm(formName,""); AppendOfficialHeader(b,h,"FORM "+formName.Replace("414-",string.Empty),title);
            b.Append("<div class='budget-rule-note'>[See Rules 19, 28, 52 and 54] &nbsp; | &nbsp; Financial Year ").Append(E(Fiscal(h))).Append("</div><table class='official-budget-table'><thead><tr><th rowspan='2'>No.</th><th rowspan='2'>Designation</th><th rowspan='2'>Post Code</th><th rowspan='2'>BPS</th><th colspan='3'>Sanctioned</th><th colspan='3'>Filled</th><th colspan='3'>Vacant</th><th colspan='3'>Recruitment Planned</th><th colspan='3'>Total Establishment</th></tr><tr><th>M</th><th>F</th><th>Total</th><th>M</th><th>F</th><th>Total</th><th>M</th><th>F</th><th>Total</th><th>M</th><th>F</th><th>Total</th><th>M</th><th>F</th><th>Total</th></tr></thead><tbody>");
            int index=0;
            foreach(DataRow post in posts.Rows){index++;int postId=I(post,"BudgetPostID"),san=I(post,"Sanctioned"),filledM=CountPost(staff,postId,false,"Male"),filledF=CountPost(staff,postId,false,"Female"),vacM=CountPost(staff,postId,true,"Male"),vacF=CountPost(staff,postId,true,"Female"),recM=CountRecruit(staff,postId,"Male"),recF=CountRecruit(staff,postId,"Female");int sanM=Math.Max(filledM+vacM, san);int sanF=0;
                b.Append("<tr><td>").Append(index).Append("</td><td>").Append(E(S(post,"PostName"))).Append("</td><td>").Append(E(S(post,"PostCode"))).Append("</td><td>").Append(I(post,"BPS")).Append("</td>");Numbers(b,sanM,sanF,sanM+sanF);Numbers(b,filledM,filledF,filledM+filledF);Numbers(b,vacM,vacF,vacM+vacF);Numbers(b,recM,recF,recM+recF);Numbers(b,filledM+recM,filledF+recF,filledM+filledF+recM+recF);b.Append("</tr>");}
            b.Append("</tbody></table>"); AppendSignatures(b); return EndForm(b);
        }

        private static string BuildEstablishmentForm(string formName,DataRow h,DataTable staff,DataTable allowances,DataTable posts)
        {
            var allowMap=AllowanceTotalsByPost(staff,allowances);var b=StartForm(formName,"");AppendOfficialHeader(b,h,"FORM "+formName.Replace("414-",string.Empty),"ESTABLISHMENT BUDGET BY FUNCTION AND DESIGNATION");
            b.Append("<div class='budget-rule-note'>[See Rules 19, 28, 52 and 54] &nbsp; | &nbsp; Financial Year ").Append(E(Fiscal(h))).Append("</div><table class='official-budget-table'><thead><tr><th>No.</th><th>Designation</th><th>Post Code</th><th>BPS</th><th>Male</th><th>Female</th><th>Total Strength</th><th>Establishment Charges</th><th>Leave Salary</th><th>Allowances</th><th>Pension</th><th>Total</th></tr></thead><tbody>");
            int index=0;decimal grand=0;
            foreach(DataRow post in posts.Rows){index++;int pid=I(post,"BudgetPostID"),m=CountPostAll(staff,pid,"Male"),f=CountPostAll(staff,pid,"Female");decimal pay=staff.AsEnumerable().Where(r=>I(r,"BudgetPostID")==pid).Sum(AnnualPay);decimal allow=allowMap.ContainsKey(pid)?allowMap[pid]:0;decimal total=pay+allow;grand+=total;b.Append("<tr><td>").Append(index).Append("</td><td>").Append(E(S(post,"PostName"))).Append("</td><td>").Append(E(S(post,"PostCode"))).Append("</td><td>").Append(I(post,"BPS")).Append("</td>");Numbers(b,m,f,m+f);b.Append("<td class='money'>").Append(N(pay)).Append("</td><td class='money'>0.00</td><td class='money'>").Append(N(allow)).Append("</td><td class='money'>0.00</td><td class='money'>").Append(N(total)).Append("</td></tr>");}
            b.Append("<tr class='grand-total'><td colspan='11'>GRAND TOTAL</td><td class='money'>").Append(N(grand)).Append("</td></tr></tbody></table>");AppendSignatures(b);return EndForm(b);
        }

        private static string BuildBm10(DataRow h,DataTable staff,DataTable scales)
        {
            var b=StartForm("414-BM-10","");AppendOfficialHeader(b,h,"FORM BM-10","NOMINAL ROLL AND DETAILED PROPOSED PROVISION FOR PAY");
            b.Append("<table class='official-budget-table'><thead><tr><th>No.</th><th>Name of Officer / Teacher / Official</th><th>Designation</th><th>BPS</th><th>Minimum</th><th>Maximum</th><th>Basic Pay</th><th>Annual Basic</th><th>Increment Date</th><th>Increment Rate</th><th>Increment Provision</th><th>Total Provision</th></tr></thead><tbody>");int index=0;decimal grand=0;
            foreach(DataRow row in staff.Rows){index++;decimal basic=D(row,"BasicPay"),inc=D(row,"IncrementRate"),annual=AnnualPay(row),incProvision=Math.Max(0,annual-(basic*12));grand+=annual;b.Append("<tr><td>").Append(index).Append("</td><td>").Append(E(S(row,"EmployeeName"))).Append("</td><td>").Append(E(S(row,"Designation"))).Append("</td><td>").Append(I(row,"BPS")).Append("</td><td class='money'>").Append(N(D(row,"MinimumPay"))).Append("</td><td class='money'>").Append(N(D(row,"MaximumPay"))).Append("</td><td class='money'>").Append(N(basic)).Append("</td><td class='money'>").Append(N(basic*12)).Append("</td><td>").Append(Date(row,"IncrementDate")).Append("</td><td class='money'>").Append(N(inc)).Append("</td><td class='money'>").Append(N(incProvision)).Append("</td><td class='money'>").Append(N(annual)).Append("</td></tr>");}
            b.Append("<tr class='grand-total'><td colspan='11'>GRAND TOTAL</td><td class='money'>").Append(N(grand)).Append("</td></tr></tbody></table><h3 class='budget-scale-heading'>Reference Pay Scales</h3><table class='official-budget-table budget-scale-table'><thead><tr><th>BPS</th><th>Minimum</th><th>Maximum</th><th>Annual Increment</th><th>BPS</th><th>Minimum</th><th>Maximum</th><th>Annual Increment</th></tr></thead><tbody>");
            for(int x=0;x<10;x++){DataRow a=scales.Rows.Count>x?scales.Rows[x]:null;DataRow c=scales.Rows.Count>x+10?scales.Rows[x+10]:null;b.Append("<tr>");AppendScale(b,a);AppendScale(b,c);b.Append("</tr>");}b.Append("</tbody></table>");AppendSignatures(b);return EndForm(b);
        }

        private static void AppendOfficialHeader(StringBuilder b,DataRow h,string form,string title)
        {
            b.Append("<header class='official-budget-header'>").Append(Logo()).Append("<div><span>Government of the Punjab</span><h2>").Append(E(S(h,"SchoolName"))).Append("</h2><strong>").Append(E(form)).Append("</strong><p>").Append(E(title)).Append("</p></div><aside><span>Budget</span><strong>").Append(E(Fiscal(h))).Append("</strong><small>Cost Center ").Append(E(S(h,"CostCenterCode"))).Append("</small></aside></header><div class='official-budget-meta'><span><b>Local Government:</b> ").Append(E(S(h,"LocalGovernmentName"))).Append("</span><span><b>Demand:</b> ").Append(E(S(h,"DemandName"))).Append("</span><span><b>Grant:</b> ").Append(E(S(h,"GrantNo"))).Append("</span><span><b>Function:</b> ").Append(E(S(h,"DetailedFunctionCode"))).Append(" - ").Append(E(S(h,"DetailedFunctionName"))).Append("</span></div>");
        }

        private static StringBuilder StartForm(string name,string css){return new StringBuilder(16384).Append("<section class='official-budget-form ").Append(css).Append("' data-form='").Append(E(name)).Append("'><div class='official-form-name'>").Append(E(name)).Append("</div>");}
        private static string EndForm(StringBuilder b){return b.Append("<footer class='official-budget-footer'><span>Digital School Manager - Permanent Annual Budget Record</span><span>Printed ").Append(DateTime.Now.ToString("dd MMM yyyy",CultureInfo.InvariantCulture)).Append("</span></footer></section>").ToString();}
        private static string Logo(){return "<img class='official-budget-logo' src='"+E(System.Web.VirtualPathUtility.ToAbsolute("~/images/SchoolLogo.png"))+"' alt='School logo' />";}
        private static void AppendSignatures(StringBuilder b){b.Append("<div class='official-signatures'><span>Prepared By</span><span>Budget / Accounts Incharge</span><span>Principal / D.D.O.</span></div>");}
        private static void AppendSummaryRow(StringBuilder b,string code,string name,string bps,object posts,decimal previous,decimal revised,decimal proposed,bool total){b.Append("<tr").Append(total?" class='summary-total'":"").Append("><td>").Append(E(code)).Append("</td><td>").Append(E(name)).Append("</td><td>").Append(E(bps)).Append("</td><td>").Append(E(Convert.ToString(posts,CultureInfo.InvariantCulture))).Append("</td><td class='money'>").Append(N(previous)).Append("</td><td class='money'>").Append(N(revised)).Append("</td><td class='money'>").Append(N(proposed)).Append("</td></tr>");}
        private static void AppendPostSummaryRows(StringBuilder b,DataTable staff,int threshold,bool atOrAbove,bool unused){foreach(IGrouping<string,DataRow> g in staff.AsEnumerable().Where(r=>(I(r,"BPS")>=threshold)==atOrAbove).GroupBy(r=>S(r,"PostCode")+"|"+S(r,"Designation")+"|"+I(r,"BPS"))){DataRow f=g.First();AppendSummaryRow(b,S(f,"PostCode"),S(f,"Designation"),I(f,"BPS").ToString(CultureInfo.InvariantCulture),g.Count(),0,0,g.Sum(AnnualPay),false);}}
        private static void AppendObjectRow(StringBuilder b,DataTable objects,string code){DataRow r=objects.AsEnumerable().FirstOrDefault(x=>S(x,"ObjectCode")==code);if(r!=null)AppendSummaryRow(b,code,S(r,"ObjectName"),"","",D(r,"PreviousBudget"),D(r,"CurrentRevised"),D(r,"ProposedBudget"),false);}
        private static decimal ObjectAmount(DataTable t,string code,string column){DataRow r=t.AsEnumerable().FirstOrDefault(x=>S(x,"ObjectCode")==code);return r==null?0:D(r,column);}
        private static int CountBps(DataTable t,int threshold,bool above){return t.AsEnumerable().Count(r=>(I(r,"BPS")>=threshold)==above);}
        private static decimal SumPay(DataTable t,int threshold,bool above){return t.AsEnumerable().Where(r=>(I(r,"BPS")>=threshold)==above).Sum(AnnualPay);}
        private static decimal AnnualPay(DataRow r){decimal basic=D(r,"BasicPay"),inc=D(r,"IncrementRate");if(r["IncrementDate"]==DBNull.Value)return basic*12;DateTime date=Convert.ToDateTime(r["IncrementDate"],CultureInfo.InvariantCulture);int months=date.Month>=7?19-date.Month:7-date.Month;return basic*12+inc*Math.Max(0,months);}
        private static Dictionary<string,decimal> AllowanceMap(DataTable t){var d=new Dictionary<string,decimal>();foreach(DataRow r in t.Rows)d[I(r,"BudgetStaffLineID")+":"+I(r,"AllowanceID")]=D(r,"MonthlyAmount");return d;}
        private static decimal GetAllowance(Dictionary<string,decimal>d,int line,int allowance){decimal v;return d.TryGetValue(line+":"+allowance,out v)?v:0;}
        private static Dictionary<int,decimal> AllowanceTotalsByPost(DataTable staff,DataTable allowances){var lineToPost=staff.AsEnumerable().ToDictionary(r=>I(r,"BudgetStaffLineID"),r=>I(r,"BudgetPostID"));var result=new Dictionary<int,decimal>();foreach(DataRow a in allowances.Rows){int line=I(a,"BudgetStaffLineID"),post;if(!lineToPost.TryGetValue(line,out post))continue;decimal current;result.TryGetValue(post,out current);result[post]=current+D(a,"MonthlyAmount")*12;}return result;}
        private static int CountPost(DataTable t,int post,bool vacant,string gender){return t.AsEnumerable().Count(r=>I(r,"BudgetPostID")==post&&Convert.ToBoolean(r["IsVacant"],CultureInfo.InvariantCulture)==vacant&&S(r,"Gender")==gender);}
        private static int CountPostAll(DataTable t,int post,string gender){return t.AsEnumerable().Count(r=>I(r,"BudgetPostID")==post&&S(r,"Gender")==gender);}
        private static int CountRecruit(DataTable t,int post,string gender){return t.AsEnumerable().Count(r=>I(r,"BudgetPostID")==post&&Convert.ToBoolean(r["RecruitmentPlanned"],CultureInfo.InvariantCulture)&&S(r,"Gender")==gender);}
        private static void Numbers(StringBuilder b,int a,int c,int total){b.Append("<td>").Append(a).Append("</td><td>").Append(c).Append("</td><td>").Append(total).Append("</td>");}
        private static void AppendScale(StringBuilder b,DataRow r){if(r==null){b.Append("<td></td><td></td><td></td><td></td>");return;}b.Append("<td>").Append(I(r,"BPS")).Append("</td><td class='money'>").Append(N(D(r,"MinimumPay"))).Append("</td><td class='money'>").Append(N(D(r,"MaximumPay"))).Append("</td><td class='money'>").Append(N(D(r,"AnnualIncrement"))).Append("</td>");}
        private static string Fiscal(DataRow r){return I(r,"FiscalStartYear")+"-"+(I(r,"FiscalEndYear")%100).ToString("00",CultureInfo.InvariantCulture);}
        private static string Date(DataRow r,string column){return r[column]==DBNull.Value?"-":Convert.ToDateTime(r[column],CultureInfo.InvariantCulture).ToString("dd MMM yyyy",CultureInfo.InvariantCulture);}
        private static string S(DataRow r,string c){return r[c]==DBNull.Value?string.Empty:Convert.ToString(r[c],CultureInfo.InvariantCulture);}
        private static int I(DataRow r,string c){return r[c]==DBNull.Value?0:Convert.ToInt32(r[c],CultureInfo.InvariantCulture);}
        private static decimal D(DataRow r,string c){return r[c]==DBNull.Value?0:Convert.ToDecimal(r[c],CultureInfo.InvariantCulture);}
        private static string N(decimal v){return v==0?"-":v.ToString("N2",CultureInfo.InvariantCulture);}
        private static string E(string v){return HttpUtility.HtmlEncode(v??string.Empty);}
    }
}
