namespace MMS.DTO.Permissions
{
    public class PostPermissionDto
    {
        public int ItemId { get; set; }
        public int TypeId { get; set; }
        public int? LevelId { get; set; }
        public int? Value { get; set; }
        public bool? AllDeleted { get; set; }
    }
}
