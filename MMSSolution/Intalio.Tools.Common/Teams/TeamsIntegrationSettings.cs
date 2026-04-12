namespace Intalio.Tools.Common.Teams
{
    /// <summary>
    /// Settings for Microsoft Teams integration
    /// </summary>
    public class TeamsIntegrationSettings
    {
        /// <summary>
        /// Whether Teams integration is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Azure AD Tenant ID
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// Azure AD Application (Client) ID
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Azure AD Client Secret
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Default organizer email for Teams meetings (must be a licensed user)
        /// </summary>
        public string OrganizerEmail { get; set; } = string.Empty;

        /// <summary>
        /// Automatically create Teams meeting when meeting is marked as online
        /// </summary>
        public bool AutoCreateOnApproval { get; set; } = true;
    }
}
