using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MMS.API.Common.Filters;
public class ValidateModelFilter : IActionFilter
{
	public void OnActionExecuting(ActionExecutingContext context)
	{
		if (!context.ModelState.IsValid)
		{
			var errors = context.ModelState
				.Where(kv => kv.Value?.Errors.Count > 0)
				.ToDictionary(
					kv => kv.Key,
					kv => kv.Value!.Errors.Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? e.Exception?.Message : e.ErrorMessage).ToArray()
				);

			var errorResponse = new
			{
				message = "One or more validation errors occurred. Please check your input.",
				errors
			};

			context.Result = new BadRequestObjectResult(errorResponse);
		}
	}

	public void OnActionExecuted(ActionExecutedContext context)
	{
		// No need to do anything after action execution in this case
	}
}