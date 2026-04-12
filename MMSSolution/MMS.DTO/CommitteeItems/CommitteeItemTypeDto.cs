namespace MMS.DTO.CommitteeItems
{
    public class CommitteeItemTypeDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;
        public int UsageCount { get; set; }
    }
}
