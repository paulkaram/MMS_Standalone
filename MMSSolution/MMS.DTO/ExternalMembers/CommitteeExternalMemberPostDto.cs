namespace MMS.DTO.ExternalMembers
{
    public class CommitteeExternalMemberPostDto
    {
        public int CommitteeId { get; set; }
        public int ExternalMemberId { get; set; }
        public int CommitteeRoleId { get; set; }
        public string? Note { get; set; }
    }
}
