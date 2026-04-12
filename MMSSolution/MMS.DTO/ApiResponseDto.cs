namespace MMS.DTO
{
	public record ApiResponseDto<T>(T? Data = default, bool Success = true, string? Message = null);

}
