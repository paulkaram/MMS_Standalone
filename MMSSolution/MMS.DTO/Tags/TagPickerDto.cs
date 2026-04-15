namespace MMS.DTO.Tags
{
    /// <summary>
    /// Picker-friendly tag DTO: localized name plus color, no usage stats.
    /// Used by the in-form tag selector so it can render colored dots without
    /// hitting the permission-gated admin endpoint.
    /// </summary>
    public class TagPickerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Color { get; set; } = "#006d4b";
    }
}
