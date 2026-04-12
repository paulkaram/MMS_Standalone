namespace MMS.DTO.Committees
{
    public class CommitteeDto
    {
        public bool Active { get; set; }
        public string Code { get; set; }
        public int? CommitteeClassificationId { get; set; }
        public int? CommitteeStyleId { get; set; }
        public int? CommitteeStatusId { get; set; }
        public string? Description { get; set; }
        public DateTime? EndDate { get; set; }
        public string? fileName { get; set; }
        public int? Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public int? ParentId { get; set; }
        public DateTime? StartDate { get; set; }
        public int TypeId { get; set; }
        public bool HasAdditionalMembers { get; set; }
        public bool IsInternal { get; set; }
        public bool IsPresentationRelated { get; set; }
        public string? AdditionalMemberName { get; set; }
    }
}
