using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Reflection;

namespace Intalio.Tools.Common.Smtp
{
    public class MailService
    {
        private readonly string _host = "";
        private readonly int _port = 0;
        private readonly string _user = "";
        private readonly string _password = "";
        private readonly string _from = "";
        private readonly bool _enableSSL = false;
        private readonly bool _asBcc = false;
        private readonly Dictionary<string, string> imageMap = new()
        {
            {"logo", "gmedia_logo.png"},
            {"in", "linkedin.png"},
            {"x", "twitter.png"},
            {"facebook", "facebook.png"},
            {"insta", "instagram.png"}
        };

        public MailService(string host, int port, string fromEmail, string user, string password, bool enableSSL, bool sendAsBcc = false)
        {
            _host = host;
            _port = port;
            _from = fromEmail;
            _user = user;
            _password = password;
            _enableSSL = enableSSL;
            _asBcc = sendAsBcc;
        }

        public void SendEmailAsync(string subject, string body, bool rtl = true, params string[] to)
        {
            if (!string.IsNullOrEmpty(_host))
            {
                bool credentialsAvailable = !string.IsNullOrEmpty(_user) && !string.IsNullOrEmpty(_password);

                MailMessage mailMessage = new()
                {
                    From = new MailAddress(_from),
                    Subject = subject,
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = true,
                    Body = body
                };
                mailMessage.Headers.Add("X-Priority", "Normal");
                mailMessage.Headers.Add("X-MSMail-Priority", "Normal");
                mailMessage.Headers.Add("X-MS-Exchange-AllowExternalImages", "true");
                if (_asBcc)
                {
                    mailMessage.To.Add(_from);
                    foreach (string destination in to)
                    {
                        mailMessage.To.Add(destination);
                    }
                }
                else
                {
                    foreach (string destination in to)
                    {
                        mailMessage.To.Add(destination);
                    }
                }

                SmtpClient client = new(_host, _port)
                {
                    EnableSsl = _enableSSL,
                    Credentials = credentialsAvailable ? new NetworkCredential(_user, _password) : null,
                    UseDefaultCredentials = !credentialsAvailable
                };

                client.SendAsync(mailMessage, null);
            }
        }

        public void SendEmailWithAttachmentAsync(string subject, string body, List<Attachment> attachments, bool rtl = true, params string[] to)
        {
            if (!string.IsNullOrEmpty(_host))
            {
                bool credentialsAvailable = !string.IsNullOrEmpty(_user) && !string.IsNullOrEmpty(_password);

                MailMessage mailMessage = new()
                {
                    From = new MailAddress(_from),
                    Subject = subject,
                    IsBodyHtml = true
                };

                // Create HTML view
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(
                    body,
                    Encoding.UTF8,
                    MediaTypeNames.Text.Html
                );

                htmlView.ContentType.CharSet = "utf-8";
                htmlView.TransferEncoding = TransferEncoding.QuotedPrintable;

                // Add embedded images from Assets/Images folder
                AddEmbeddedImages(htmlView);

                mailMessage.AlternateViews.Add(htmlView);

                // Add calendar and other attachments
                /*if (attachments.Count > 0)
                {
                    foreach (Attachment attachment in attachments)
                    {
                        mailMessage.Attachments.Add(attachment);
                    }
                }*/

                // Add recipients
                if (_asBcc)
                {
                    mailMessage.To.Add(_from);
                    foreach (string destination in to)
                    {
                        mailMessage.To.Add(destination);
                    }
                }
                else
                {
                    foreach (string destination in to)
                    {
                        mailMessage.To.Add(destination);
                    }
                }

                SmtpClient client = new(_host, _port)
                {
                    EnableSsl = _enableSSL,
                    Credentials = credentialsAvailable ? new NetworkCredential(_user, _password) : null,
                    UseDefaultCredentials = !credentialsAvailable
                };

                // Add headers for better compatibility
                mailMessage.Headers.Add("MIME-Version", "1.0");
                mailMessage.Headers.Add("X-Mailer", "Intalio Mail Service");
                mailMessage.Headers.Add("X-Priority", "Normal");
                mailMessage.Headers.Add("X-MSMail-Priority", "Normal");
                mailMessage.Headers.Add("X-MS-Exchange-AllowExternalImages", "true");

                client.SendAsync(mailMessage, null);
            }
        }

