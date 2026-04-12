
namespace MMS.DTO.Users.Auth
{
    public record LoggedInUserDto(string Token, string RefreshToken, LoggedInUserInfoDto User);
}
