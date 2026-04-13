namespace MMS.DTO.Bids
{
    public class BidStatusHistoryDto
    {
        public int Id { get; set; }
        public int BidId { get; set; }
        public int? FromStatusId { get; set; }
        public string? FromStatusName { get; set; }
        public int ToStatusId { get; set; }
        public string ToStatusName { get; set; } = null!;
        public string ChangedBy { get; set; } = null!;
        public string ChangedByName { get; set; } = null!;
        public DateTime ChangedDate { get; set; }
        public string? Note { get; set; }
    }
}
