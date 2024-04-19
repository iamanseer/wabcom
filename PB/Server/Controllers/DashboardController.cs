using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using PB.Shared.Models;
using PB.DatabaseFramework;
using PB.Model;
using PB.Server.Repository;
using PB.Shared;
using PB.Shared.Enum;
using PB.Shared.Helpers;
using PB.Shared.Tables;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.RegularExpressions;
using static NPOI.HSSF.Util.HSSFColor;
using PB.Shared.Enum.CRM;
using PB.CRM.Model.Enum;
using PB.EntityFramework;

namespace PB.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DashboardController : BaseController
    {
        private readonly IDbContext _dbContext;
        public DashboardController(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Dashboard

        [HttpGet("get-dashboard-count")]
        public async Task<IActionResult> GetDashboardCount()
        {
            int pendingEnquiryCount = await _dbContext.GetByQueryAsync<int>($"Select Count(EnquiryID) from Enquiry Where CurrentFollowupNature={(int)FollowUpNatures.Followup} and IsDeleted=0 and BranchID={CurrentBranchID}", null);
            int pendingQuotationCount = await _dbContext.GetByQueryAsync<int>($"Select Count(QuotationID) from Quotation Where CurrentFollowupNature={(int)FollowUpNatures.Followup} and IsDeleted=0 and BranchID={CurrentBranchID}", null);

            
            int closedEnquiryCount = await _dbContext.GetByQueryAsync<int>($"Select Count(EnquiryID) from Enquiry Where CurrentFollowupNature={(int)FollowUpNatures.Cancelled} and IsDeleted=0 and BranchID={CurrentBranchID}", null);
            int closedQuotationCount = await _dbContext.GetByQueryAsync<int>($"Select Count(QuotationID) from Quotation Where CurrentFollowupNature={(int)FollowUpNatures.Cancelled} and IsDeleted=0 and BranchID={CurrentBranchID}", null);

            var res = new DashboardCountModel
            {
                NewEnquiries = await _dbContext.GetByQueryAsync<int>($"Select Count(EnquiryID) from Enquiry Where CurrentFollowupNature = {(int)FollowUpNatures.New} and IsDeleted=0 and BranchID={CurrentBranchID}", null),
                Followups = pendingEnquiryCount + pendingQuotationCount,
                Interested = await _dbContext.GetByQueryAsync<int>($"Select Count(EnquiryID) from Enquiry Where CurrentFollowupNature={(int)FollowUpNatures.Converted} and IsDeleted=0 and BranchID={CurrentBranchID}", null),
                Closed = closedEnquiryCount + closedQuotationCount,
                TotalEnquiry = await _dbContext.GetByQueryAsync<int>($"Select Count(EnquiryID) from Enquiry Where IsDeleted=0 and BranchID={CurrentBranchID}", null),
            };

            return Ok(res);
        }



        [HttpGet("get-dashboard-follow-up-list")]
        public async Task<IActionResult> GetDashboardFollowUpList()
        {
            var res = new List<DashboardFollowUpListModel>();

            string QuotationCondition = "";
            string EnquiryCondition = "";
            if (CurrentUserTypeID > (int)UserTypes.Client)
            {
                List<int> accessibleQuotations = await _dbContext.GetListByQueryAsync<int>($@"
																							SELECT Q.QuotationID 
                                                                                            FROM Quotation Q 
                                                                                            LEFT JOIN QuotationAssignee QA ON Q.QuotationID = QA.QuotationID AND QA.IsDeleted=0 
                                                                                            WHERE (QA.QuotationID IS NULL OR QA.EntityID={CurrentEntityID}) AND Q.BranchID={CurrentBranchID} AND Q.IsDeleted=0
                ", null);

                if (accessibleQuotations.Count > 0)
                {
                    string commaSeparatedIds = string.Join(",", accessibleQuotations);
                    QuotationCondition += $" AND vF.QuotationID in ({commaSeparatedIds})";
                }

                List<int> accessibleEnquiries = await _dbContext.GetListByQueryAsync<int>($@"SELECT E.EnquiryID 
                                                                                            FROM Enquiry E 
                                                                                            LEFT JOIN EnquiryAssignee EA ON E.EnquiryID = EA.EnquiryID AND EA.IsDeleted=0 
                                                                                            WHERE (EA.EntityID = {CurrentEntityID} OR EA.EnquiryID IS NULL) AND E.BranchID={CurrentBranchID} AND E.IsDeleted=0", null);

                if (accessibleEnquiries.Count > 0)
                {
                    string commaSeparatedIds = string.Join(",", accessibleEnquiries);

                    EnquiryCondition += $" or vF.EnquiryID in ({commaSeparatedIds})";
                }
            }

            res = await _dbContext.GetListByQueryAsync<DashboardFollowUpListModel>($@"Select CustomerName,
                                                                                    Case 
                                                                                    When Type={(int)FollowUpTypes.Enquiry} Then EnquiryNo
                                                                                    When Type={(int)FollowUpTypes.Quotation}  Then QuotationNo
                                                                                    End As No,
                                                                                    Type,vF.EnquiryID,vF.QuotationID,Convert(date,DATEADD(MINUTE,330, getutcdate())) Today
                                                                                    From viFollowUp vF
                                                                                    Left Join Enquiry E on E.EnquiryID=vF.EnquiryID  and Type={(int)FollowUpTypes.Enquiry} 
                                                                                    Left Join Quotation Q on Q.QuotationID=vF.QuotationID  and Type={(int)FollowUpTypes.Quotation}
                                                                                    Where vF.BranchID={CurrentBranchID} and Today=Convert(date,NextFollowUpDate) and (vF.CurrentFollowUpNature=0 or vF.CurrentFollowUpNature={(int)FollowUpNatures.Followup}) {QuotationCondition} {EnquiryCondition}", null);

            return Ok(res);
        }


        //[HttpGet("get-dashboard-leadquality-chart-count")]
        //public async Task<IActionResult> GetDashboardDonutChartCount()
        //{
        //    var res = new List<DashboardDonutChartModel>();

        //    res = await _dbContext.GetListByQueryAsync<DashboardDonutChartModel>($@"Select * From (Select count(*) as DataCount,Case When LeadQuality={(int)LeadQualities.Hot} Then 'Hot' End as FieldName,LeadQuality
        //                                                                                From Enquiry
        //                                                                                Where ClientID={CurrentClientID} and IsDeleted=0 and LeadQuality={(int)LeadQualities.Hot}
        //                                                                                Group By LeadQuality
        //                                                                                UNION
        //                                                                                Select count(*) as DataCount,Case When LeadQuality={(int)LeadQualities.Warm} Then 'Warm' End as FieldName,LeadQuality
        //                                                                                From Enquiry
        //                                                                                Where ClientID={CurrentClientID} and IsDeleted=0 and LeadQuality={(int)LeadQualities.Warm}
        //                                                                                Group By LeadQuality
        //                                                                                UNION
        //                                                                                Select count(*) as DataCount,Case When LeadQuality={(int)LeadQualities.Cold} Then 'Cold' End as FieldName,LeadQuality
        //                                                                                From Enquiry
        //                                                                                Where ClientID={CurrentClientID} and IsDeleted=0 and LeadQuality={(int)LeadQualities.Cold}
        //                                                                                Group By LeadQuality) as A
        //                                                                                Order By A.LeadQuality Asc");

        //    return Ok(res);
        //}



        [HttpGet("get-dashboard-leadquality-chart-count")]
        public async Task<IActionResult> GetDashboardDonutChartCount()
        {
            var res = new List<DashboardLeadQualityChartModel>();

            res = await _dbContext.GetListByQueryAsync<DashboardLeadQualityChartModel>($@"WITH Natures (Nature) AS (
                                                                                    SELECT 1
                                                                                    UNION ALL
                                                                                    SELECT 2
                                                                                    UNION ALL 
                                                                                    SELECT 3
                                                                                )
                                                                                SELECT COUNT(E.LeadQuality) AS DataCount, N.Nature AS LeadQuality,
                                                                                Case When N.Nature={(int)LeadQualities.Hot} then 'Hot'
		                                                                                When N.Nature={(int)LeadQualities.Warm} then 'Warm'
		                                                                                When N.Nature={(int)LeadQualities.Cold} then 'Cold'
		                                                                                end as FieldName
                                                                                FROM Natures N
                                                                                LEFT JOIN Enquiry E ON E.LeadQuality = N.Nature
                                                                                                    AND E.BranchID = {CurrentBranchID}
                                                                                                    AND E.IsDeleted = 0
                                                                                GROUP BY N.Nature
                                                                                ORDER BY LeadQuality", null);

            return Ok(res);
        }

        [HttpGet("get-lead-through-chart-counts")]
        public async Task<IActionResult> GetDashboardLeadMediaChartCount()
        {
            var res = new List<DashboardLeadThroughChartModel>();

            //res = await _dbContext.GetListByQueryAsync<DashboardLeadThroughChartModel>($@"Select count(E.LeadThroughID) As DataCount,Name as FieldName,LT.LeadThroughID 
            //                                                                            From LeadThrough LT
            //                                                                            Left Join Enquiry E on E.LeadThroughID=LT.LeadThroughID and E.IsDeleted=0 
            //                                                                            Where LT.IsDeleted =0 and (LT.ClientID={CurrentClientID} OR LT.ClientID IS NULL )
            //                                                                            Group By LT.LeadThroughID, Name
            //");

            //res = await _dbContext.GetListByQueryAsync<DashboardLeadThroughChartModel>($@"SELECT COUNT(E.LeadThroughID) AS DataCount,Name as FieldName
            //                                                                                FROM Enquiry E
            //                                                                                LEFT JOIN LeadThrough LT ON LT.LeadThroughID = E.LeadThroughID AND LT.IsDeleted=0
            //                                                                                WHERE E.ClientID ={CurrentClientID} AND E.IsDeleted=0
            //                                                                                GROUP BY E.LeadThroughID,Name
            //                                                                                            ");
            res = await _dbContext.GetListByQueryAsync<DashboardLeadThroughChartModel>($@"SELECT COUNT(E.LeadThroughID) AS DataCount,Name as FieldName,E.LeadThroughID
                                                                                            FROM Enquiry E
                                                                                            LEFT JOIN LeadThrough LT ON LT.LeadThroughID = E.LeadThroughID
                                                                                            WHERE E.BranchID ={CurrentBranchID} AND E.IsDeleted = 0 and (LT.ClientID={CurrentClientID} OR LT.ClientID IS NULL) and LT.IsDeleted = 0
                                                                                            GROUP BY E.LeadThroughID,Name
            ", null);


            return Ok(res);
        }

        [HttpGet("get-dashboard-bar-chart-data")]
        public async Task<IActionResult> GetDashboardLeadAnalyticChartCount()
        {
            List<DashboardDataAnalyticGraphModel> res = new List<DashboardDataAnalyticGraphModel>();
            var barCartModel = new DashboardBarChartDataModel();

            List<int> monthList = await _dbContext.GetListByQueryAsync<int>($@"
                                                                            Select MONTH(Date) [Month] 
                                                                            From Enquiry
                                                                            Where IsDeleted=0 and BranchID={CurrentBranchID}
                                                                            Group by MONTH(Date)
                                                                            Order by 1
            ", null);

            if (monthList.Count > 0)
            {
                string commaSeparatedMonthValues = string.Join(",", monthList);
                res = await _dbContext.GetListByQueryAsync<DashboardDataAnalyticGraphModel>($@"
																			SELECT
																				Months.EnquiryMonth AS Month,
																				NatureEnum.CurrentFollowupNature,
																				ISNULL(EnquiryCounts.EnquiryCount, 0) AS Count
																			FROM
																				(
																					SELECT DISTINCT
																						DATEPART(MONTH, [Date]) AS EnquiryMonth
																					FROM
																						[dbo].[Enquiry]
																				   WHERE
																						BranchID={CurrentBranchID}
																						--[Date] >= '2023-04-01' AND [Date] < '2023-07-01' -- Adjust the date range as needed
																				) AS Months
																			CROSS JOIN
																				(
																					SELECT 0 AS CurrentFollowupNature
																					UNION ALL
																					SELECT 1 AS CurrentFollowupNature
																					UNION ALL
																					SELECT 2 AS CurrentFollowupNature
																					UNION ALL
																					SELECT 3 AS CurrentFollowupNature
																				) AS NatureEnum
																			LEFT JOIN
																				(
																					SELECT
																						DATEPART(MONTH, [Date]) AS EnquiryMonth,
																						[CurrentFollowupNature],
																						COUNT(*) AS EnquiryCount
																					FROM
																						[dbo].[Enquiry]
																					WHERE
																						BranchID={CurrentBranchID} AND
																						--[Date] >= '2023-04-01' AND [Date] < '2023-07-01' -- Adjust the date range as needed
																						[CurrentFollowupNature] IN (0,1,2, 3) -- Exclude nature 1 if desired
																					GROUP BY
																						DATEPART(MONTH, [Date]),
																						[CurrentFollowupNature]
																				) AS EnquiryCounts
																					ON Months.EnquiryMonth = EnquiryCounts.EnquiryMonth
																					AND NatureEnum.CurrentFollowupNature = EnquiryCounts.CurrentFollowupNature
																			ORDER BY
																				Months.EnquiryMonth,
																				NatureEnum.CurrentFollowupNature", null);

                if (res != null && res.Count > 0)
                {
                    for (int i = 0; i < monthList.Count; i++)
                    {
                        string monthName = new DateTime(DateTime.Now.Year, monthList[i], 1).ToString("MMMM");
                        int newCount = res.Where(r => r.Month == monthList[i] && r.CurrentFollowupNature == 0).First().Count;
                        int followUpCount = res.Where(r => r.Month == monthList[i] && r.CurrentFollowupNature == (int)FollowUpNatures.Followup).First().Count;
                        int businessCount = res.Where(r => r.Month == monthList[i] && r.CurrentFollowupNature == (int)FollowUpNatures.Converted).First().Count;
                        int closedCount = res.Where(r => r.Month == monthList[i] && r.CurrentFollowupNature == (int)FollowUpNatures.Cancelled).First().Count;
                        int totalCount = newCount + followUpCount + businessCount + closedCount;
                        barCartModel.Months.Add(monthName);
                        barCartModel.NewEnquiryCounts.Add(newCount);
                        barCartModel.InFollowupEnquiryCounts.Add(followUpCount);
                        barCartModel.InBusinessEnquiryCounts.Add(businessCount);
                        barCartModel.InClosedEnquiryCount.Add(closedCount);
                        barCartModel.TotalEnquiryCounts.Add(totalCount);
                    }
                }
            }
            return Ok(barCartModel ?? new());
        }

        #endregion
    }
}
