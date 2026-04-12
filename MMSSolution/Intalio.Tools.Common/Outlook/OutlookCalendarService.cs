using Intalio.Tools.Common.Smtp;

namespace Intalio.Tools.Common.Outlook
{
    /// <summary>
    /// Service for sending Outlook calendar invitations via SMTP with iCalendar
    /// </summary>
    public class OutlookCalendarService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly OutlookIntegrationSettings _outlookSettings;
        private readonly string _domain;

        public OutlookCalendarService(
            SmtpSettings smtpSettings,
            OutlookIntegrationSettings outlookSettings,
            string domain = "mms.local")
        {
            _smtpSettings = smtpSettings;
            _outlookSettings = outlookSettings;
            _domain = domain;
        }

        /// <summary>
        /// Checks if Outlook integration is enabled and configured
        /// </summary>
        public bool IsEnabled =>
            _outlookSettings.Enabled &&
            !string.IsNullOrEmpty(_smtpSettings.Host);

        /// <summary>
        /// Sends a meeting invitation to all attendees
        /// </summary>
        public void SendMeetingInvitation(
            int meetingId,
            string meetingTitle,
            string meetingDescription,
            DateTime startTime,
            DateTime endTime,
            string location,
            string organizerEmail,
            string organizerName,
            List<AttendeeInfo> attendees,
            int sequence = 0,
            string? teamsJoinUrl = null,
            string? teamsMeetingId = null,
            string? teamsPasscode = null)
        {
            if (!IsEnabled || !_outlookSettings.SendInviteOnApproval)
                return;

            var uid = ICalendarGenerator.GenerateUid(meetingId, _domain);
            var attendeeEmails = attendees.Select(a => a.Email).Where(e => !string.IsNullOrEmpty(e)).ToList();

            var icsContent = ICalendarGenerator.CreateMeetingInvite(
                uid,
                meetingTitle,
                meetingDescription,
                startTime,
                endTime,
                location,
                organizerEmail,
                organizerName,
                attendeeEmails,
                sequence);

            var emailBody = GenerateInviteEmailBody(
                meetingTitle,
                startTime,
                endTime,
                location,
                organizerName,
                meetingDescription,
                teamsJoinUrl,
                teamsMeetingId,
                teamsPasscode);

            var mailService = new MailService(
                _smtpSettings.Host,
                _smtpSettings.Port,
                _smtpSettings.FromEmail ?? _smtpSettings.User,
                _smtpSettings.User,
                _smtpSettings.Password,
                _smtpSettings.EnableSSL,
                false);

            mailService.SendCalendarInviteAsync(
                $"Meeting Invitation: {meetingTitle}",
                emailBody,
                icsContent,
                "REQUEST",
                attendeeEmails.ToArray());
        }

        /// <summary>
        /// Sends a meeting update to all attendees
        /// </summary>
        public void SendMeetingUpdate(
            int meetingId,
            string meetingTitle,
            string meetingDescription,
            DateTime startTime,
            DateTime endTime,
            string location,
            string organizerEmail,
            string organizerName,
            List<AttendeeInfo> attendees,
            int sequence)
        {
            if (!IsEnabled || !_outlookSettings.SendUpdateOnChange)
                return;

            var uid = ICalendarGenerator.GenerateUid(meetingId, _domain);
            var attendeeEmails = attendees.Select(a => a.Email).Where(e => !string.IsNullOrEmpty(e)).ToList();

            var icsContent = ICalendarGenerator.CreateMeetingUpdate(
                uid,
                meetingTitle,
                meetingDescription,
                startTime,
                endTime,
                location,
                organizerEmail,
                organizerName,
                attendeeEmails,
                sequence);

            var emailBody = GenerateUpdateEmailBody(
                meetingTitle,
                startTime,
                endTime,
                location,
                organizerName,
                meetingDescription);

            var mailService = new MailService(
                _smtpSettings.Host,
                _smtpSettings.Port,
                _smtpSettings.FromEmail ?? _smtpSettings.User,
                _smtpSettings.User,
                _smtpSettings.Password,
                _smtpSettings.EnableSSL,
                false);

            mailService.SendCalendarInviteAsync(
                $"Meeting Updated: {meetingTitle}",
                emailBody,
                icsContent,
                "REQUEST",
                attendeeEmails.ToArray());
        }

        /// <summary>
        /// Sends a meeting cancellation to all attendees
        /// </summary>
        public void SendMeetingCancellation(
            int meetingId,
            string meetingTitle,
            DateTime startTime,
            DateTime endTime,
            string organizerEmail,
            string organizerName,
            List<AttendeeInfo> attendees,
            int sequence)
        {
            if (!IsEnabled || !_outlookSettings.SendCancellationOnCancel)
                return;

            var uid = ICalendarGenerator.GenerateUid(meetingId, _domain);
            var attendeeEmails = attendees.Select(a => a.Email).Where(e => !string.IsNullOrEmpty(e)).ToList();

            var icsContent = ICalendarGenerator.CreateMeetingCancellation(
                uid,
                meetingTitle,
                startTime,
                endTime,
                organizerEmail,
                organizerName,
                attendeeEmails,
                sequence);

            var emailBody = GenerateCancellationEmailBody(
                meetingTitle,
                startTime,
                organizerName);

            var mailService = new MailService(
                _smtpSettings.Host,
                _smtpSettings.Port,
                _smtpSettings.FromEmail ?? _smtpSettings.User,
                _smtpSettings.User,
                _smtpSettings.Password,
                _smtpSettings.EnableSSL,
                false);

            mailService.SendCalendarInviteAsync(
                $"Meeting Cancelled: {meetingTitle}",
                emailBody,
                icsContent,
                "CANCEL",
                attendeeEmails.ToArray());
        }

