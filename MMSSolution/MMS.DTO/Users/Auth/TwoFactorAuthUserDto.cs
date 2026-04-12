namespace MMS.DTO.Users.Auth
{
    public class TwoFactorAuthUserDto
    {
        public int StatusCode { get; set; }
        public LoggedInUserInfoDto? UserInfo { get; set; }
    }
}
