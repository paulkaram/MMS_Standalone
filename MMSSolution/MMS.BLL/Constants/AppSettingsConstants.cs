namespace MMS.BLL.Constants
{
    public static class AppSettingsConstants
    {
        public const string JwtSectionName = "Jwt";
        public const string LdapSectionName = "Ldap";
        public const string SmtpSectionName = "Smtp";
        public const string SmsSectionName = "Sms";
        public const string StorageSectionName = "Storage";
        public const string FBAPassword = "FBAPassword";
        public const string ActivityLogUrl = "ActivityLogUrl";
        public const string TotalCountForAutoComplete = "TotalCountForAutoComplete";
        public const string EnableEmailNotification = "EnableEmailNotification";
        public const string EnableSmsNotification = "EnableSmsNotification";
        public const string EnableExternalLogin = "EnableExternalLogin";//one of both must be true
        public const string EnableLogin = "EnableLogin";//one of both must be true
		public const string ApplicationName = "ApplicationName";
        public const string UserGuideStorePath = "UserGuideStorePath";
        public const string Enable2FA = "Enable2FA";
        public const string MeetingMinutesTemplatePath = "MeetingMinutesTemplatePath";
        public const string MaxCommitteesForUser = "MaxCommitteesForUser";
		public const string MoiJwtSectionName = "MoiJwt";
		public const string ExternalTokenName = "ExternalTokenName";
		public const string PincodeSectionName = "Pincode";
		public const string OidcSectionName = "Oidc";
        public const string NotificationSectionName= "Notification";
		/// <summary>
		/// NCA Compliance: Encryption settings section name (NCS-1:2020 Section 8).
		/// Keys should be configured via environment variables or secure vault.
		/// </summary>
		public const string EncryptionSectionName = "Encryption";

		/// <summary>
		/// Outlook Integration settings for calendar sync and email invites.
		/// </summary>
		public const string OutlookIntegrationSectionName = "OutlookIntegration";

		/// <summary>
		/// Microsoft Teams Integration settings for online meetings.
		/// </summary>
		public const string TeamsIntegrationSectionName = "TeamsIntegration";

		// AI Service
		public const string AiServiceBaseUrl = "AiService:BaseUrl";
		public const string AiServiceApiKey = "AiService:ApiKey";
		public const string AiTranscriptSummaryPromptEN = "AiService:TranscriptSummaryPromptEN";
		public const string AiTranscriptSummaryPromptAR = "AiService:TranscriptSummaryPromptAR";
		public const string AiSummaryMaxTokens = "AiService:SummaryMaxTokens";
		public const string AiSummaryTemperature = "AiService:SummaryTemperature";

    }

    public static class EmailTemplateNames
	{
        public const string NewTask = "NewTask";
        public const string MeetingInvitation = "MeetingInvitation";
        public const string InitialMeetingMinutes = "InitialMeetingMinutes";
        public const string FinalMeetingMinutes = "FinalMeetingMinutes";
        public const string MeetingRequest = "MeetingRequest";
    }
}
