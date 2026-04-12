namespace MMS.DTO
{
    public record ListItemWithDataDto<T>(int Id, string Name, List<T> Data);
}
