namespace Backend.Models
{
    public class CourierSummaryDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Completed { get; set; }
        public int Rejected { get; set; }
    }
}
