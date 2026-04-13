namespace MMS.DTO.ExternalMembers
{
    public class ExternalMemberPostDto
    {
        public string FullnameAr { get; set; } = null!;
        public string FullnameEn { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Mobile { get; set; }
        public string? Organization { get; set; }
        public string? Position { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
