namespace MMS.DTO.ExternalMembers
{
    public class CommitteeExternalMemberDto
    {
        public int Id { get; set; }
        public int CommitteeId { get; set; }
        public int ExternalMemberId { get; set; }
        public string MemberName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Organization { get; set; }
        public string? Position { get; set; }
        public int CommitteeRoleId { get; set; }
        public string? CommitteeRoleName { get; set; }
        public bool Active { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
