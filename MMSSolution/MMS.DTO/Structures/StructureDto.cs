namespace MMS.DTO.Structures
{
    public record StructureDto(int? Id, string Code, string NameAr, string NameEn, string Description, int TypeId,int? BranchId, int? ParentId, bool Active, bool ExternalStructure);
}
