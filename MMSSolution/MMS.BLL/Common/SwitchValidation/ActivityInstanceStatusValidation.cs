namespace MMS.BLL.Common.SwitchValidation
{
	public class ActivityInstanceStatusValidation
	{

		public static string GetName(int statusId)
		{
			return statusId switch
			{
				1 => "Active",
				2 => "Waiting",
				3 => "Completed",
				4 => "Canceled",
				7 => "Deleted",
				8 => "Error",
				_ => "Stopped",
			};
		}
	}
}