        private void AddEmbeddedImages(AlternateView htmlView)
        {
            foreach (var imageEntry in imageMap)
            {
                string cid = imageEntry.Key;
                string imageName = imageEntry.Value;

                try
                {
                    // Try to get image from file system first (for published app)
                    byte[] imageBytes = GetImageFromFile(imageName);

                    // If not found in file system, try embedded resources
                    if (imageBytes == null)
                    {
                        imageBytes = GetEmbeddedImage(imageName);
                    }

                    if (imageBytes != null)
                    {
                        MemoryStream imageStream = new MemoryStream(imageBytes);
                        string mimeType = GetMimeTypeFromFileName(imageName);

                        LinkedResource linkedResource = new LinkedResource(imageStream, mimeType);
                        linkedResource.ContentId = cid;
                        linkedResource.TransferEncoding = TransferEncoding.Base64;

                        htmlView.LinkedResources.Add(linkedResource);

                        Console.WriteLine($"Embedded image added: cid={cid}, file={imageName}");
                    }
                    else
                    {
                        Console.WriteLine($"Image not found: {imageName}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing embedded image {imageName}: {ex.Message}");
                }
            }
        }

        private byte[] GetImageFromFile(string imageName)
        {
            try
            {
                // Try different possible paths
                string[] possiblePaths = {
                    Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Images", imageName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", imageName),
                    Path.Combine(Directory.GetCurrentDirectory(), "Images", imageName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", imageName)
                };

                foreach (string path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        Console.WriteLine($"Found image at: {path}");
                        return File.ReadAllBytes(path);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading image file {imageName}: {ex.Message}");
                return null;
            }
        }

        private byte[] GetEmbeddedImage(string imageName)
        {
            try
            {
                // Get the current assembly
                var assembly = Assembly.GetExecutingAssembly();

                // Try different possible resource names
                string[] possibleResourceNames = {
                    $"Intalio.Tools.Common.Assets.Images.{imageName}",
                    $"Intalio.Tools.Common.Images.{imageName}",
                    $"Assets.Images.{imageName}",
                    $"Images.{imageName}",
                    imageName
                };

                foreach (string resourceName in possibleResourceNames)
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        Console.WriteLine($"Found embedded resource: {resourceName}");
                        using var memoryStream = new MemoryStream();
                        stream.CopyTo(memoryStream);
                        return memoryStream.ToArray();
                    }
                }

                // List all available resources for debugging
                Console.WriteLine("Available embedded resources:");
                foreach (string resource in assembly.GetManifestResourceNames())
                {
                    Console.WriteLine($"  - {resource}");
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading embedded image {imageName}: {ex.Message}");
                return null;
            }
        }

        private string GetMimeTypeFromFileName(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLower();
            return extension switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                ".bmp" => "image/bmp",
                _ => "image/png" // default fallback
            };
        }

        /// <summary>
        /// Sends an email with an iCalendar (.ics) attachment for Outlook calendar integration.
        /// The calendar event will appear as an invite in Outlook with Accept/Decline buttons.
        /// </summary>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email HTML body</param>
        /// <param name="icsContent">iCalendar content string</param>
        /// <param name="method">Calendar method: REQUEST, CANCEL, etc.</param>
        /// <param name="to">Recipient email addresses</param>
        public void SendCalendarInviteAsync(string subject, string body, string icsContent, string method = "REQUEST", params string[] to)
        {
            if (!string.IsNullOrEmpty(_host))
            {
                bool credentialsAvailable = !string.IsNullOrEmpty(_user) && !string.IsNullOrEmpty(_password);

                MailMessage mailMessage = new()
                {
                    From = new MailAddress(_from),
                    Subject = subject,
                    IsBodyHtml = true
                };

                // Create HTML body view
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(
                    body,
                    Encoding.UTF8,
                    MediaTypeNames.Text.Html
                );
                htmlView.TransferEncoding = TransferEncoding.QuotedPrintable;
                AddEmbeddedImages(htmlView);

                // Create calendar view - this makes Outlook show Accept/Decline buttons
                ContentType calendarContentType = new ContentType("text/calendar");
                calendarContentType.Parameters.Add("method", method);
                calendarContentType.CharSet = "UTF-8";

                AlternateView calendarView = AlternateView.CreateAlternateViewFromString(
                    icsContent,
                    Encoding.UTF8,
                    "text/calendar"
                );
                calendarView.ContentType = calendarContentType;
                calendarView.TransferEncoding = TransferEncoding.SevenBit;

                // Add views - calendar view first for better Outlook compatibility
                mailMessage.AlternateViews.Add(calendarView);
                mailMessage.AlternateViews.Add(htmlView);

                // Also attach .ics file for clients that don't support inline calendar
                byte[] icsBytes = Encoding.UTF8.GetBytes(icsContent);
                Attachment icsAttachment = new Attachment(
                    new MemoryStream(icsBytes),
                    "invite.ics",
                    "text/calendar"
                );
                mailMessage.Attachments.Add(icsAttachment);

                // Add recipients
                if (_asBcc)
                {
                    mailMessage.To.Add(_from);
                    foreach (string destination in to)
                    {
                        if (!string.IsNullOrEmpty(destination))
                            mailMessage.Bcc.Add(destination);
                    }
                }
                else
                {
                    foreach (string destination in to)
                    {
                        if (!string.IsNullOrEmpty(destination))
                            mailMessage.To.Add(destination);
                    }
                }

                // Headers for better compatibility
                mailMessage.Headers.Add("Content-class", "urn:content-classes:calendarmessage");
                mailMessage.Headers.Add("X-Priority", "Normal");
                mailMessage.Headers.Add("X-MSMail-Priority", "Normal");

                SmtpClient client = new(_host, _port)
                {
                    EnableSsl = _enableSSL,
                    Credentials = credentialsAvailable ? new NetworkCredential(_user, _password) : null,
                    UseDefaultCredentials = !credentialsAvailable
                };

                client.SendAsync(mailMessage, null);
            }
        }
    }
}