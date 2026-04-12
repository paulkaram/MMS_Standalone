namespace MMS.DAL.Enumerations
{
    public enum MeetingStatusDbEnum
    {
        Draft = 1,
        PendingMeetingApproval = 2,
        Approved = 3,
		Started = 4,
		Finished = 5,
  		PendingInitialMeetingMinutesApproval = 6,
		InitialMeetingMinutesApproved = 7,
		PendingFinalMeetingMinutesSign = 8,
		FinalMeetingMinutesSigned = 9,
        Canceled = 10,
    }
}
