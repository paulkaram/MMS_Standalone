using DocumentFormat.OpenXml.Drawing;
using Intalio.Tools.Common.Outlook;
using Intalio.Tools.Common.Smtp;
using Microsoft.Extensions.Configuration;
using MMS.BLL.Constants;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO.AppSettings;
using MMS.DTO.Meetings;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Task = System.Threading.Tasks.Task;
namespace MMS.BLL.Managers
{
    public class EmailNotificationManager
    {
        private readonly IProcessUnitOfWork _processUnitOfWork;
        private readonly ISettingsUnitOfWork _settingsUnitOfWork;
        private readonly IUserManagementUnitOfWork _userManagementUnitOfWork;
        private readonly SmtpSettings _smtpSettings;
        private readonly NotificationSettingsDto _notificationSettings;
        private readonly OutlookIntegrationSettings _outlookSettings;
        private readonly OutlookCalendarService _outlookCalendarService;

        private readonly bool EmailNotificationEnabled;

        public EmailNotificationManager(IConfiguration configuration, ISettingsUnitOfWork settingsUnitOfWork, IUserManagementUnitOfWork userManagementUnitOfWork, IProcessUnitOfWork processUnitOfWork)
        {
            _processUnitOfWork = processUnitOfWork;
            _settingsUnitOfWork = settingsUnitOfWork;
            _smtpSettings = configuration.GetSection(AppSettingsConstants.SmtpSectionName).Get<SmtpSettings>() ?? new();
            _notificationSettings = configuration.GetSection(AppSettingsConstants.NotificationSectionName).Get<NotificationSettingsDto>() ?? new();
            _outlookSettings = configuration.GetSection(AppSettingsConstants.OutlookIntegrationSectionName).Get<OutlookIntegrationSettings>() ?? new();
            _outlookCalendarService = new OutlookCalendarService(_smtpSettings, _outlookSettings);

            _userManagementUnitOfWork = userManagementUnitOfWork;
            EmailNotificationEnabled = configuration.GetValue<bool>(AppSettingsConstants.EnableEmailNotification);
        }

        public async Task SendNewTasksNotificationByCompletedActivityId(int completedActivityInstanceId, LanguageDbEnum language)
        {
            // CaseManagement dependency removed - this method previously used _caseUnitOfWork to resolve activity/workflow instances
            await Task.CompletedTask;
        }

