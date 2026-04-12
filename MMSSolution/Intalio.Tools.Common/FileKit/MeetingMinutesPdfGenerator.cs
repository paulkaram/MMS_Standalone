using Aspose.Pdf;
using MMS.DTO.Meetings;
using MMS.DTO.Settings;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Intalio.Tools.Common.FileKit
{
    /// <summary>
    /// Generates PDF Minutes of Meeting documents using HTML template converted to PDF
    /// This approach provides better RTL support and easier styling
    /// </summary>
    public class MeetingMinutesPdfGenerator
    {
        private readonly MomTemplateConfigDto _config;
        private readonly string? _htmlTemplate;
        private bool _isArabic = true; // Tracks the language for the current generation

        // Arabic culture for date formatting
        private static readonly CultureInfo ArabicCulture = new CultureInfo("ar-SA")
        {
            DateTimeFormat = { Calendar = new GregorianCalendar() }
        };

        private static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo("en-US");

        /// <summary>
        /// Returns the appropriate date culture based on current language context
        /// </summary>
        private CultureInfo DateCulture => _isArabic ? ArabicCulture : EnglishCulture;

        /// <summary>
        /// Creates a new instance with the specified template configuration
        /// </summary>
        public MeetingMinutesPdfGenerator(MomTemplateConfigDto? config = null, string? htmlTemplate = null)
        {
            _config = config ?? GetDefaultConfig();
            _htmlTemplate = htmlTemplate;
        }

        /// <summary>
        /// Static method for backward compatibility - uses default config
        /// </summary>
        public static byte[] GeneratePdf(MeetingMinutesDto minutes, bool includeVoterNames = true)
        {
            var generator = new MeetingMinutesPdfGenerator();
            return generator.Generate(minutes, includeVoterNames);
        }

        /// <summary>
        /// Static method with config - creates instance and generates PDF
        /// </summary>
        public static byte[] GeneratePdf(MeetingMinutesDto minutes, MomTemplateConfigDto? config, bool includeVoterNames = true)
        {
            var generator = new MeetingMinutesPdfGenerator(config);
            return generator.Generate(minutes, includeVoterNames);
        }

        /// <summary>
        /// Static method with config and HTML template
        /// </summary>
        public static byte[] GeneratePdf(MeetingMinutesDto minutes, MomTemplateConfigDto? config, string? htmlTemplate, bool includeVoterNames = true)
        {
            var generator = new MeetingMinutesPdfGenerator(config, htmlTemplate);
            return generator.Generate(minutes, includeVoterNames);
        }

        /// <summary>
        /// Generates a PDF document for meeting minutes using HTML template
        /// </summary>
        public byte[] Generate(MeetingMinutesDto minutes, bool includeVoterNames = true)
        {
            // Set language context for label/role resolution
            _isArabic = string.Equals(minutes.Language, "ar", StringComparison.OrdinalIgnoreCase);

            // Generate HTML content
            var html = GenerateHtml(minutes, includeVoterNames);

            // Convert HTML to PDF using Aspose.PDF
            using var htmlStream = new MemoryStream(Encoding.UTF8.GetBytes(html));
            var options = new HtmlLoadOptions
            {
                PageInfo = new PageInfo
                {
                    // Use A4 Landscape for wider content (842 x 595 points)
                    Width = 842,
                    Height = 595,
                    Margin = new MarginInfo(
                        _config.PageLayout.LeftMargin,
                        _config.PageLayout.BottomMargin,
                        _config.PageLayout.RightMargin,
                        _config.PageLayout.TopMargin
                    )
                }
            };

            var document = new Document(htmlStream, options);

            using var outputStream = new MemoryStream();
            document.Save(outputStream);
            return outputStream.ToArray();
        }

        private string GenerateHtml(MeetingMinutesDto minutes, bool includeVoterNames)
        {
            // If custom HTML template is provided, use it with placeholder replacement
            if (!string.IsNullOrEmpty(_htmlTemplate))
            {
                return ProcessHtmlTemplate(_htmlTemplate, minutes, includeVoterNames);
            }

            // Otherwise, generate default HTML
            return GenerateDefaultHtml(minutes, includeVoterNames);
        }

        private string ProcessHtmlTemplate(string template, MeetingMinutesDto minutes, bool includeVoterNames)
        {
            var html = template;

            // Determine language direction
            var isArabic = string.Equals(minutes.Language, "ar", StringComparison.OrdinalIgnoreCase);
            var dateCulture = isArabic ? ArabicCulture : System.Globalization.CultureInfo.GetCultureInfo("en-US");
            var direction = isArabic ? "rtl" : "ltr";
            var lang = isArabic ? "ar" : "en";
            var textAlign = isArabic ? "right" : "left";
            var textAlignOpp = isArabic ? "left" : "right";

            // Replace direction/language placeholders
            html = html.Replace("{{Direction}}", direction);
            html = html.Replace("{{Lang}}", lang);
            html = html.Replace("{{TextAlign}}", textAlign);
            html = html.Replace("{{TextAlignOpposite}}", textAlignOpp);

            // Replace simple placeholders
            html = html.Replace("{{Title}}", Escape(minutes.Title));
            html = html.Replace("{{OrganizationName}}", Escape(minutes.OrganizationName ?? ""));
            html = html.Replace("{{CommitteeName}}", Escape(minutes.CommitteeName ?? ""));
            html = html.Replace("{{ReferenceNumber}}", Escape(minutes.ReferenceNumber));
            html = html.Replace("{{MeetingNumber}}", minutes.MeetingNumber.ToString());
            html = html.Replace("{{Date}}", minutes.Date.ToString("dddd, d MMMM yyyy", dateCulture));
            html = html.Replace("{{StartTime}}", minutes.StartTime);
            html = html.Replace("{{EndTime}}", minutes.EndTime);
            html = html.Replace("{{Location}}", Escape(minutes.Location ?? GetLabel("notSpecified")));
            html = html.Replace("{{ActualDuration}}", Escape(minutes.ActualDuration ?? GetLabel("notSpecified")));
            html = html.Replace("{{PresentCount}}", minutes.PresentCount.ToString());
            html = html.Replace("{{TotalAttendees}}", minutes.TotalAttendees.ToString());
            html = html.Replace("{{QuorumStatus}}", minutes.QuorumMet ? GetLabel("quorumMet") : GetLabel("quorumNotMet"));
            html = html.Replace("{{CouncilSessionName}}", Escape(minutes.CouncilSessionName ?? ""));
            html = html.Replace("{{ChairmanTitle}}", Escape(minutes.ChairmanTitle));
            html = html.Replace("{{ChairmanName}}", Escape(minutes.ChairmanName ?? ""));
            html = html.Replace("{{SecretaryTitle}}", Escape(minutes.SecretaryTitle));
            html = html.Replace("{{SecretaryName}}", Escape(minutes.SecretaryName ?? ""));
            html = html.Replace("{{MeetingSummary}}", EscapeRtl(minutes.MeetingSummary ?? ""));
            html = html.Replace("{{VersionLabel}}", Escape(minutes.VersionLabel));
            html = html.Replace("{{GeneratedAt}}", minutes.GeneratedAt.ToString("d/M/yyyy HH:mm", dateCulture));

            // Process conditional blocks
            html = ProcessConditionalBlock(html, "OrganizationName", !string.IsNullOrEmpty(minutes.OrganizationName));
            html = ProcessConditionalBlock(html, "CommitteeName", !string.IsNullOrEmpty(minutes.CommitteeName));
            html = ProcessConditionalBlock(html, "CouncilSessionName", !string.IsNullOrEmpty(minutes.CouncilSessionName));
            html = ProcessConditionalBlock(html, "MeetingSummary", !string.IsNullOrEmpty(minutes.MeetingSummary));
            html = ProcessConditionalBlock(html, "Attendees", minutes.Attendees?.Any() == true);
            html = ProcessConditionalBlock(html, "AgendaItems", minutes.AgendaItems?.Any() == true);

            // Process attendees list
            if (minutes.Attendees?.Any() == true)
            {
                html = ProcessAttendeesList(html, minutes.Attendees);
            }

            // Process agenda items
            if (minutes.AgendaItems?.Any() == true)
            {
                html = ProcessAgendaItems(html, minutes.AgendaItems, includeVoterNames);
            }

            // Process all recommendations
            var allRecommendations = minutes.AgendaItems?
                .SelectMany((a, i) => (a.Recommendations ?? new List<MinutesRecommendationDto>())
                    .Select(r => new { AgendaIndex = i + 1, Rec = r }))
                .ToList();

            html = ProcessConditionalBlock(html, "AllRecommendations", allRecommendations?.Any() == true);
            if (allRecommendations?.Any() == true)
            {
                html = ProcessRecommendationsSummary(html, allRecommendations.Cast<dynamic>().ToList());
            }

            return html;
        }



        private string ProcessConditionalBlock(string html, string blockName, bool condition)
        {
            // Use nested block extraction for proper handling of nested blocks
            var extracted = ExtractNestedBlock(html, "if", blockName);
            if (extracted == null)
            {
                // Fall back to simple regex for non-nested cases
                var pattern = $@"\{{\{{#if {blockName}\}}\}}([\s\S]*?)\{{\{{/if\}}\}}";
                if (condition)
                {
                    return Regex.Replace(html, pattern, "$1");
                }
                else
                {
                    return Regex.Replace(html, pattern, "");
                }
            }

            var (fullMatch, content) = extracted.Value;
            if (condition)
            {
                // Keep content, remove markers
                return html.Replace(fullMatch, content);
            }
            else
            {
                // Remove entire block
                return html.Replace(fullMatch, "");
            }
        }

        private string ProcessAttendeesList(string html, List<MinutesAttendeeDto> attendees)
        {
            var pattern = @"\{\{#each Attendees\}\}([\s\S]*?)\{\{/each\}\}";
            var match = Regex.Match(html, pattern);

            if (!match.Success) return html;

            var itemTemplate = match.Groups[1].Value;
            var sb = new StringBuilder();
            int index = 1;

            foreach (var attendee in attendees)
            {
                var item = itemTemplate;
                item = item.Replace("{{Index}}", index.ToString());
                item = item.Replace("{{Name}}", Escape(attendee.Name));
                item = item.Replace("{{JobTitle}}", Escape(attendee.JobTitle ?? "—"));
                item = item.Replace("{{Role}}", GetRoleDisplayName(attendee.Role));
                item = item.Replace("{{Status}}", attendee.Attended ? GetLabel("present") : GetLabel("absent"));
                item = item.Replace("{{StatusClass}}", attendee.Attended ? "status-present" : "status-absent");

                // Handle alternating row background
                item = item.Replace("{{RowBackground}}", index % 2 == 0 ? "#F8F5F8" : "#FFFFFF");

                // Handle {{#if IsPresent}} conditional
                item = ProcessIfElseBlock(item, "IsPresent", attendee.Attended);

                sb.Append(item);
                index++;
            }

            return Regex.Replace(html, pattern, sb.ToString());
        }

        /// <summary>
        /// Processes {{#if condition}}...{{else}}...{{/if}} blocks
        /// </summary>
        private string ProcessIfElseBlock(string html, string blockName, bool condition)
        {
            // Pattern for if-else-endif
            var ifElsePattern = $@"\{{\{{#if {blockName}\}}\}}([\s\S]*?)\{{\{{else\}}\}}([\s\S]*?)\{{\{{/if\}}\}}";
            var ifElseMatch = Regex.Match(html, ifElsePattern);

            if (ifElseMatch.Success)
            {
                var trueContent = ifElseMatch.Groups[1].Value;
                var falseContent = ifElseMatch.Groups[2].Value;
                return Regex.Replace(html, ifElsePattern, condition ? trueContent : falseContent);
            }

            // Pattern for if-endif (no else)
            var ifPattern = $@"\{{\{{#if {blockName}\}}\}}([\s\S]*?)\{{\{{/if\}}\}}";
            var ifMatch = Regex.Match(html, ifPattern);

            if (ifMatch.Success)
            {
                var content = ifMatch.Groups[1].Value;
                return Regex.Replace(html, ifPattern, condition ? content : "");
            }

            return html;
        }

        private string ProcessAgendaItems(string html, List<MinutesAgendaItemDto> agendaItems, bool includeVoterNames)
        {
            // Use nested block extraction to properly handle nested {{#each}} blocks
            var extracted = ExtractNestedBlock(html, "each", "AgendaItems");
            if (extracted == null) return html;

            var (fullMatch, itemTemplate) = extracted.Value;
            var sb = new StringBuilder();

            foreach (var agenda in agendaItems)
            {
                var item = itemTemplate;
                item = item.Replace("{{Index}}", agenda.Index.ToString());
                item = item.Replace("{{Title}}", Escape(agenda.Title));
                item = item.Replace("{{Description}}", Escape(agenda.Description ?? ""));
                item = item.Replace("{{PlannedDuration}}", (agenda.PlannedDuration ?? 0).ToString());
                item = item.Replace("{{ActualDuration}}", (agenda.ActualDuration ?? 0).ToString());
                item = item.Replace("{{Summary}}", EscapeRtl(agenda.Summary ?? ""));

                // Conditional blocks within agenda
                item = ProcessConditionalBlock(item, "Description", !string.IsNullOrEmpty(agenda.Description));
                item = ProcessConditionalBlock(item, "Summary", !string.IsNullOrEmpty(agenda.Summary));
                item = ProcessConditionalBlock(item, "HasDuration", agenda.PlannedDuration.HasValue || agenda.ActualDuration.HasValue);
                item = ProcessConditionalBlock(item, "DiscussionNotes", agenda.DiscussionNotes?.Any() == true);
                item = ProcessConditionalBlock(item, "HasVoting", agenda.HasVoting && agenda.VotingResults != null);
                item = ProcessConditionalBlock(item, "Recommendations", agenda.Recommendations?.Any() == true);
                item = ProcessConditionalBlock(item, "IncludeVoterNames", includeVoterNames);

                // Process discussion notes
                if (agenda.DiscussionNotes?.Any() == true)
                {
                    item = ProcessDiscussionNotes(item, agenda.DiscussionNotes);
                }

                // Process voting
                if (agenda.HasVoting && agenda.VotingResults != null)
                {
                    item = ProcessVotingResults(item, agenda.VotingResults, includeVoterNames);
                }

                // Process recommendations
                if (agenda.Recommendations?.Any() == true)
                {
                    item = ProcessAgendaRecommendations(item, agenda.Recommendations);
                }

                sb.Append(item);
            }

            return html.Replace(fullMatch, sb.ToString());
        }

        /// <summary>
        /// Extracts a nested block properly handling nested {{#blockType}}...{{/blockType}} tags.
        /// Returns the full match string and the inner content, or null if not found.
        /// </summary>
        private (string fullMatch, string content)? ExtractNestedBlock(string html, string blockType, string blockName)
        {
            var openTag = $"{{{{#{blockType} {blockName}}}}}";
            var closeTag = $"{{{{/{blockType}}}}}";
            var genericOpenPattern = $"{{{{#{blockType} ";

            var startIndex = html.IndexOf(openTag);
            if (startIndex == -1) return null;

            var contentStart = startIndex + openTag.Length;
            var depth = 1;
            var currentIndex = contentStart;

            while (depth > 0 && currentIndex < html.Length)
            {
                var nextOpen = html.IndexOf(genericOpenPattern, currentIndex);
                var nextClose = html.IndexOf(closeTag, currentIndex);

                if (nextClose == -1)
                {
                    // No closing tag found
                    return null;
                }

                if (nextOpen != -1 && nextOpen < nextClose)
                {
                    // Found a nested opening tag first
                    depth++;
                    currentIndex = nextOpen + genericOpenPattern.Length;
                }
                else
                {
                    // Found a closing tag
                    depth--;
                    if (depth == 0)
                    {
                        var content = html.Substring(contentStart, nextClose - contentStart);
                        var fullMatch = html.Substring(startIndex, nextClose + closeTag.Length - startIndex);
                        return (fullMatch, content);
                    }
                    currentIndex = nextClose + closeTag.Length;
                }
            }

            return null;
        }

        private string ProcessDiscussionNotes(string html, List<MinutesNoteDto> notes)
        {
            var pattern = @"\{\{#each DiscussionNotes\}\}([\s\S]*?)\{\{/each\}\}";
            var match = Regex.Match(html, pattern);

            if (!match.Success) return html;

            var itemTemplate = match.Groups[1].Value;
            var sb = new StringBuilder();

            foreach (var note in notes)
            {
                var item = itemTemplate;
                item = item.Replace("{{Text}}", Escape(note.Text));
                item = item.Replace("{{AuthorName}}", Escape(note.AuthorName));
                sb.Append(item);
            }

            return Regex.Replace(html, pattern, sb.ToString());
        }

        private string ProcessVotingResults(string html, MinutesVotingResultsDto voting, bool includeVoterNames)
        {
            html = html.Replace("{{VotingOutcome}}", Escape(voting.Outcome));

            var pattern = @"\{\{#each VotingOptions\}\}([\s\S]*?)\{\{/each\}\}";
            var match = Regex.Match(html, pattern);

            if (!match.Success || voting.Options == null) return html;

            var itemTemplate = match.Groups[1].Value;
            var sb = new StringBuilder();
            int index = 1;

            foreach (var option in voting.Options)
            {
                var item = itemTemplate;
                item = item.Replace("{{Name}}", Escape(option.NameAr ?? option.Name));
                item = item.Replace("{{VoteCount}}", option.VoteCount.ToString());
                item = item.Replace("{{Percentage}}", option.Percentage.ToString());
                item = item.Replace("{{Voters}}", Escape(string.Join("، ", option.Voters ?? new List<string>())));
                item = item.Replace("{{RowBackground}}", index % 2 == 0 ? "#F8F5F8" : "#FFFFFF");

                // Handle conditional voter names
                item = ProcessConditionalBlock(item, "../IncludeVoterNames", includeVoterNames);

                sb.Append(item);
                index++;
            }

            return Regex.Replace(html, pattern, sb.ToString());
        }

        private string ProcessAgendaRecommendations(string html, List<MinutesRecommendationDto> recommendations)
        {
            var pattern = @"\{\{#each Recommendations\}\}([\s\S]*?)\{\{/each\}\}";
            var match = Regex.Match(html, pattern);

            if (!match.Success) return html;

            var itemTemplate = match.Groups[1].Value;
            var sb = new StringBuilder();

            foreach (var rec in recommendations)
            {
                var item = itemTemplate;
                item = item.Replace("{{Text}}", Escape(rec.Text));
                item = item.Replace("{{OwnerName}}", Escape(rec.OwnerName ?? GetLabel("notSpecified")));
                item = item.Replace("{{DueDate}}", rec.DueDate?.ToString("d MMMM yyyy", DateCulture) ?? GetLabel("notSpecified"));
                item = ProcessConditionalBlock(item, "HasMeta", !string.IsNullOrEmpty(rec.OwnerName) || rec.DueDate.HasValue);
                sb.Append(item);
            }

            return Regex.Replace(html, pattern, sb.ToString());
        }

        private string ProcessRecommendationsSummary(string html, List<dynamic> recommendations)
        {
            var pattern = @"\{\{#each AllRecommendations\}\}([\s\S]*?)\{\{/each\}\}";
            var match = Regex.Match(html, pattern);

            if (!match.Success) return html;

            var itemTemplate = match.Groups[1].Value;
            var sb = new StringBuilder();
            int index = 1;

            foreach (var item in recommendations)
            {
                var row = itemTemplate;
                row = row.Replace("{{Index}}", index.ToString());
                row = row.Replace("{{Text}}", Escape(item.Rec.Text));
                row = row.Replace("{{AgendaIndex}}", item.AgendaIndex.ToString());
                row = row.Replace("{{OwnerName}}", Escape(item.Rec.OwnerName ?? GetLabel("notSpecified")));
                row = row.Replace("{{DueDate}}", item.Rec.DueDate?.ToString("d/M/yyyy", DateCulture) ?? "—");
                row = row.Replace("{{RowBackground}}", index % 2 == 0 ? "#F8F5F8" : "#FFFFFF");
                sb.Append(row);
                index++;
            }

            return Regex.Replace(html, pattern, sb.ToString());
        }

        private string GenerateDefaultHtml(MeetingMinutesDto minutes, bool includeVoterNames)
        {
            var sb = new StringBuilder();

            var dir = _isArabic ? "rtl" : "ltr";
            var lang = _isArabic ? "ar" : "en";
            var textAlign = _isArabic ? "right" : "left";
            var dateCulture = DateCulture;

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine($"<html lang=\"{lang}\" dir=\"{dir}\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"UTF-8\">");
            sb.AppendLine("<style>");
            sb.AppendLine(GenerateCss());
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Header
            sb.AppendLine("<div class=\"header\">");
            if (!string.IsNullOrEmpty(minutes.OrganizationName))
                sb.AppendLine($"<div class=\"org-name\">{Escape(minutes.OrganizationName)}</div>");
            if (!string.IsNullOrEmpty(minutes.CommitteeName))
                sb.AppendLine($"<div class=\"committee-name\">{Escape(minutes.CommitteeName)}</div>");
            sb.AppendLine($"<div class=\"main-title\">{GetLabel("meetingMinutes")}</div>");
            sb.AppendLine($"<div class=\"meeting-title\">{Escape(minutes.Title)}</div>");
            sb.AppendLine($"<div class=\"reference-info\">{GetLabel("referenceNumber")}: {Escape(minutes.ReferenceNumber)} | {GetLabel("meetingNumber")}: {minutes.MeetingNumber}</div>");
            sb.AppendLine("</div>");

            // Meeting Info
            sb.AppendLine("<div class=\"section\">");
            sb.AppendLine($"<div class=\"section-title\">{GetLabel("meetingInfo")}</div>");
            sb.AppendLine("<table class=\"info-table\">");
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td><span class=\"label\">{GetLabel("date")}: </span><span class=\"value\">{minutes.Date.ToString("dddd, d MMMM yyyy", dateCulture)}</span></td>");
            sb.AppendLine($"<td><span class=\"label\">{GetLabel("time")}: </span><span class=\"value\">{GetLabel("from")} {minutes.StartTime} {GetLabel("to")} {minutes.EndTime}</span></td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td><span class=\"label\">{GetLabel("location")}: </span><span class=\"value\">{Escape(minutes.Location ?? GetLabel("notSpecified"))}</span></td>");
            sb.AppendLine($"<td><span class=\"label\">{GetLabel("duration")}: </span><span class=\"value\">{Escape(minutes.ActualDuration ?? GetLabel("notSpecified"))}</span></td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td><span class=\"label\">{GetLabel("attendance")}: </span><span class=\"value\">{minutes.PresentCount} {GetLabel("outOf")} {minutes.TotalAttendees} ({(minutes.QuorumMet ? GetLabel("quorumMet") : GetLabel("quorumNotMet"))})</span></td>");
            if (!string.IsNullOrEmpty(minutes.CouncilSessionName))
                sb.AppendLine($"<td><span class=\"label\">{GetLabel("session")}: </span><span class=\"value\">{Escape(minutes.CouncilSessionName)}</span></td>");
            else
                sb.AppendLine("<td></td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");

            // Attendees
            if (minutes.Attendees?.Any() == true)
            {
                sb.AppendLine("<div class=\"section\">");
                sb.AppendLine($"<div class=\"section-title\">{GetLabel("attendeesAndAbsence")}</div>");
                sb.AppendLine("<table class=\"data-table\">");
                sb.AppendLine("<thead><tr>");
                sb.AppendLine($"<th>#</th><th>{GetLabel("name")}</th><th>{GetLabel("jobTitle")}</th><th>{GetLabel("role")}</th><th>{GetLabel("attendanceStatus")}</th>");
                sb.AppendLine("</tr></thead><tbody>");
                int idx = 1;
                foreach (var a in minutes.Attendees)
                {
                    var cls = a.Attended ? "status-present" : "status-absent";
                    var sts = a.Attended ? GetLabel("present") : GetLabel("absent");
                    sb.AppendLine($"<tr><td>{idx}</td><td>{Escape(a.Name)}</td><td>{Escape(a.JobTitle ?? "—")}</td><td>{GetRoleDisplayName(a.Role)}</td><td class=\"{cls}\">{sts}</td></tr>");
                    idx++;
                }
                sb.AppendLine("</tbody></table></div>");
            }

            // Agenda
            if (minutes.AgendaItems?.Any() == true)
            {
                sb.AppendLine("<div class=\"section\">");
                sb.AppendLine($"<div class=\"section-title\">{GetLabel("agenda")}</div>");
                foreach (var agenda in minutes.AgendaItems)
                {
                    sb.AppendLine("<div class=\"agenda-item\">");
                    sb.AppendLine($"<div class=\"agenda-title\">{GetLabel("agendaItem")} {agenda.Index}: {Escape(agenda.Title)}</div>");
                    if (!string.IsNullOrEmpty(agenda.Description))
                        sb.AppendLine($"<div class=\"agenda-description\">{Escape(agenda.Description)}</div>");
                    if (agenda.PlannedDuration.HasValue || agenda.ActualDuration.HasValue)
                        sb.AppendLine($"<div class=\"agenda-duration\">{GetLabel("plannedDuration")}: {agenda.PlannedDuration ?? 0} {GetLabel("minutes")} | {GetLabel("actualDuration")}: {agenda.ActualDuration ?? 0} {GetLabel("minutes")}</div>");

                    // Notes
                    if (agenda.DiscussionNotes?.Any() == true)
                    {
                        sb.AppendLine("<div class=\"notes-section\">");
                        sb.AppendLine($"<div class=\"notes-title\">{GetLabel("notes")}:</div>");
                        foreach (var n in agenda.DiscussionNotes)
                            sb.AppendLine($"<div class=\"note-item\">{Escape(n.Text)} — {Escape(n.AuthorName)}</div>");
                        sb.AppendLine("</div>");
                    }

                    // Voting
                    if (agenda.HasVoting && agenda.VotingResults?.Options?.Any() == true)
                    {
                        sb.AppendLine("<div class=\"voting-section\">");
                        sb.AppendLine($"<div class=\"voting-title\">{GetLabel("votingResults")}:</div>");
                        sb.AppendLine("<table class=\"data-table\"><thead><tr>");
                        sb.AppendLine($"<th>{GetLabel("option")}</th><th>{GetLabel("votes")}</th><th>{GetLabel("percentage")}</th>");
                        if (includeVoterNames) sb.AppendLine($"<th>{GetLabel("voters")}</th>");
                        sb.AppendLine("</tr></thead><tbody>");
                        foreach (var opt in agenda.VotingResults.Options)
                        {
                            sb.Append($"<tr><td>{Escape(opt.NameAr ?? opt.Name)}</td><td>{opt.VoteCount}</td><td>{opt.Percentage}%</td>");
                            if (includeVoterNames) sb.Append($"<td>{Escape(string.Join("، ", opt.Voters ?? new List<string>()))}</td>");
                            sb.AppendLine("</tr>");
                        }
                        sb.AppendLine("</tbody></table>");
                        sb.AppendLine($"<div class=\"voting-outcome\">{GetLabel("decision")}: {Escape(agenda.VotingResults.Outcome)}</div>");
                        sb.AppendLine("</div>");
                    }

                    // Recommendations
                    if (agenda.Recommendations?.Any() == true)
                    {
                        sb.AppendLine("<div class=\"recommendations-section\">");
                        sb.AppendLine($"<div class=\"recommendations-title\">{GetLabel("recommendations")}:</div>");
                        foreach (var r in agenda.Recommendations)
                        {
                            sb.AppendLine($"<div class=\"recommendation-item\">{Escape(r.Text)}</div>");
                            if (!string.IsNullOrEmpty(r.OwnerName) || r.DueDate.HasValue)
                                sb.AppendLine($"<div class=\"recommendation-meta\">{GetLabel("responsible")}: {Escape(r.OwnerName ?? GetLabel("notSpecified"))} | {GetLabel("dueDate")}: {(r.DueDate?.ToString("d MMMM yyyy", dateCulture) ?? GetLabel("notSpecified"))}</div>");
                        }
                        sb.AppendLine("</div>");
                    }
                    sb.AppendLine("</div>");
                }
                sb.AppendLine("</div>");
            }

            // Recommendations Summary
            var allRecs = minutes.AgendaItems?
                .SelectMany((a, i) => (a.Recommendations ?? new List<MinutesRecommendationDto>())
                    .Select(r => new { AgendaIndex = i + 1, Rec = r }))
                .ToList();
            if (allRecs?.Any() == true)
            {
                sb.AppendLine("<div class=\"section\">");
                sb.AppendLine($"<div class=\"section-title\">{GetLabel("recommendationsSummary")}</div>");
                sb.AppendLine("<table class=\"data-table\"><thead><tr>");
                sb.AppendLine($"<th>#</th><th>{GetLabel("recommendation")}</th><th>{GetLabel("agendaItem")}</th><th>{GetLabel("responsible")}</th><th>{GetLabel("dueDate")}</th>");
                sb.AppendLine("</tr></thead><tbody>");
                int ri = 1;
                foreach (var item in allRecs)
                {
                    sb.AppendLine($"<tr><td>{ri}</td><td style=\"text-align:{textAlign};\">{Escape(item.Rec.Text)}</td><td>{GetLabel("agendaItem")} {item.AgendaIndex}</td><td>{Escape(item.Rec.OwnerName ?? GetLabel("notSpecified"))}</td><td>{(item.Rec.DueDate?.ToString("d/M/yyyy", dateCulture) ?? "—")}</td></tr>");
                    ri++;
                }
                sb.AppendLine("</tbody></table></div>");
            }

            // Summary
            if (!string.IsNullOrEmpty(minutes.MeetingSummary))
            {
                sb.AppendLine("<div class=\"section\">");
                sb.AppendLine($"<div class=\"section-title\">{GetLabel("meetingSummary")}</div>");
                sb.AppendLine($"<div class=\"summary-text\">{Escape(minutes.MeetingSummary)}</div>");
                sb.AppendLine("</div>");
            }

            // Signatures
            sb.AppendLine("<div class=\"section\">");
            sb.AppendLine($"<div class=\"section-title\">{GetLabel("signatures")}</div>");
            sb.AppendLine("<table class=\"signature-table\"><tr>");
            sb.AppendLine($"<td><div class=\"signature-title\">{Escape(minutes.ChairmanTitle)}</div><div class=\"signature-name\">{Escape(minutes.ChairmanName ?? "")}</div><div class=\"signature-line\">{GetLabel("signature")}</div></td>");
            sb.AppendLine($"<td><div class=\"signature-title\">{Escape(minutes.SecretaryTitle)}</div><div class=\"signature-name\">{Escape(minutes.SecretaryName ?? "")}</div><div class=\"signature-line\">{GetLabel("signature")}</div></td>");
            sb.AppendLine("</tr></table>");
            sb.AppendLine($"<div class=\"footer\">{GetLabel("meetingMinutes")} {minutes.MeetingNumber} | {GetLabel("version")} {minutes.VersionLabel} | {GetLabel("generatedAt")} {minutes.GeneratedAt:d/M/yyyy HH:mm}</div>");
            sb.AppendLine("</div>");

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private string GenerateCss()
        {
            return $@"
                @import url('https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700&display=swap');
                * {{ margin: 0; padding: 0; box-sizing: border-box; }}
                body {{ font-family: 'Tajawal', sans-serif; font-size: 12px; color: #000; direction: {(_isArabic ? "rtl" : "ltr")}; text-align: {(_isArabic ? "right" : "left")}; line-height: 1.6; padding: 20px; }}
                .header {{ text-align: center; margin-bottom: 30px; padding-bottom: 20px; border-bottom: 2px solid {_config.Colors.Border}; }}
                .org-name {{ font-size: 14px; font-weight: 700; color: {_config.Colors.Primary}; margin-bottom: 5px; }}
                .committee-name {{ font-size: 12px; color: {_config.Colors.MutedText}; margin-bottom: 15px; }}
                .main-title {{ font-size: 24px; font-weight: 700; color: {_config.Colors.Primary}; margin-bottom: 10px; }}
                .meeting-title {{ font-size: 18px; font-weight: 700; color: #000; margin-bottom: 10px; }}
                .reference-info {{ font-size: 10px; color: {_config.Colors.MutedText}; }}
                .section {{ margin-bottom: 25px; }}
                .section-title {{ font-size: 14px; font-weight: 700; color: {_config.Colors.Primary}; margin-bottom: 12px; padding-bottom: 5px; border-bottom: 1px solid {_config.Colors.Border}; }}
                table {{ width: 100%; border-collapse: collapse; margin-bottom: 15px; }}
                .info-table td {{ padding: 10px 15px; border: 1px solid #e0e0e0; }}
                .info-table tr:nth-child(odd) td {{ background: {_config.Colors.Secondary}; }}
                .info-table .label {{ font-weight: 700; color: {_config.Colors.MutedText}; }}
                .data-table {{ border-top: 2px solid {_config.Colors.Primary}; border-bottom: 2px solid {_config.Colors.Primary}; }}
                .data-table th {{ background: {_config.Colors.Primary}; color: #fff; padding: 12px 10px; font-weight: 700; text-align: center; font-size: 12px; }}
                .data-table td {{ padding: 10px; border-bottom: 1px solid #e0e0e0; text-align: center; font-size: 11px; }}
                .data-table tr:nth-child(even) td {{ background: {_config.Colors.Secondary}; }}
                .status-present {{ color: {_config.Colors.Success}; font-weight: 700; }}
                .status-absent {{ color: {_config.Colors.Danger}; font-weight: 700; }}
                .agenda-item {{ margin-bottom: 20px; padding: 15px; background: #fafafa; border-radius: 8px; {(_isArabic ? "border-right" : "border-left")}: 4px solid {_config.Colors.Primary}; }}
                .agenda-title {{ font-size: 14px; font-weight: 700; color: {_config.Colors.Primary}; margin-bottom: 8px; }}
                .agenda-description {{ font-size: 12px; color: {_config.Colors.MutedText}; margin-bottom: 8px; }}
                .agenda-duration {{ font-size: 10px; color: {_config.Colors.MutedText}; margin-bottom: 10px; }}
                .notes-section, .recommendations-section {{ margin-top: 10px; padding-top: 10px; border-top: 1px dashed #ddd; }}
                .notes-title, .recommendations-title {{ font-size: 12px; font-weight: 700; margin-bottom: 8px; }}
                .note-item, .recommendation-item {{ font-size: 11px; margin-bottom: 5px; padding-right: 15px; }}
                .note-item::before, .recommendation-item::before {{ content: '•'; margin-left: 8px; color: {_config.Colors.Primary}; }}
                .recommendation-meta {{ font-size: 10px; color: {_config.Colors.MutedText}; padding-right: 15px; }}
                .voting-section {{ margin-top: 15px; padding: 12px; background: #f0f7ff; border-radius: 6px; }}
                .voting-title {{ font-size: 12px; font-weight: 700; margin-bottom: 10px; }}
                .voting-outcome {{ margin-top: 10px; font-weight: 700; color: {_config.Colors.Success}; }}
                .signature-table {{ width: 100%; border: none; margin-top: 40px; }}
                .signature-table td {{ width: 50%; text-align: center; border: none; padding: 20px; vertical-align: top; }}
                .signature-title {{ font-size: 14px; font-weight: 700; color: {_config.Colors.Primary}; margin-bottom: 5px; }}
                .signature-name {{ font-size: 13px; margin-bottom: 30px; }}
                .signature-line {{ border-top: 1px solid {_config.Colors.Border}; padding-top: 5px; font-size: 10px; color: {_config.Colors.MutedText}; }}
                .footer {{ margin-top: 30px; padding-top: 15px; border-top: 1px solid #eee; text-align: center; font-size: 9px; color: {_config.Colors.MutedText}; }}
                .summary-text {{ font-size: 12px; line-height: 1.8; padding: 15px; background: {_config.Colors.Secondary}; border-radius: 8px; }}
            ";
        }

        private static string Escape(string? text) => string.IsNullOrEmpty(text) ? "" : System.Net.WebUtility.HtmlEncode(text);

        private string EscapeRtl(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var escaped = System.Web.HttpUtility.HtmlEncode(text);
            return "\u200F" + escaped; // RLM character forces RTL
        }

        private string GetLabel(string key) => _config.Labels.TryGetValue(key, out var label) ? label : GetDefaultLabel(key);

        private string GetRoleDisplayName(string role)
        {
            var roleKey = role?.ToLower();
            if (roleKey != null && _config.Roles.TryGetValue(roleKey, out var roleName))
                return roleName;
            return role?.ToLower() switch
            {
                "chairman" => _isArabic ? "رئيس" : "Chairman",
                "vice_chairman" => _isArabic ? "نائب الرئيس" : "Vice Chairman",
                "secretary" => _isArabic ? "أمين السر" : "Secretary",
                "member" => _isArabic ? "عضو" : "Member",
                "guest" => _isArabic ? "ضيف" : "Guest",
                "observer" => _isArabic ? "مراقب" : "Observer",
                _ => _isArabic ? "عضو" : "Member"
            };
        }

        private string GetDefaultLabel(string key) => key switch
        {
            "meetingMinutes" => _isArabic ? "محضر اجتماع" : "Meeting Minutes",
            "referenceNumber" => _isArabic ? "رقم المرجع" : "Reference Number",
            "meetingNumber" => _isArabic ? "رقم الاجتماع" : "Meeting Number",
            "meetingInfo" => _isArabic ? "معلومات الاجتماع" : "Meeting Information",
            "date" => _isArabic ? "التاريخ" : "Date",
            "time" => _isArabic ? "الوقت" : "Time",
            "location" => _isArabic ? "المكان" : "Location",
            "duration" => _isArabic ? "المدة" : "Duration",
            "attendance" => _isArabic ? "الحضور" : "Attendance",
            "session" => _isArabic ? "الدورة" : "Session",
            "attendeesAndAbsence" => _isArabic ? "الحضور والغياب" : "Attendees & Absence",
            "name" => _isArabic ? "الاسم" : "Name",
            "jobTitle" => _isArabic ? "المسمى الوظيفي" : "Job Title",
            "role" => _isArabic ? "الصفة" : "Role",
            "attendanceStatus" => _isArabic ? "الحضور" : "Status",
            "present" => _isArabic ? "حاضر" : "Present",
            "absent" => _isArabic ? "غائب" : "Absent",
            "agenda" => _isArabic ? "جدول الأعمال" : "Agenda",
            "agendaItem" => _isArabic ? "البند" : "Item",
            "plannedDuration" => _isArabic ? "المدة المخططة" : "Planned Duration",
            "actualDuration" => _isArabic ? "المدة الفعلية" : "Actual Duration",
            "minutes" => _isArabic ? "دقيقة" : "min",
            "notes" => _isArabic ? "الملاحظات" : "Notes",
            "votingResults" => _isArabic ? "نتائج التصويت" : "Voting Results",
            "option" => _isArabic ? "الخيار" : "Option",
            "votes" => _isArabic ? "الأصوات" : "Votes",
            "percentage" => _isArabic ? "النسبة" : "Percentage",
            "voters" => _isArabic ? "المصوتون" : "Voters",
            "decision" => _isArabic ? "القرار" : "Decision",
            "recommendations" => _isArabic ? "التوصيات" : "Recommendations",
            "recommendationsSummary" => _isArabic ? "ملخص التوصيات والمهام" : "Recommendations & Tasks Summary",
            "recommendation" => _isArabic ? "التوصية" : "Recommendation",
            "responsible" => _isArabic ? "المسؤول" : "Responsible",
            "dueDate" => _isArabic ? "الموعد" : "Due Date",
            "notSpecified" => _isArabic ? "غير محدد" : "Not specified",
            "meetingSummary" => _isArabic ? "ملخص الاجتماع" : "Meeting Summary",
            "signatures" => _isArabic ? "التوقيعات" : "Signatures",
            "quorumMet" => _isArabic ? "النصاب مكتمل" : "Quorum met",
            "quorumNotMet" => _isArabic ? "النصاب غير مكتمل" : "Quorum not met",
            "outOf" => _isArabic ? "من أصل" : "out of",
            "from" => _isArabic ? "من" : "From",
            "to" => _isArabic ? "إلى" : "To",
            "version" => _isArabic ? "الإصدار" : "Version",
            "generatedAt" => _isArabic ? "تم الإنشاء بتاريخ" : "Generated at",
            "signature" => _isArabic ? "التوقيع" : "Signature",
            _ => key
        };

        private static MomTemplateConfigDto GetDefaultConfig() => new()
        {
            Colors = new MomTemplateColorsDto(),
            Fonts = new MomTemplateFontsDto(),
            PageLayout = new MomTemplatePageLayoutDto(),
            Sections = new Dictionary<string, MomTemplateSectionDto>
            {
                ["header"] = new() { Visible = true, Order = 1 },
                ["meetingInfo"] = new() { Visible = true, Order = 2 },
                ["attendees"] = new() { Visible = true, Order = 3 },
                ["agenda"] = new() { Visible = true, Order = 4 },
                ["voting"] = new() { Visible = true, Order = 5 },
                ["recommendations"] = new() { Visible = true, Order = 6 },
                ["summary"] = new() { Visible = true, Order = 7 },
                ["signatures"] = new() { Visible = true, Order = 8 }
            },
            Labels = new Dictionary<string, string>(),
            Roles = new Dictionary<string, string>(), // Empty: bilingual defaults handled by GetRoleDisplayName()
            TableColumns = new MomTemplateTableColumnsDto()
        };

        /// <summary>
        /// Gets the default HTML template from embedded resources
        /// </summary>
        public static string GetDefaultHtmlTemplate()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = Path.Combine(Path.GetDirectoryName(assembly.Location) ?? "", "Assets", "Templates", "MomTemplate.html");

            if (File.Exists(resourcePath))
            {
                return File.ReadAllText(resourcePath, Encoding.UTF8);
            }

            // Return embedded default if file not found
            return GetEmbeddedDefaultTemplate();
        }

        private static string GetEmbeddedDefaultTemplate()
        {
            // This template uses {{Direction}}, {{Lang}}, {{TextAlign}} placeholders
            // which are resolved by ProcessHtmlTemplate at generation time.
            // Labels like {{Title}} are also resolved by the template processor.
            return @"<!DOCTYPE html>
<html lang=""{{Lang}}"" dir=""{{Direction}}"">
<head>
    <meta charset=""UTF-8"">
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700&display=swap');
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: 'Tajawal', sans-serif; font-size: 12px; color: #000; direction: {{Direction}}; text-align: {{TextAlign}}; line-height: 1.6; padding: 20px; }
        .header { text-align: center; margin-bottom: 30px; padding-bottom: 20px; border-bottom: 2px solid #C085C0; }
        .org-name { font-size: 14px; font-weight: 700; color: #803580; margin-bottom: 5px; }
        .committee-name { font-size: 12px; color: #646464; margin-bottom: 15px; }
        .main-title { font-size: 24px; font-weight: 700; color: #803580; margin-bottom: 10px; }
        .meeting-title { font-size: 18px; font-weight: 700; color: #000; margin-bottom: 10px; }
        .reference-info { font-size: 10px; color: #646464; }
        .section { margin-bottom: 25px; }
        .section-title { font-size: 14px; font-weight: 700; color: #803580; margin-bottom: 12px; padding-bottom: 5px; border-bottom: 1px solid #C085C0; }
        table { width: 100%; border-collapse: collapse; margin-bottom: 15px; }
        .info-table td { padding: 10px 15px; border: 1px solid #e0e0e0; }
        .info-table tr:nth-child(odd) td { background: #F5F5FA; }
        .info-table .label { font-weight: 700; color: #646464; }
        .data-table { border-top: 2px solid #803580; border-bottom: 2px solid #803580; }
        .data-table th { background: #803580; color: #fff; padding: 12px 10px; font-weight: 700; text-align: center; }
        .data-table td { padding: 10px; border-bottom: 1px solid #e0e0e0; text-align: center; font-size: 11px; }
        .data-table tr:nth-child(even) td { background: #F5F5FA; }
        .status-present { color: #228B22; font-weight: 700; }
        .status-absent { color: #DC3545; font-weight: 700; }
        .agenda-item { margin-bottom: 20px; padding: 15px; background: #fafafa; border-radius: 8px; border-right: 4px solid #803580; }
        .agenda-title { font-size: 14px; font-weight: 700; color: #803580; margin-bottom: 8px; }
        .signature-table td { width: 50%; text-align: center; border: none; padding: 20px; }
        .signature-title { font-size: 14px; font-weight: 700; color: #803580; margin-bottom: 5px; }
        .signature-name { font-size: 13px; margin-bottom: 30px; }
        .signature-line { border-top: 1px solid #C085C0; padding-top: 5px; font-size: 10px; color: #646464; }
        .footer { margin-top: 30px; padding-top: 15px; border-top: 1px solid #eee; text-align: center; font-size: 9px; color: #646464; }
    </style>
</head>
<body>
    <div class=""header"">
        {{#if OrganizationName}}<div class=""org-name"">{{OrganizationName}}</div>{{/if}}
        {{#if CommitteeName}}<div class=""committee-name"">{{CommitteeName}}</div>{{/if}}
        <div class=""main-title"">{{Title}}</div>
        <div class=""meeting-title"">{{Title}}</div>
        <div class=""reference-info"">{{ReferenceNumber}} | {{MeetingNumber}}</div>
    </div>
    <!-- Add more sections as needed -->
</body>
</html>";
        }
    }
}
