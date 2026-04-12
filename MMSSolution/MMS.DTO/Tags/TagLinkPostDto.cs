namespace MMS.DTO.Tags
{
    public class TagLinkPostDto
    {
        public int EntityTypeId { get; set; }
        public int EntityId { get; set; }
        public List<int> TagIds { get; set; } = new();
    }
}
