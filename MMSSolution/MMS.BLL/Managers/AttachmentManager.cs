using Intalio.Tools.Common;
using Intalio.Tools.Common.Extensions.FileExtensions;
using Intalio.Tools.Common.Extensions.StringExtensions;
using Intalio.Tools.Common.FileKit;
using Intalio.Tools.Common.Objects;
using Intalio.Tools.Common.Storage;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MMS.BLL.Storage;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using Newtonsoft.Json;
using Path = System.IO.Path;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
	public class AttachmentManager
	{
		private readonly IMapper _mapper;
		private const string EXTRA_STRING = "_vP_";
		private readonly IProcessUnitOfWork _processUnitOfWork;
		private readonly StorageManager _storageManager;
		private readonly IConfiguration _configuration;
		private readonly IMMSUnitOfWork _mmsUnitOfWork;

		public AttachmentManager(IProcessUnitOfWork processUnitOfWork,
			IMapper mapper,
			StorageManager storageManager,
			IConfiguration configuration,
			IMMSUnitOfWork mmsUnitOfWork)
		{
			_processUnitOfWork = processUnitOfWork;
			_mapper = mapper;
			_configuration = configuration;
			_mmsUnitOfWork = mmsUnitOfWork;
			_storageManager= storageManager;
		}

		public async Task<(string? Filename, byte[]? Bytes)> GetAttachment(int attachmentId, int activityId, string fullName)
		{
			var attachment = await _processUnitOfWork.Attachments.GetAsync(x => x.Id == attachmentId);
			var annotations = await ListPreviousAnnotations(attachmentId, activityId);
			if (attachment != null && !attachment.Deleted)
			{
				string extension = Path.GetExtension(attachment.FileName)??"";
				bool isDocumentSupportedByViewer = StorageFactory.SupportedExtensionsForViewer.Any(x => x == extension.ToLower());
				var bytes = await _storageManager.GetFileForViewer(Path.GetExtension(attachment.FileName),
					fullName, isDocumentSupportedByViewer, annotations,
					attachmentId: attachmentId,
					fileWebRelativeUrl: attachment.FileRelativeUrl);

				return (
					Filename: attachment.FileName,
					Bytes: bytes
			   );
			}

			return (null, null);
		}

		/// <summary>
		/// Gets the raw attachment bytes without any processing (for conversion purposes)
		/// </summary>
		public async Task<(string? Filename, byte[]? Bytes)> GetAttachmentById(int attachmentId)
		{
			var attachment = await _processUnitOfWork.Attachments.GetAsync(x => x.Id == attachmentId);
			if (attachment != null && !attachment.Deleted)
			{
				var bytes = await _storageManager.GetFile(attachmentId, attachment.FileRelativeUrl);
				return (
					Filename: attachment.FileName,
					Bytes: bytes
				);
			}
			return (null, null);
		}

		public async Task<string?> RegenerateAttchmentQuery(int attachmentId, string userId, string actions, bool isSigned, int activityId)
		{
			var attachment = await _processUnitOfWork.Attachments.GetAsync(x => x.Id == attachmentId);
			var attachmentAction = GetAttachmentActions(actions);
			//the user can sign only one time, if he signed the attachement, the sign action should be disabled, else, keep it as it is
			attachmentAction.Sign = isSigned ? false : true;
			attachmentAction.RemoveSign =  false;
			return await GetAttachmentQuery(attachment, userId, attachmentAction, activityId);
		}

		public AttachmentActionDto GetAttachmentActions(string? actions)
		{
			//string decyptedActions = EncryptionService.Decrypt(actions);
			string decyptedActions = Base64UrlEncoder.Decode(actions ?? string.Empty);
			return AttachmentActionDto.LoadFromQuery(decyptedActions);
		}

		public async Task<string?> GetAttachmentQuery(int attachmentId, int recordId, AttachmentRecordTypeDbEnum attachmentTypeId, string userId, int activityId)
		{
			var attachment = await _processUnitOfWork.Attachments.GetAsync(x => x.Id == attachmentId && x.RecordId == recordId && x.RecordTypeId == (int)attachmentTypeId);
			return await GetAttachmentQuery(attachment, userId, new(), activityId);
		}

		public async Task<string?> GetAttachmentQuery(int attachmentId, AttachmentRecordTypeDbEnum attachmentTypeId, string userId, int activityId)
		{
			var attachment = await _processUnitOfWork.Attachments.GetAsync(x => x.Id == attachmentId && x.RecordTypeId == (int)attachmentTypeId);
			return await GetAttachmentQuery(attachment, userId, new(), activityId);
		}

		public async Task<string?> GetAttachmentQuery(int recordId, AttachmentRecordTypeDbEnum attachmentTypeId, string userId, AttachmentActionDto attachmentAction, int activityId)
		{
			var attachment = await _processUnitOfWork.Attachments.GetAsync(x => x.RecordId == recordId && x.RecordTypeId == (int)attachmentTypeId);
			return await GetAttachmentQuery(attachment, userId, attachmentAction, activityId);
		}

		public async Task<string?> GetAttachmentQuery(int attachmentId, string userId, AttachmentActionDto attachmentAction, int activityId)
		{
			var attachment = await _processUnitOfWork.Attachments.GetAsync(x => x.Id == attachmentId);
			if (attachment.RecordTypeId == (int)AttachmentRecordTypeDbEnum.FinalMeetingMinutes)
			{
				attachmentAction = await GetFinalMeetingMinutesAttachmentActions(attachment, userId);
			}
			return await GetAttachmentQuery(attachment, userId, attachmentAction, activityId);
		}

		private async Task<AttachmentActionDto> GetFinalMeetingMinutesAttachmentActions(Attachment attachment, string userId)
		{
			var actions = new AttachmentActionDto();
			var task = await _mmsUnitOfWork.Tasks.GetAsync(x =>
									x.TypeId == (int)TaskTypeDbEnum.SignFinalMeetingMinutes &&
									x.UserId == userId &&
									x.MeetingId == attachment.RecordId &&
									x.StatusId == (int)TaskStatusDbEnum.PendingApproval);
			if (task != null)
			{
				var signedAlready = await _processUnitOfWork.AttachmentsSignatures.AnyAsync(x => x.UserId == userId && x.AttachmentId == attachment.Id);
				actions.Sign = !signedAlready;
				actions.RemoveSign = signedAlready; // Allow unsign if already signed
			}
			return actions;
		}

		public async Task<List<StampDto>?> ListCurrentAnnotations(int att, int activityId, string userId)
		{
			List<StampDto>? annotations = null;
			var attachmentAnnotations = await _processUnitOfWork.AttachmentAnnotations.ListAsync(x => x.AttachmentId == att && x.ActivityId == activityId && x.UserId == userId);
			if (attachmentAnnotations.Any())
			{
				annotations = new();
				attachmentAnnotations.Select(x => x.Annotation).ToList().ForEach(x =>
				{
					annotations.Add(JsonConvert.DeserializeObject<StampDto>(x));
				});
			}
			return annotations;
		}

		public async Task<List<StampDto>?> ListPreviousAnnotations(int att, int activityId)
		{
			List<StampDto>? annotations = null;

			// Get all annotations for this attachment
			var allAnnotationsForAttachment = await _processUnitOfWork.AttachmentAnnotations.ListAsync(x =>
				x.AttachmentId == att &&
				x.Annotation != null);

			// Filter: include annotations from OTHER activities, PLUS all signature annotations from ANY activity
			var filteredAnnotations = allAnnotationsForAttachment.Where(x =>
			{
				if (x.Annotation == null) return false;

				// Check if this is a signature annotation
				bool isSignature = x.Annotation.Contains("\"StampType\":23") ||
								   x.Annotation.Contains("\"stampType\":23") ||
								   x.Annotation.Contains("\"AnnotationType\":23") ||
								   x.Annotation.Contains("\"annotationType\":23");

				// Include signatures from any activity, other annotations only from previous activities
				return isSignature || x.ActivityId != activityId;
			}).ToList();

			if (filteredAnnotations.Any())
			{
				annotations = new();
				filteredAnnotations.Select(x => x.Annotation).ToList().ForEach(x =>
				{
					if (x != null)
					{
						var stamp = JsonConvert.DeserializeObject<StampDto>(x);
						if (stamp != null)
						{
							annotations.Add(stamp);
						}
					}
				});
			}
			return annotations;
		}

		public async Task AddAnnotationAsync(int att, StampDto stamp, string userId, int activityId)
		{
			string annotation = JsonConvert.SerializeObject(stamp);
			AttachmentAnnotation attachmentAnnotation = new();
			attachmentAnnotation.Annotation = annotation;
			attachmentAnnotation.AttachmentId = att;
			attachmentAnnotation.UserId = userId;
			attachmentAnnotation.ActivityId = activityId;

			await _processUnitOfWork.AttachmentAnnotations.AddAsync(attachmentAnnotation);
			await _processUnitOfWork.SaveChangesAsync();
		}

		public async Task<bool> ValidateToken(int attachmentId, string userId, string token, string hashValidation, string? actions, bool deleteToken = true)
		{
			var viewerToken = await _mmsUnitOfWork.ViewerTokens.GetAsync(x => x.Token == token && x.Username == userId.ToString());

			if (viewerToken == null && deleteToken)
			{
				//if during the validation, the token should be deleted, but it is not found, then the validation fails
				return false;
			}

			if (viewerToken != null && deleteToken)
			{
				//if the token is found, and it should be deleted, then delete the token and continue with the validation
				_mmsUnitOfWork.ViewerTokens.Remove(viewerToken);
				await _mmsUnitOfWork.SaveChangesAsync();
			}

			var attachment = await _processUnitOfWork.Attachments.GetAsync(x => x.Id == attachmentId);
			string extension = Path.GetExtension(attachment?.FileName) ?? "";
			bool isDocumentSupportedByViewer = StorageFactory.SupportedExtensionsForViewer.Any(x => x == extension.ToLower());

			// NCA Compliance: Use SHA-384 for hash validation (NCS-1:2020 Section 4.1)
			string hash = StringManipulation.SHA_384(
			   attachmentId.ToString(),
			   userId.ToString(),
			   token,
			   isDocumentSupportedByViewer.ToString(),
			   actions ?? string.Empty,
			   EXTRA_STRING);

			return hash == hashValidation;

		}

		private async Task<string?> GetAttachmentQuery(Attachment? attachment, string userId, AttachmentActionDto attachmentAction, int task)
		{
			if (attachment != null && !attachment.Deleted)
			{
				string extension = Path.GetExtension(attachment.FileName ?? string.Empty);
				bool isDocumentSupportedByViewer = StorageFactory.SupportedExtensionsForViewer.Any(x => x == extension.ToLower());
				string supportedByViewerQueryString = isDocumentSupportedByViewer ? "render=true" : "norender=true";

				ViewerToken token = new ViewerToken
				{
					Token = Guid.NewGuid().ToString(),
					Username = userId.ToString()
				};

				await _mmsUnitOfWork.ViewerTokens.AddAsync(token);
				await _mmsUnitOfWork.SaveChangesAsync();

				//string actions = EncryptionService.Encrypt(attachmentAction.ToQueryString()).Trim();
				string actions = Base64UrlEncoder.Encode(attachmentAction.ToQueryString()).Trim();
				// NCA Compliance: Use SHA-384 for hash generation (NCS-1:2020 Section 4.1)
				string hash = StringManipulation.SHA_384(
					attachment.Id.ToString(),
					userId.ToString(),
					token.Token,
					isDocumentSupportedByViewer.ToString(),
					actions,
					EXTRA_STRING);
				return $"att={attachment.Id}&task={task}&tk={token.Token}&hvd={hash}&act={actions}&{supportedByViewerQueryString}";
			}

			return null;
		}

		private AttachmentVersion UpgradeToNewVersion(Attachment currentAttachment, string? newFilename, string userId)
		{
			AttachmentVersion attachmentVersion = new AttachmentVersion()
			{
				AttachementId = currentAttachment.Id,
				CreatedBy = currentAttachment.CreatedBy,
				CreatedDate = currentAttachment.CreatedDate,
				FileName = currentAttachment.FileName??"",
				FileRelativeUrl = currentAttachment.FileRelativeUrl,
				Version = currentAttachment.Version,
			};

			string baseFileRelativeUrl = attachmentVersion.FileRelativeUrl.Contains("_v_") ? attachmentVersion.FileRelativeUrl.Split("_v_")[0] : attachmentVersion.FileRelativeUrl;

			currentAttachment.FileName = newFilename ?? currentAttachment.FileName;
			currentAttachment.Version += 1;
			currentAttachment.FileRelativeUrl = baseFileRelativeUrl + "_v_" + currentAttachment.Version;
			currentAttachment.CreatedDate = DateTime.Now;

			return attachmentVersion;
		}

		public async Task RemoveCurrentStamps(int activityId, string userId)
		{
			var currentUserStamps = await _processUnitOfWork.AttachmentAnnotations.ListAsync(x => x.UserId == userId && x.ActivityId == activityId);
			_processUnitOfWork.AttachmentAnnotations.RemoveRange(currentUserStamps);
			await _processUnitOfWork.SaveChangesAsync();
		}

		public async Task<bool> RemoveByByAttachmentIdAsync(int attachmentId, string userId)
		{
			var attachment = await _processUnitOfWork.Attachments.GetAsync(x => x.Id == attachmentId&&x.CreatedBy== userId);
			if (attachment != null)
			{
				attachment.Deleted = true;
				return await _processUnitOfWork.SaveChangesAsync()>0;
			}
			return false;
		}

		public (string? Filename, byte[]? Bytes) GetUserGuideAttachment()
		{
			string? userguideStorePath = _configuration.GetValue<string>(Constants.AppSettingsConstants.UserGuideStorePath);

			if (userguideStorePath != null)
			{
				byte[] byteArray = File.ReadAllBytes(userguideStorePath);
				return (
					Filename: Path.GetFileName(userguideStorePath),
					Bytes: byteArray
			   );
			}

			return (null, null);
		}

		
		public async Task<bool> SignFinalMeetingMinutesAttachmentAsync(int att, StampDto signature, string userId, int activityId)
		{
			var attachment = await _processUnitOfWork.Attachments.GetAsync(x => x.Id == att);
			var stampData = signature.StampData.GetFromCleanBase64();

			// Validate basic requirements
			if (attachment == null || stampData == null || signature.Rect == null)
			{
				return false;
			}

			// Ensure StampType is set for proper detection
			signature.StampType = (int)AnnotationTypeEnum.Signature;
			signature.AnnotationType = AnnotationTypeEnum.Signature;

			// Store signature as annotation (not burned immediately) - allows unsign
			string annotationJson = JsonConvert.SerializeObject(signature);
			AttachmentAnnotation signatureAnnotation = new()
			{
				Annotation = annotationJson,
				AttachmentId = att,
				UserId = userId,
				ActivityId = activityId
			};
			await _processUnitOfWork.AttachmentAnnotations.AddAsync(signatureAnnotation);

			// Track that user has signed - this is checked during approval
			await _processUnitOfWork.AttachmentsSignatures.AddAsync(new AttachmentsSignature() { UserId = userId, AttachmentId = attachment.Id });

			var signs = await _processUnitOfWork.UserSignatures.ListWithTrackAsync(x => x.UserId == userId);
			foreach (var sign in signs)
			{
				sign.LastSuccessfulAttempt = null;
			}

			await _processUnitOfWork.SaveChangesAsync();
			return true;
		}

		public async Task<bool> UnsignFinalMeetingMinutesAttachmentAsync(int att, string userId, int activityId)
		{
			var attachment = await _processUnitOfWork.Attachments.GetAsync(x => x.Id == att);
			if (attachment == null || attachment.RecordTypeId != (int)AttachmentRecordTypeDbEnum.FinalMeetingMinutes)
			{
				return false;
			}

			// Check if user has signed
			var signatureRecord = await _processUnitOfWork.AttachmentsSignatures.GetAsync(x => x.UserId == userId && x.AttachmentId == att);
			if (signatureRecord == null)
			{
				return false; // User hasn't signed
			}

			// Remove signature annotations - check for signature type (23) with case-insensitive search
			// Also don't filter by activityId since it might have been stored with a different value
			var allUserAnnotations = await _processUnitOfWork.AttachmentAnnotations.ListAsync(x =>
				x.AttachmentId == att &&
				x.UserId == userId &&
				x.Annotation != null);

			// Filter to only signature annotations (StampType = 23)
			var signatureAnnotations = allUserAnnotations.Where(x =>
				x.Annotation != null &&
				(x.Annotation.Contains("\"StampType\":23") ||
				 x.Annotation.Contains("\"stampType\":23") ||
				 x.Annotation.Contains("\"AnnotationType\":23") ||
				 x.Annotation.Contains("\"annotationType\":23"))).ToList();

			if (signatureAnnotations.Any())
			{
				_processUnitOfWork.AttachmentAnnotations.RemoveRange(signatureAnnotations);
			}

			// Remove signature tracking record
			_processUnitOfWork.AttachmentsSignatures.Remove(signatureRecord);

			await _processUnitOfWork.SaveChangesAsync();
			return true;
		}

		public async Task<bool> CheckFinalMeetingMinutesSigned(int attachmentId, string userId)
		{
			return await _processUnitOfWork.AttachmentsSignatures.AnyAsync(x=>x.UserId == userId&&x.AttachmentId== attachmentId);
		}

		public async Task<bool> CheckUserAccess(int attachmentId, string userId)
		{
			//TODO revire code and remove dubplicate logic for performance enhancments
			var attachment = await _processUnitOfWork.Attachments.Find(attachmentId);

			if (attachment == null)
			{
				return false;
			}
			// Check if the user is the creator
			if (attachment.CreatedBy == userId)
			{
				return true;
			}

			// Check if the user has any tasks associated with the attachment
			var hasTask = await _mmsUnitOfWork.Tasks.AnyAsync(x =>
				x.UserId == userId &&
				x.AttachmentId == attachmentId);

			if (hasTask)
			{
				return true;
			}

			// For MOM attachments, check if user has pending approval/sign tasks for the meeting
			if (attachment.RecordTypeId == (int)AttachmentRecordTypeDbEnum.InitialMeetingMinutes ||
				attachment.RecordTypeId == (int)AttachmentRecordTypeDbEnum.FinalMeetingMinutes)
			{
				var hasMomTask = await _mmsUnitOfWork.Tasks.AnyAsync(x =>
					x.UserId == userId &&
					x.MeetingId == attachment.RecordId &&
					(x.TypeId == (int)TaskTypeDbEnum.InitialMeetingMinutesApproval ||
					 x.TypeId == (int)TaskTypeDbEnum.SignFinalMeetingMinutes));

				if (hasMomTask)
				{
					return true;
				}
			}
			int? committeeId=null ;
			if (attachment.RecordTypeId == (int)AttachmentRecordTypeDbEnum.Meeting||
                attachment.RecordTypeId == (int)AttachmentRecordTypeDbEnum.InitialMeetingMinutes||
                attachment.RecordTypeId == (int)AttachmentRecordTypeDbEnum.FinalMeetingMinutes)
			{
                committeeId =(await _mmsUnitOfWork.Meetings.Find(attachment.RecordId)).CommitteeId;

            }
            else if (attachment.RecordTypeId == (int)AttachmentRecordTypeDbEnum.MeetingAgenda){
				var meetingId = (await _mmsUnitOfWork.MeetingAgendas.Find(attachment.RecordId)).MeetingId;
                committeeId = (await _mmsUnitOfWork.Meetings.Find(meetingId)).CommitteeId;

            }else if(attachment.RecordTypeId == (int)AttachmentRecordTypeDbEnum.AgendaRecommendation)
			{
				var recmmendationCreator = ((await _mmsUnitOfWork.MeetingAgendaRecommendations.Find(attachment.RecordId)).CreateBy == userId);
				if (recmmendationCreator) return true;
                var agendaId = (await _mmsUnitOfWork.MeetingAgendaRecommendations.Find(attachment.RecordId)).MeetingAgendaId;
                committeeId = (await _mmsUnitOfWork.Meetings.GetAsync(x=>x.MeetingAgenda.Any(x=>x.Id== agendaId))).CommitteeId;
            }
            

            if (committeeId != null)
            {
                var hasCommitteePermission = await _mmsUnitOfWork.CommitteePermissions.AnyAsync(x =>
                   x.UserId == userId &&
                   x.CommitteeId == committeeId &&
                   x.PermissionId == (int)CommitteePermissionDbEnum.CommitteeMeetingAttachments);

                hasCommitteePermission = await _mmsUnitOfWork.PermissionMatrices.AnyAsync(p => p.UserId == userId && p.PermissionId == (int)PermissionDbEnum.SuperAdmin);

                if (!hasCommitteePermission)
                {
                    var classificationId = (await _mmsUnitOfWork.Committees.Find(committeeId.GetValueOrDefault())).CommitteeClassificationId;
                    if (classificationId != null)
                    {
                        hasCommitteePermission = await _mmsUnitOfWork.PermissionMatrices.AnyAsync(x => x.UserId == userId && x.Permission.MapId == classificationId);

                    }
                }
				if (hasCommitteePermission) return true; 

            }
            bool hasAccess = false;
			// Check access based on RecordTypeId
			switch (attachment.RecordTypeId)
			{
				
				case (int)AttachmentRecordTypeDbEnum.Meeting:
					hasAccess = await _mmsUnitOfWork.Meetings.AnyAsync(x => x.Id == attachment.RecordId && x.IsCommittee == true && x.Committee != null &&
									x.Committee.CommitteePermissions.Any(p => p.UserId == userId && p.PermissionId == (int)CommitteePermissionDbEnum.CommitteeMeetingAttachments));
					if (hasAccess) return true;

					return await _mmsUnitOfWork.MeetingAttendees.AnyAsync(x => x.UserId == userId&&x.MeetingId==attachment.RecordId);


				case (int)AttachmentRecordTypeDbEnum.MeetingAgenda:
					hasAccess = await _mmsUnitOfWork.MeetingAgendas.AnyAsync(x => x.Id == attachment.RecordId && x.Meeting.IsCommittee == true && x.Meeting.Committee != null &&
									x.Meeting.Committee.CommitteePermissions.Any(p => p.UserId == userId && p.PermissionId == (int)CommitteePermissionDbEnum.CommitteeMeetingAttachments));
					if (hasAccess) return true;
					return await _mmsUnitOfWork.MeetingAgendas.AnyAsync(x =>
						x.Id == attachment.RecordId &&
						x.Meeting.MeetingAttendees.Any(att => att.UserId == userId));
				case (int)AttachmentRecordTypeDbEnum.InitialMeetingMinutes:
				case (int)AttachmentRecordTypeDbEnum.FinalMeetingMinutes:
					hasAccess = await _mmsUnitOfWork.Meetings.AnyAsync(x => x.Id == attachment.RecordId && x.IsCommittee == true && x.Committee != null &&
									x.Committee.CommitteePermissions.Any(p => p.UserId == userId && p.PermissionId == (int)CommitteePermissionDbEnum.CommitteeMeetingMinutes));
					if (hasAccess) return true;
					return await _mmsUnitOfWork.MeetingAgendas.AnyAsync(x =>
						x.Id == attachment.RecordId &&
						x.Meeting.MeetingAttendees.Any(att => att.UserId == userId));

				default:
					return false;
			}
		}

	}
}
