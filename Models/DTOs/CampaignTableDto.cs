using System;

namespace Backend.Models.DTOs
{
    public class CampaignTableDto
    {
        public string CamId { get; set; }
        public string Description { get; set; }
        public double ClickThroughRate { get; set; }
        public double Cpc { get; set; }
        public double SpentAmount { get; set; }
        public int NoOfVisitors { get; set; }
        public int NoOfCustomers { get; set; }
        public DateTime Date { get; set; }
    }
}