        public async Task SendMeetingRequest(MeetingInvitationMailDto meeting, List<MeetingAttendee> meetingAttendee, LanguageDbEnum language)
        {
            if (EmailNotificationEnabled)
            {
                var template = await _settingsUnitOfWork.EmailTemplates.GetAsync(x => x.Name == EmailTemplateNames.MeetingRequest);
                bool isArabic = language == LanguageDbEnum.Arabic ? true : false;

                if (template != null && !string.IsNullOrEmpty(_smtpSettings.Host))
                {
                    var emailWrapper = new MailService(_smtpSettings.Host, _smtpSettings.Port, _smtpSettings.User, _smtpSettings.User, _smtpSettings.Password, _smtpSettings.EnableSSL, true);
                    List<string> emails = new();
                    List<User> users = new();

                    var attendeesHtml = new StringBuilder();
                    int attendeeIndex = 1;
                    foreach (var attendee in meetingAttendee)
                    {
                        var attendeeName = isArabic
                            ? (attendee.User.FullnameAr ?? attendee.User.FullnameEn)
                            : (attendee.User.FullnameEn ?? attendee.User.FullnameAr);
                        var attendanceStatus = attendee.Attended
                            ? (isArabic ? "مؤكد" : "Confirmed")
                            : (isArabic ? "بإنتظار تأكيد الحضور" : "Pending Confirmation");
                        attendeesHtml.AppendLine($@"
                       <tr>
                           <td style=""width:5%;"" class=""tdIndex"">{attendeeIndex++}</td>
                           <td style=""width:30%;"">{attendeeName}</td>
                           <td>{attendee.JobTitle}</td>
                           <td style=""width:15%;"">{attendanceStatus}</td>
                       </tr>");
                    }
                    var agendasHtml = new StringBuilder();
                    int agendaIndex = 1;
                    foreach (var agenda in meeting.MeetingAgenda)
                    {
                        agendasHtml.AppendLine($@"
                       <tr>
                           <td style=""width:5%;"" class=""tdIndex"">{agendaIndex++}</td>
                           <td style=""width:80%;"" colspan=""2"">{agenda.Title}</td>
                           <td style=""width:15%;"">{(agenda.Duration.ToString())}</td>
                       </tr>");
                    }
                    DateTime start = DateTime.Parse(meeting.StartTime);
                    DateTime end = DateTime.Parse(meeting.EndTime);
                    TimeSpan duration = end - start;
                    var meetingDuration = duration.ToString(@"hh\:mm");
                    string emailBody = template.Body
                                      .Replace("{meetingSubject}", meeting.Title)
                                      .Replace("{meetingLocation}", meeting.Location)
                                      .Replace("{meetingHolder}", meeting.CreatedByName)
                                      .Replace("{meetingAgenda}", agendasHtml.ToString())//meeting.Agenda)
                                      .Replace("{presenterName}", "")//meeting.PresenterName)
                                      .Replace("{momWriter}", "")//meeting.MomWriter)
                                      .Replace("{meetingDuration}", meetingDuration)
                                      .Replace("{secretaryName}", meeting.CreatedByName)
                                      .Replace("{secretaryContact}", "")//meeting.SecretaryContact)
                                      .Replace("{protocolName}", "")//meeting.ProtocolName)
                                      .Replace("{protocolContact}", "")//meeting.ProtocolContact)
                                      .Replace("{attendeesRows}", attendeesHtml.ToString());

                    string icsContent = $@"BEGIN:VCALENDAR
                                           METHOD:REQUEST
                                           PRODID:-//YourApp//Meeting Scheduler//EN
                                           VERSION:2.0
                                           BEGIN:VEVENT
                                           UID:{Guid.NewGuid()}
                                           DTSTAMP:{DateTime.Now:yyyyMMddTHHmmssZ}
                                           DTSTART:{DateTime.Parse(meeting.StartTime).ToUniversalTime():yyyyMMddTHHmmssZ}
                                           DTEND:{DateTime.Parse(meeting.EndTime).ToUniversalTime():yyyyMMddTHHmmssZ}
                                           SUMMARY:{meeting.Title}
                                           DESCRIPTION:{meeting.Notes}
                                           LOCATION:{meeting.Location}
                                           ORGANIZER;CN=""{meeting.CreatedByName}"":mailto:organizer@yourdomain.com
                                           ATTENDEE;CN=""Attendee Name"";RSVP=TRUE:mailto:attendee@yourdomain.com
                                           SEQUENCE:0
                                           STATUS:CONFIRMED
                                           BEGIN:VALARM
                                           TRIGGER:-PT15M
                                           ACTION:DISPLAY
                                           DESCRIPTION:Reminder
                                           END:VALARM
                                           END:VEVENT
                                           END:VCALENDAR";

                    var calendarView = AlternateView.CreateAlternateViewFromString(
                        icsContent,
                        new ContentType("text/calendar; method=REQUEST; charset=UTF-8")
                    );


                    byte[] bytes = Encoding.UTF8.GetBytes(icsContent);
                    var calendarAttachment = new System.Net.Mail.Attachment(new MemoryStream(bytes), "meeting.ics", "text/calendar");
                    List<System.Net.Mail.Attachment> attachments = [calendarAttachment];

                    emailWrapper.SendEmailWithAttachmentAsync(template.Subject, emailBody, attachments, isArabic, meeting.AttendeesEmails.ToArray());
                }
            }
        }

        public async Task SendMeetingInvitation(MeetingInvitationMailDto meeting, LanguageDbEnum language)
        {
            if (EmailNotificationEnabled)
            {
                var template = await _settingsUnitOfWork.EmailTemplates.GetAsync(x => x.Name == EmailTemplateNames.MeetingInvitation);
                bool isArabic = language == LanguageDbEnum.Arabic ? true : false;
                if (template != null && !string.IsNullOrEmpty(_smtpSettings.Host))
                {
                    var emailWrapper = new MailService(_smtpSettings.Host, _smtpSettings.Port, _smtpSettings.User, _smtpSettings.User, _smtpSettings.Password, _smtpSettings.EnableSSL, true);
                    List<string> emails = new();
                    List<User> users = new();
                    string emailBody = template.Body.Replace("{createdByName}", meeting.CreatedByName)
                        .Replace("{meetingSubject}", meeting.Title)
                        .Replace("{ReferenceNumber}", meeting.ReferenceNumber)
                        .Replace("{meetingNotes}", meeting.Notes)
                        .Replace("{startTime}", DateTime.TryParse(meeting.StartTime, out var start) ? start.ToString("hh:mm tt") : meeting.StartTime)
                        .Replace("{endTime}", DateTime.TryParse(meeting.EndTime, out var end) ? end.ToString("hh:mm tt") : meeting.EndTime)
                        .Replace("{meetingDate}", meeting.Date.ToString("MMM dd, yyyy"))
                        .Replace("{taskUrl}", _notificationSettings.MeetingTaskUrl)
                        .Replace("{meetingHall}", meeting.Location.ToString());

                    string icsContent = @"BEGIN:VCALENDAR
                                            VERSION:2.0
                                            PRODID:-//YourApp//NONSGML v1.0//EN
                                            BEGIN:VEVENT
                                            UID:" + Guid.NewGuid() + @"
                                            DTSTAMP:" + meeting.Date.ToString("yyyyMMddTHHmmssZ") + @"
                                            DTSTART:" + meeting.StartTime + @"
                                            DTEND:" + meeting.EndTime + @"
                                            SUMMARY:" + meeting.Title + @"
                                            DESCRIPTION:" + meeting.Notes + @"
                                            ORGANIZER;CN=" + meeting.CreatedByName + @":mailto:organizer@intalio.com
                                            ATTENDEE;CN=Paul Karam;RSVP=TRUE:mailto:attendee@intalio.com
                                            END:VEVENT
                                            END:VCALENDAR";

                    byte[] bytes = Encoding.UTF8.GetBytes(icsContent);
                    var calendarAttachment = new System.Net.Mail.Attachment(new MemoryStream(bytes), "meeting.ics", "text/calendar");
                    List<System.Net.Mail.Attachment> attachments = [calendarAttachment];

                    emailWrapper.SendEmailWithAttachmentAsync(template.Subject, emailBody, attachments, isArabic, meeting.AttendeesEmails.ToArray());
                }
            }
        }

        public async Task InitialMeetingMinutesApprovalEmail(Meeting? meeting, List<string> usersIds)
        {
            if (EmailNotificationEnabled)
            {
                var template = await _settingsUnitOfWork.EmailTemplates.GetAsync(x => x.Name == EmailTemplateNames.InitialMeetingMinutes);
                if (template != null && !string.IsNullOrEmpty(_smtpSettings.Host))
                {

                    var emailWrapper = new MailService(_smtpSettings.Host, _smtpSettings.Port, _smtpSettings.User, _smtpSettings.User, _smtpSettings.Password, _smtpSettings.EnableSSL, true);

                    List<string> emails = new();
                    List<User> users = new();

                    string emailBody = template.Body.Replace("{createdByName}", meeting.CreatedbyNavigation.FullnameAr)
                        .Replace("{meetingSubject}", meeting.Title)
                        .Replace("{ReferenceNumber}", meeting.ReferenceNumber)
                        .Replace("{meetingNotes}", meeting.Notes)
                        .Replace("{startTime}", meeting.StartTime.ToString())
                        .Replace("{endTime}", meeting.EndTime.ToString())
                        .Replace("{meetingDate}", meeting.CreatedDate.ToString())
                        .Replace("{taskUrl}", _notificationSettings.MeetingTaskUrl)
                        .Replace("{meetingHall}", meeting.Location.ToString());
                    string icsContent = @"BEGIN:VCALENDAR
                            VERSION:2.0
                            PRODID:-//YourApp//NONSGML v1.0//EN
                            BEGIN:VEVENT
                            UID:" + Guid.NewGuid() + @"
                            DTSTAMP:" + meeting.CreatedDate?.ToString("yyyyMMddTHHmmssZ") + @"
                            DTSTART:" + meeting.StartTime + @"
                            DTEND:" + meeting.EndTime + @"
                            SUMMARY:" + meeting.Title + @"
                            DESCRIPTION:" + meeting.Notes + @"
                            ORGANIZER;CN=" + meeting.CreatedbyNavigation.FullnameAr + @":mailto:organizer@intalio.com
                            ATTENDEE;CN=Paul Karam;RSVP=TRUE:mailto:attendee@intalio.com
                            END:VEVENT
                            END:VCALENDAR";

                    byte[] bytes = Encoding.UTF8.GetBytes(icsContent);
                    var calendarAttachment = new System.Net.Mail.Attachment(new MemoryStream(bytes), "meeting.ics", "text/calendar");
                    List<System.Net.Mail.Attachment> attachments = [calendarAttachment];
                    emailWrapper.SendEmailWithAttachmentAsync(template.Subject, emailBody, attachments, true, meeting.MeetingAttendees.Where(x => usersIds.Contains(x.UserId)).Select(x => x.User.Email).ToArray());
                }
            }
        }
        public async Task FinalMeetingMinutesApprovalEmail(Meeting? meeting, List<string> usersIds)
        {
            if (EmailNotificationEnabled)
            {
                var template = await _settingsUnitOfWork.EmailTemplates.GetAsync(x => x.Name == EmailTemplateNames.FinalMeetingMinutes);
                if (template != null && !string.IsNullOrEmpty(_smtpSettings.Host))
                {

                    var emailWrapper = new MailService(_smtpSettings.Host, _smtpSettings.Port, _smtpSettings.User, _smtpSettings.User, _smtpSettings.Password, _smtpSettings.EnableSSL, true);

                    List<string> emails = new();
                    List<User> users = new();

                    string emailBody = template.Body.Replace("{createdByName}", meeting.CreatedbyNavigation.FullnameAr)
                        .Replace("{meetingSubject}", meeting.Title)
                        .Replace("{ReferenceNumber}", meeting.ReferenceNumber)
                        .Replace("{meetingNotes}", meeting.Notes)
                        .Replace("{startTime}", meeting.StartTime.ToString())
                        .Replace("{endTime}", meeting.EndTime.ToString())
                        .Replace("{meetingDate}", meeting.CreatedDate.ToString())
                        .Replace("{taskUrl}", _notificationSettings.MeetingTaskUrl)
                        .Replace("{meetingHall}", meeting.Location.ToString());
                    string icsContent = @"BEGIN:VCALENDAR
                            VERSION:2.0
                            PRODID:-//YourApp//NONSGML v1.0//EN
                            BEGIN:VEVENT
                            UID:" + Guid.NewGuid() + @"
                            DTSTAMP:" + meeting.CreatedDate?.ToString("yyyyMMddTHHmmssZ") + @"
                            DTSTART:" + meeting.StartTime + @"
                            DTEND:" + meeting.EndTime + @"
                            SUMMARY:" + meeting.Title + @"
                            DESCRIPTION:" + meeting.Notes + @"
                            ORGANIZER;CN=" + meeting.CreatedbyNavigation.FullnameAr + @":mailto:organizer@intalio.com
                            ATTENDEE;CN=Paul Karam;RSVP=TRUE:mailto:attendee@intalio.com
                            END:VEVENT
                            END:VCALENDAR";

                    byte[] bytes = Encoding.UTF8.GetBytes(icsContent);
                    var calendarAttachment = new System.Net.Mail.Attachment(new MemoryStream(bytes), "meeting.ics", "text/calendar");
                    List<System.Net.Mail.Attachment> attachments = [calendarAttachment];
                    emailWrapper.SendEmailWithAttachmentAsync(template.Subject, emailBody, attachments, true, meeting.MeetingAttendees.Where(x => usersIds.Contains(x.UserId)).Select(x => x.User.Email).ToArray());
                }
            }
        }

        /// <summary>
        /// Sends Outlook calendar invite when meeting is approved
        /// </summary>
        public void SendMeetingCalendarInvite(Meeting meeting)
        {
            if (!_outlookCalendarService.IsEnabled)
                return;

            var attendees = meeting.MeetingAttendees
                .Where(a => a.User != null && !string.IsNullOrEmpty(a.User.Email))
                .Select(a => new AttendeeInfo
                {
                    Email = a.User.Email,
                    Name = a.User.FullnameAr ?? a.User.FullnameEn ?? a.User.Email,
                    IsRequired = true
                })
                .ToList();

            if (!attendees.Any())
                return;

            var organizer = meeting.CreatedbyNavigation;
            var organizerEmail = organizer?.Email ?? _smtpSettings.FromEmail ?? _smtpSettings.User;
            var organizerName = organizer?.FullnameAr ?? organizer?.FullnameEn ?? "Meeting Organizer";

            var (startTime, endTime) = ParseMeetingTimes(meeting);

            _outlookCalendarService.SendMeetingInvitation(
                meeting.Id,
                meeting.Title ?? "Meeting",
                meeting.Notes ?? string.Empty,
                startTime,
                endTime,
                meeting.Location ?? string.Empty,
                organizerEmail,
                organizerName,
                attendees,
                0,
                meeting.MeetingUrl,
                meeting.OnlineMeetingId,
                meeting.OnlineMeetingPasscode);
        }

        /// <summary>
        /// Sends Outlook calendar update when meeting is modified
        /// </summary>
        public void SendMeetingCalendarUpdate(Meeting meeting, int sequence)
        {
            if (!_outlookCalendarService.IsEnabled)
                return;

            var attendees = meeting.MeetingAttendees
                .Where(a => a.User != null && !string.IsNullOrEmpty(a.User.Email))
                .Select(a => new AttendeeInfo
                {
                    Email = a.User.Email,
                    Name = a.User.FullnameAr ?? a.User.FullnameEn ?? a.User.Email,
                    IsRequired = true
                })
                .ToList();

            if (!attendees.Any())
                return;

            var organizer = meeting.CreatedbyNavigation;
            var organizerEmail = organizer?.Email ?? _smtpSettings.FromEmail ?? _smtpSettings.User;
            var organizerName = organizer?.FullnameAr ?? organizer?.FullnameEn ?? "Meeting Organizer";

            var (startTime, endTime) = ParseMeetingTimes(meeting);

            _outlookCalendarService.SendMeetingUpdate(
                meeting.Id,
                meeting.Title ?? "Meeting",
                meeting.Notes ?? string.Empty,
                startTime,
                endTime,
                meeting.Location ?? string.Empty,
                organizerEmail,
                organizerName,
                attendees,
                sequence);
        }

        /// <summary>
        /// Sends Outlook calendar cancellation when meeting is cancelled
        /// </summary>
        public void SendMeetingCalendarCancellation(Meeting meeting, int sequence)
        {
            if (!_outlookCalendarService.IsEnabled)
                return;

            var attendees = meeting.MeetingAttendees
                .Where(a => a.User != null && !string.IsNullOrEmpty(a.User.Email))
                .Select(a => new AttendeeInfo
                {
                    Email = a.User.Email,
                    Name = a.User.FullnameAr ?? a.User.FullnameEn ?? a.User.Email,
                    IsRequired = true
                })
                .ToList();

            if (!attendees.Any())
                return;

            var organizer = meeting.CreatedbyNavigation;
            var organizerEmail = organizer?.Email ?? _smtpSettings.FromEmail ?? _smtpSettings.User;
            var organizerName = organizer?.FullnameAr ?? organizer?.FullnameEn ?? "Meeting Organizer";

            var (startTime, endTime) = ParseMeetingTimes(meeting);

            _outlookCalendarService.SendMeetingCancellation(
                meeting.Id,
                meeting.Title ?? "Meeting",
                startTime,
                endTime,
                organizerEmail,
                organizerName,
                attendees,
                sequence);
        }

        /// <summary>
        /// Parses meeting start and end times from string format
        /// </summary>
        private (DateTime startTime, DateTime endTime) ParseMeetingTimes(Meeting meeting)
        {
            var meetingDate = meeting.Date;

            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now.AddHours(1);

            if (!string.IsNullOrEmpty(meeting.StartTime) && DateTime.TryParse(meeting.StartTime, out var parsedStart))
            {
                startTime = meetingDate.Date.Add(parsedStart.TimeOfDay);
            }

            if (!string.IsNullOrEmpty(meeting.EndTime) && DateTime.TryParse(meeting.EndTime, out var parsedEnd))
            {
                endTime = meetingDate.Date.Add(parsedEnd.TimeOfDay);
            }

            return (startTime, endTime);
        }
    }
    public class MeetingAttendeeInfo
    {
        public string Name { get; set; }
        public string JobTitleAndInstitution { get; set; }
        public string Status { get; set; }
    }
}
