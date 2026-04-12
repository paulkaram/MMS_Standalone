using MMS.BLL.Common.Logging;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DTO.LogActivity;
using Newtonsoft.Json;

namespace MMS.BLL.Managers
{
    /// <summary>
    /// Manager for logging user activities.
    /// DCC Compliance (NCA DCC-1:2022 Section 2-4): Enhanced with IP address, device info, and session tracking.
    /// </summary>
    public class LogIntalioActivityManager
	{
		private readonly IProcessUnitOfWork _processUnitOfWork;

		public LogIntalioActivityManager(IProcessUnitOfWork processUnitOfWork)
		{
			_processUnitOfWork = processUnitOfWork;
		}

		public async Task LogActivity(string logActivityUrl, string username, string userId, int operation, string actionName,
			string controllerName, string descriptionString, IDictionary<string, object> parameters,
			string? ipAddress = null, string? userAgent = null, string? sessionId = null, string? deviceInfo = null)
		{
			CustomObjectForLogActivity customObj = await ResolveCustomObj(controllerName, parameters);
			string description =
				ResolveDescriptionString(
					ResolveDescriptionString(descriptionString, parameters)
					, customObj);

			await LogToAudit.LogIntalioActivityAsync(
				logActivityUrl,
				username: username,
				userId: userId,
				operationId: operation,
				processInstanceId: customObj.ProcessInstanceId,
				commentId: customObj.CommentId,
				letterId: customObj.LetterId,
				recordId: customObj.RecordId,
				actionName: actionName,
				controllerName: controllerName,
				description: description,
				additionalInfo: customObj.AdditionalInfo,
				ipAddress: ipAddress,
				userAgent: userAgent,
				sessionId: sessionId,
				deviceInfo: deviceInfo);
		}

		private async Task<CustomObjectForLogActivity> ResolveCustomObj(string controllerName, IDictionary<string, object> parameters)
		{
			CustomObjectForLogActivity retVal = new();

			switch (controllerName)
			{
				default:
					retVal.AdditionalInfo = JsonConvert.SerializeObject(parameters);
					break;
			}

			return retVal;
		}
		private string ResolveDescriptionString(string descriptionString, IDictionary<string, object> parameters)
		{
			return parameters.Aggregate(descriptionString, (current, argument) => current.Replace($"{{{argument.Key}}}", Convert.ToString(argument.Value)));
		}
		private string ResolveDescriptionString(string descriptionString, CustomObjectForLogActivity customIds)
		{
			return descriptionString
				.Replace("{commentId}", Convert.ToString(customIds.CommentId))
				.Replace("{letterId}", Convert.ToString(customIds.LetterId))
				.Replace("{activityInstanceId}", Convert.ToString(customIds.ActivityInstanceId))
				.Replace("{recordId}", Convert.ToString(customIds.RecordId))
				.Replace("{processInstanceId}", Convert.ToString(customIds.ProcessInstanceId));
		}
	}
}
