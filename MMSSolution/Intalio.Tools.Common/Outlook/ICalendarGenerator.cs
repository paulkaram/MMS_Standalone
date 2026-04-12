using System.Text;

namespace Intalio.Tools.Common.Outlook
{
    /// <summary>
    /// Generates iCalendar (.ics) content for Outlook calendar integration
    /// </summary>
    public static class ICalendarGenerator
    {
        /// <summary>
        /// Creates an iCalendar event for a meeting invitation
        /// </summary>
        public static string CreateMeetingInvite(
            string uid,
            string summary,
            string description,
            DateTime startTime,
            DateTime endTime,
            string location,
            string organizerEmail,
            string organizerName,
            List<string> attendeeEmails,
            int sequence = 0)
        {
            return CreateEvent(
                uid,
                summary,
                description,
                startTime,
                endTime,
                location,
                organizerEmail,
                organizerName,
                attendeeEmails,
                "REQUEST",
                sequence);
        }

        /// <summary>
        /// Creates an iCalendar update for an existing meeting
        /// </summary>
        public static string CreateMeetingUpdate(
            string uid,
            string summary,
            string description,
            DateTime startTime,
            DateTime endTime,
            string location,
            string organizerEmail,
            string organizerName,
            List<string> attendeeEmails,
            int sequence)
        {
            return CreateEvent(
                uid,
                summary,
                description,
                startTime,
                endTime,
                location,
                organizerEmail,
                organizerName,
                attendeeEmails,
                "REQUEST",
                sequence);
        }

        /// <summary>
        /// Creates an iCalendar cancellation for a meeting
        /// </summary>
        public static string CreateMeetingCancellation(
            string uid,
            string summary,
            DateTime startTime,
            DateTime endTime,
            string organizerEmail,
            string organizerName,
            List<string> attendeeEmails,
            int sequence)
        {
            return CreateEvent(
                uid,
                summary,
                "This meeting has been cancelled.",
                startTime,
                endTime,
                string.Empty,
                organizerEmail,
                organizerName,
                attendeeEmails,
                "CANCEL",
                sequence,
                "CANCELLED");
        }

        private static string CreateEvent(
            string uid,
            string summary,
            string description,
            DateTime startTime,
            DateTime endTime,
            string location,
            string organizerEmail,
            string organizerName,
            List<string> attendeeEmails,
            string method,
            int sequence,
            string status = "CONFIRMED")
        {
            var sb = new StringBuilder();

            // iCalendar header
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:-//MMS Meeting Management System//EN");
            sb.AppendLine("CALSCALE:GREGORIAN");
            sb.AppendLine($"METHOD:{method}");

            // Event
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{uid}");
            sb.AppendLine($"DTSTAMP:{DateTime.Now:yyyyMMddTHHmmssZ}");
            sb.AppendLine($"DTSTART:{startTime.ToUniversalTime():yyyyMMddTHHmmssZ}");
            sb.AppendLine($"DTEND:{endTime.ToUniversalTime():yyyyMMddTHHmmssZ}");
            sb.AppendLine($"SUMMARY:{EscapeText(summary)}");

            if (!string.IsNullOrEmpty(description))
            {
                sb.AppendLine($"DESCRIPTION:{EscapeText(description)}");
            }

            if (!string.IsNullOrEmpty(location))
            {
                sb.AppendLine($"LOCATION:{EscapeText(location)}");
            }

            sb.AppendLine($"STATUS:{status}");
            sb.AppendLine($"SEQUENCE:{sequence}");

            // Organizer
            sb.AppendLine($"ORGANIZER;CN={EscapeText(organizerName)}:mailto:{organizerEmail}");

            // Attendees
            foreach (var attendeeEmail in attendeeEmails)
            {
                if (!string.IsNullOrEmpty(attendeeEmail))
                {
                    sb.AppendLine($"ATTENDEE;CUTYPE=INDIVIDUAL;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE:mailto:{attendeeEmail}");
                }
            }

            // Reminder (15 minutes before)
            sb.AppendLine("BEGIN:VALARM");
            sb.AppendLine("TRIGGER:-PT15M");
            sb.AppendLine("ACTION:DISPLAY");
            sb.AppendLine("DESCRIPTION:Meeting reminder");
            sb.AppendLine("END:VALARM");

            sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");

            return sb.ToString();
        }

        /// <summary>
        /// Escapes special characters for iCalendar format
        /// </summary>
        private static string EscapeText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text
                .Replace("\\", "\\\\")
                .Replace(";", "\\;")
                .Replace(",", "\\,")
                .Replace("\r\n", "\\n")
                .Replace("\n", "\\n")
                .Replace("\r", "\\n");
        }

        /// <summary>
        /// Generates a unique UID for a meeting based on meeting ID
        /// </summary>
        public static string GenerateUid(int meetingId, string domain = "mms.local")
        {
            return $"meeting-{meetingId}@{domain}";
        }
    }
}
