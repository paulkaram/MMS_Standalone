namespace MMS.DTO.Delegations
{
    public class DelegationPostDto
    {
        public string ToUserId { get; set; } = null!;
        public int TypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
        public List<int> TaskIds { get; set; } = new();
    }
}
