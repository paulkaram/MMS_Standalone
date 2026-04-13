namespace MMS.DTO.Delegations
{
    public class DelegationDto
    {
        public int Id { get; set; }
        public string FromUserId { get; set; } = null!;
        public string FromUserName { get; set; } = null!;
        public string ToUserId { get; set; } = null!;
        public string ToUserName { get; set; } = null!;
        public int TypeId { get; set; }
        public string? TypeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsCurrentlyActive { get; set; }
        public string? Reason { get; set; }
        public List<int> TaskIds { get; set; } = new();
        public DateTime CreatedDate { get; set; }
    }
}
