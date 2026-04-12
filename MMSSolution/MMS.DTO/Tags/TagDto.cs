namespace MMS.DTO.Tags
{
    public class TagDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;
        public string Color { get; set; } = null!;
        public int UsageCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
