namespace MMS.DTO.Bids
{
    public class BidStatusDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;
        public int StepOrder { get; set; }
    }
}
