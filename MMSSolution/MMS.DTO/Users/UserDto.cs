namespace MMS.DTO.Users
{
	public record UserDto(string? Id, string FullNameAr, string FullNameEn, string Username, string Email, string? Mobile, string? NationalId, bool Approved, int DefaultLanguageId, string? Password = null);
}
