namespace OMSV1.Application.Dtos.Offices
{
    public class OfficeStatisticsDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public int ReceivingStaff { get; set; }
        public int AccountStaff { get; set; }
        public int PrintingStaff { get; set; }
        public int QualityStaff { get; set; }
        public int DeliveryStaff { get; set; }
    }
}
