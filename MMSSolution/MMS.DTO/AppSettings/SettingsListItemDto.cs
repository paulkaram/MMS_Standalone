namespace MMS.DTO.AppSettings
{
    public class SettingsListItemDto
    {
        public string? CategoryName { get; set; }
        public List<SettingsListItemSubDto>? Items { get; set; }
    }
}
