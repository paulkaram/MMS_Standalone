namespace Intalio.Tools.Common.Outlook
{
    public class OutlookIntegrationSettings
    {
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Integration mode: SMTP, Graph, or Both
        /// </summary>
        public string Mode { get; set; } = "SMTP";

        /// <summary>
        /// Auto-send calendar invite when meeting is approved
        /// </summary>
        public bool SendInviteOnApproval { get; set; } = true;

        /// <summary>
        /// Auto-send update when meeting is modified
        /// </summary>
        public bool SendUpdateOnChange { get; set; } = true;

        /// <summary>
        /// Auto-send cancellation when meeting is cancelled
        /// </summary>
        public bool SendCancellationOnCancel { get; set; } = true;

        /// <summary>
        /// Microsoft Graph API settings
        /// </summary>
        public GraphSettings Graph { get; set; } = new();
    }

    public class GraphSettings
    {
        public bool Enabled { get; set; } = false;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
    }
}
