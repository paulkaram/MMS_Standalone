using Intalio.Tools.Common.Storage;
using MMS.DAL.Enumerations;

namespace MMS.BLL.Storage
{
	public class StorageFactory
    {
        public static string[] SupportedExtensionsForViewer = new string[] { ".docx", ".doc", ".pdf", ".png", ".jpg", ".jpeg", ".pptx", ".ppt" };

        private readonly StorageSettings _storageSettings;

        public StorageFactory(StorageSettings storageSettings)
        {
            _storageSettings = storageSettings;
		}

        public IStorage GetStorage()
        {
            return new FileSystemStorage(_storageSettings.FileSystem!.Path);
        }

        public static string GetMeetingDirectory(int meetingId)
        {
            return $"Meetings/{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}/{meetingId}/";
        }

		public string GetMeetingMinutesTemplateDirectory(AttachmentRecordTypeDbEnum attachmentRecordTypeDbEnum)
        {
            return attachmentRecordTypeDbEnum switch
            {
                AttachmentRecordTypeDbEnum.FinalMeetingMinutes => _storageSettings.FileSystem!.FinalMeetingMinutesTemplatePath,
                _ => _storageSettings.FileSystem!.InitialMeetingMinutesTemplatePath
            };
        }

		public static string GetMeetingMinutesDirectory(int meetingId)
		{
			return $"MeetingsMinutes/{meetingId}/";
		}

		public static string GetFinalMeetingMinutesDirectory(int meetingId)
		{
			return $"FinalMeetingsMinutes/{meetingId}/";
		}

		public static string GetMeetingMinutesSignatureDirectory(int meetingId)
		{
			return $"MeetingMinutesSignature/{meetingId}/";
		}

		public static string GetRecommendationDirectory(int recommendationId)
		{
			return $"Recommendations/{recommendationId}/";
		}

        public static string GetCommitteeCreationFilesDirectory(int committeeId)
        {
            return $"CommitteeCreation/{committeeId}/";
        }

        public static string GetCommitteeFilesDirectory(int committeeId)
        {
            return $"Committee/{committeeId}/";
        }

        public static string GetSessionDirectory(int sessionId)
        {
            return $"Sessions/{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}/{sessionId}/";
        }

        public static string GetBidDirectory(int bidId)
        {
            return $"Bids/{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}/{bidId}/";
        }

        public string GetProfilePictureDirectory(string UserId)
		{
			return $"ProfilePictures/{UserId}/";
		}
	}
}