        private string GenerateInviteEmailBody(
            string title,
            DateTime startTime,
            DateTime endTime,
            string location,
            string organizerName,
            string description,
            string? teamsJoinUrl = null,
            string? teamsMeetingId = null,
            string? teamsPasscode = null)
        {
            var teamsSection = "";
            if (!string.IsNullOrEmpty(teamsJoinUrl))
            {
                teamsSection = $@"
            <div style='margin: 20px 0; padding: 15px; background: #f3f2f1; border-radius: 8px; border-right: 4px solid #6264a7;'>
                <div style='display: flex; align-items: center; margin-bottom: 10px;'>
                    <span style='font-size: 18px; font-weight: bold; color: #6264a7;'>📹 Microsoft Teams</span>
                </div>
                <a href='{teamsJoinUrl}' style='display: inline-block; background: #6264a7; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold; margin: 10px 0;'>
                    انضم إلى الاجتماع
                </a>
                {(string.IsNullOrEmpty(teamsMeetingId) ? "" : $"<div style='margin-top: 10px; font-size: 12px; color: #666;'>Meeting ID: {teamsMeetingId}</div>")}
                {(string.IsNullOrEmpty(teamsPasscode) ? "" : $"<div style='font-size: 12px; color: #666;'>Passcode: {teamsPasscode}</div>")}
            </div>";
            }

            return $@"
<!DOCTYPE html>
<html dir='rtl'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Arial, sans-serif; direction: rtl; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #0078d4; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f5f5f5; padding: 20px; border-radius: 0 0 8px 8px; }}
        .detail {{ margin: 10px 0; }}
        .label {{ font-weight: bold; color: #333; }}
        .footer {{ margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>دعوة اجتماع</h2>
        </div>
        <div class='content'>
            <h3>{title}</h3>
            <div class='detail'><span class='label'>التاريخ:</span> {startTime:yyyy-MM-dd}</div>
            <div class='detail'><span class='label'>الوقت:</span> {startTime:HH:mm} - {endTime:HH:mm}</div>
            <div class='detail'><span class='label'>المكان:</span> {(string.IsNullOrEmpty(location) ? "غير محدد" : location)}</div>
            <div class='detail'><span class='label'>المنظم:</span> {organizerName}</div>
            {(string.IsNullOrEmpty(description) ? "" : $"<div class='detail'><span class='label'>الوصف:</span> {description}</div>")}
            {teamsSection}
            <div class='footer'>
                يرجى الرد على هذه الدعوة من خلال Outlook.
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateUpdateEmailBody(
            string title,
            DateTime startTime,
            DateTime endTime,
            string location,
            string organizerName,
            string description)
        {
            return $@"
<!DOCTYPE html>
<html dir='rtl'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Arial, sans-serif; direction: rtl; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #ff8c00; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f5f5f5; padding: 20px; border-radius: 0 0 8px 8px; }}
        .detail {{ margin: 10px 0; }}
        .label {{ font-weight: bold; color: #333; }}
        .footer {{ margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>تحديث اجتماع</h2>
        </div>
        <div class='content'>
            <h3>{title}</h3>
            <p><strong>تم تحديث تفاصيل هذا الاجتماع.</strong></p>
            <div class='detail'><span class='label'>التاريخ:</span> {startTime:yyyy-MM-dd}</div>
            <div class='detail'><span class='label'>الوقت:</span> {startTime:HH:mm} - {endTime:HH:mm}</div>
            <div class='detail'><span class='label'>المكان:</span> {(string.IsNullOrEmpty(location) ? "غير محدد" : location)}</div>
            <div class='detail'><span class='label'>المنظم:</span> {organizerName}</div>
            <div class='footer'>
                يرجى مراجعة التفاصيل المحدثة في Outlook.
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateCancellationEmailBody(
            string title,
            DateTime startTime,
            string organizerName)
        {
            return $@"
<!DOCTYPE html>
<html dir='rtl'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Arial, sans-serif; direction: rtl; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #d32f2f; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f5f5f5; padding: 20px; border-radius: 0 0 8px 8px; }}
        .detail {{ margin: 10px 0; }}
        .label {{ font-weight: bold; color: #333; }}
        .footer {{ margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>إلغاء اجتماع</h2>
        </div>
        <div class='content'>
            <h3>{title}</h3>
            <p><strong>تم إلغاء هذا الاجتماع.</strong></p>
            <div class='detail'><span class='label'>التاريخ الأصلي:</span> {startTime:yyyy-MM-dd HH:mm}</div>
            <div class='detail'><span class='label'>المنظم:</span> {organizerName}</div>
            <div class='footer'>
                سيتم إزالة هذا الاجتماع من تقويمك تلقائياً.
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }

    /// <summary>
    /// Attendee information for calendar invites
    /// </summary>
    public class AttendeeInfo
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = true;
    }
}
