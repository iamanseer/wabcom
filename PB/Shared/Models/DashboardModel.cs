using PB.Shared.Models;
using PB.Model;
using PB.Shared;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using System;

namespace PB.Shared.Models
{
	public class DashboardCountModel
    {
        public int NewEnquiries { get; set; }
        public int Followups { get; set; }
        public int Closed { get; set; }
        public int Interested { get; set; }
        public int TotalEnquiry { get; set; }

		public int Total { get
            {
                return NewEnquiries + Followups + Closed + Interested;
            } 
        }
    }

    public class DashboardFollowUpListModel
    {
        public string? CustomerName { get; set; }
        public int? Type { get; set; }
        public int? No { get; set; }
        public int EnquiryID { get; set; }
        public int QuotationID { get; set; }
    }

    public class DashboardLeadQualityChartModel
	{
        public int? DataCount { get; set; }
       
        public string? FieldName { get; set; }

        public string? Color
        {
            get
            {
                switch (FieldName)
                {
                    case "Warm": return "e3dd1b";
                    case "Hot":return "f54c4c";
                    case "Cold":return "a1c9e5";
                }
                return "";
            }
        }
    }
    public class DashboardLeadThroughChartModel
    {
        public int? DataCount { get; set; }
        public string? FieldName { get; set; }
        private string? color;
        public string? Color
        {
            get { return color ??= GenerateRandomColor(FieldName); }
            set { color = value; }
        }


        private static Random random = new Random();
        

        private static string GenerateRandomColor(string fieldName)
        {
            switch (fieldName)
            {
                case "Whatsapp":
                    return "25D366";
                case "Facebook":
                    return "1877F2";
                case "Instagram":
                    return "E1306C";
                case "Email":
                    return "E6E30C";
                case "Office":
                    return "6BE6E8";
                case "Referral":
                    return "65A1F9";
                case "Event":
                    return "D765F9";
                case "Advertisement":
                    return "B79FD6";
                case "Other":
                    return "D6D6D6";
                case "Phone":
                    return "EA1616";
                case "Website":
                    return "02A5C6";
                default:
                List<string> ColorCode = new List<string> { "25D366", "02A5C6", "EA1616", "D6D6D6", "B79FD6", "D765F9", "65A1F9", "6BE6E8", "E6E30C", "E1306C", "1877F2" };
                string color;

                    do
                    {
                        byte[] buffer = new byte[3];
                        random.NextBytes(buffer);
                        color = BitConverter.ToString(buffer).Replace("-", "");
                    }
                    while (ColorCode.Contains(color)); // Check if the generated color is already defined

                    ColorCode.Add(color); // Add the generated color to the list of predefined colors

                    return color;
            }
            
        }
    }
    
    public class DashboardDataAnalyticGraphModel
    {
        public int Month { get; set; }
        public int CurrentFollowupNature { get; set; }
        public int Count { get; set; }
    }
    public class DashboardBarChartDataModel
    {
        public List<string> Months { get; set; } = new();
        public List<int> NewEnquiryCounts { get; set; } = new();
        public List<int> InFollowupEnquiryCounts { get; set; } = new();
        public List<int> InBusinessEnquiryCounts { get; set; } = new();
        public List<int> InClosedEnquiryCount { get; set; } = new();
        public List<int> TotalEnquiryCounts { get; set; } = new();
    } 
}
