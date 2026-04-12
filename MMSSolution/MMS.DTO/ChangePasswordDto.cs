namespace MMS.DTO
{
    public record ChangePasswordDto(string OldPassword, string NewPassword, string? Code, string? Validator);
}
