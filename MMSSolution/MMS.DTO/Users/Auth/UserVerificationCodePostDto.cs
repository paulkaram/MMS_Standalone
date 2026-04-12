namespace MMS.DTO.Users.Auth
{
    public class UserVerificationCodePostDto
    {
        public LoggedInUserInfoDto User { get; set; }
        public string? Last4MobileDigits { get; set; }
    }
}
