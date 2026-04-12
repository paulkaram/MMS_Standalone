namespace MMS.DTO.Users
{
    public record RegisterUserDto(int? Id, string FullNameAr, string FullNameEn, string Username, string Email, string? Mobile, string? NationalId, bool Approved, int DefaultLanguageId, string Password);
}
