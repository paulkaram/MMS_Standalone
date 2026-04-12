namespace MMS.API.Common.Filters
{
	public class HttpMethodRestrictionMiddleware
	{
		private readonly RequestDelegate _next;

		public HttpMethodRestrictionMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			// Deny TRACE, OPTIONS, and HEAD methods
			if (context.Request.Method == HttpMethods.Trace ||
				context.Request.Method == HttpMethods.Head)
			{
				context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed; // Return 405 Method Not Allowed
				await context.Response.WriteAsync("Method Not Allowed");
				return;
			}

			// Proceed with the next middleware
			await _next(context);
		}
	}
}
