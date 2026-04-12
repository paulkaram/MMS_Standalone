using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MMS.API.Common.Filters;
public class ValidateModelFilter : IActionFilter
{
	public void OnActionExecuting(ActionExecutingContext context)
	{
		if (!context.ModelState.IsValid)
		{
			// Customize the error response here
			var errorResponse = new
			{
				message = "One or more validation errors occurred. Please check your input."
			};

			context.Result = new BadRequestObjectResult(errorResponse);
		}
	}

	public void OnActionExecuted(ActionExecutedContext context)
	{
		// No need to do anything after action execution in this case
	}
}