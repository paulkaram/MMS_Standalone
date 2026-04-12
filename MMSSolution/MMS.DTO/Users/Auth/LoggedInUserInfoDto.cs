namespace MMS.DTO.Users.Auth
{
    public record LoggedInUserInfoDto(string Id, string FullnameAr, string FullnameEn, string Language, string Email, string Mobile, string NationalId, bool HasProfilePicture);
}
