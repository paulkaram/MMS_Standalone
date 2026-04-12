using DocumentFormat.OpenXml.Wordprocessing;
using Intalio.Tools.Common.Enumerations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.API.Common.Hubs;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.CommitteePermissions;
using MMS.DTO.Committees;
using MMS.DTO.CouncilCommittees;
using MMS.DTO.Meetings;
using MMS.DTO.Permissions;
using MMS.DTO.Users;

namespace MMS.API.Controllers
{
    [Route("api/councilCommittees")]
    [ApiController]
    public class CouncilCommitteesController : IntalioBaseController
    {
        private readonly CouncilCommitteeManager _councilCommitteeManager;
        public CouncilCommitteesController(CouncilCommitteeManager councilCommitteeManager)
        {
            _councilCommitteeManager = councilCommitteeManager;
        }

        [HttpGet]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListCommittees()
        {
            try
            {
                var committees = await _councilCommitteeManager.ListCommitteesAsync(base.Language);

                return Ok(new ApiResponseDto<List<CommitteeListItemDto>>(committees));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPost]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> AddCommittee([FromForm]  CommitteeDto committee)
        {
            try
            {
                FileStatusEnum status = FileStatusEnum.Valid;
                IFormCollection formCollection = await Request.ReadFormAsync();
                if (formCollection.Files.Count > 0)
                {
                    (status, _) = base.AreValidAttachments(formCollection.Files);

                }

                if (status == FileStatusEnum.Valid)
                {
                    await _councilCommitteeManager.AddCommitteeAsync(committee, formCollection.Files,UserId);

                }
                else
                {
                    return Ok(new ApiResponseDto<bool>(Success: false, Message: status.GetDisplayName()));
                }

                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }
        [HttpPut("{councilCommitteeId}")]
        [RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Write)]
        public async Task<IActionResult> UpdateCommittee(int councilCommitteeId, [FromForm] CommitteeDto committee)
        {
            try
            {
                FileStatusEnum status = FileStatusEnum.Valid;
                IFormCollection formCollection = await Request.ReadFormAsync();
                if (formCollection.Files.Count > 0)
                {
                    (status, _) = base.AreValidAttachments(formCollection.Files);

                }

                if (status == FileStatusEnum.Valid)
                {
                    await _councilCommitteeManager.UpdateAsync(councilCommitteeId, committee, formCollection.Files,UserId);


                }
                else
                {
                    return Ok(new ApiResponseDto<bool>(Success: false, Message: status.GetDisplayName()));
                }
                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("my-organization")]
        public async Task<IActionResult> ListMyOrganization(bool onlyActive = true)
        {
            try
            {
                var adminResult = await _councilCommitteeManager.GetUserAdminCommitteesAsync(UserId);
                var organization = await _councilCommitteeManager.ListCommitteeStructuresForUserAsync(Language, onlyActive, adminResult.CommitteeIds);

                return Ok(new ApiResponseDto<List<TreeviewListItemDto>>(organization));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("organization")]
		[RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListOrganization(bool onlyActive = true)
        {
            try
            {
                var organization = await _councilCommitteeManager.ListCommitteeStructuresAsync(Language, onlyActive);

                return Ok(new ApiResponseDto<List<TreeviewListItemDto>>(organization));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("{councilCommitteeId}")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetCommittee(int councilCommitteeId)
        {
            try
            {
                var councilCommittee= await _councilCommitteeManager.GetAsync(councilCommitteeId);

                return Ok(new ApiResponseDto<CommitteeDto>(councilCommittee));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("{councilCommitteeId}/users")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListUsersInCommittee(int councilCommitteeId)
        {
            try
            {
                var users = await _councilCommitteeManager.ListUsersInCommitteeAsync(councilCommitteeId, Language);

                return Ok(new ApiResponseDto<List<UserCommitteeListItemDto>>(users));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }
		[HttpGet("{councilCommitteeId}/user-permissions/{userId}")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListUsersPermissionsInCommittee(int councilCommitteeId,string userId)
		{
			try
			{
				var permissions = await _councilCommitteeManager.ListUsersPermissionsInCommitteeAsync(councilCommitteeId,userId);

				return Ok(new ApiResponseDto<PermissionListItemDto>(permissions));
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpPut("{councilCommitteeId}/user-permissions")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> UpdateUsersPermissionsInCommittee(int councilCommitteeId, CommitteePermissionPutDto committeePermissionPutDto)
		{
			try
			{
				var updated = await _councilCommitteeManager.UpdateUserPermissionsInCommitteeAsync(councilCommitteeId, committeePermissionPutDto);

				return Ok(new ApiResponseDto<bool>(updated));
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpPut("{councilCommitteeId}/update-user")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> UpdateUserInCommittee(UpdateCommitteeUserDto updateCommitteeUserDto,int councilCommitteeId)
		{
			try
			{
				bool updated = await _councilCommitteeManager.UpdateCommitteeUser(updateCommitteeUserDto, councilCommitteeId);

				return Ok(new ApiResponseDto<bool>(updated,Success: updated));
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpGet("{councilCommitteeId}/parents")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommitteesGeneralInfo, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetCommitteeParents(int councilCommitteeId)
		{
			try
			{
				var parents = await _councilCommitteeManager.GetCommitteeParents(councilCommitteeId, Language);

				return Ok(new ApiResponseDto<List<ListItemDto>>(parents));
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpGet("{councilCommitteeId}/{page}/{pageSize}/users-general-info")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommitteesGeneralInfo, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListUsersInCommittee(int councilCommitteeId,int page,int pageSize)
		{
			try
			{
				bool hasAccess =await _councilCommitteeManager.HasCommitteeAccess(councilCommitteeId, UserId, CommitteePermissionDbEnum.CommitteeUsers);
				if (hasAccess)
				{
					var users = await _councilCommitteeManager.ListUsersInCommitteeForGeneralInfoAsync(councilCommitteeId, page, pageSize, Language);
					return Ok(new ApiResponseDto<GenericPaginationListDto<UserCommitteeListItemDto>>(users));
				}
				return NoContent();
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

        [HttpGet("{councilCommitteeId}/{page}/{pageSize}/activities-general-info")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommitteesGeneralInfo, PermissionLevelDbEnum.Read)]
        public async Task<IActionResult> ListActivitiesInCommittee(int councilCommitteeId, int page, int pageSize)
        {
            try
            {
                bool hasAccess = await _councilCommitteeManager.HasCommitteeAccess(councilCommitteeId, UserId, CommitteePermissionDbEnum.CommitteeActivities);
                if (hasAccess)
                {
                    var activities = await _councilCommitteeManager.ListActivitiesInCommitteeForGeneralInfoAsync(councilCommitteeId, page, pageSize, Language);
                    return Ok(new ApiResponseDto<GenericPaginationListDto<ActivitiesCommitteeListItemDto>>(activities));
                }
                return NoContent();

            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("{councilCommitteeId}/{page}/{pageSize}/attachments-general-info")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommitteesGeneralInfo, PermissionLevelDbEnum.Read)]
        public async Task<IActionResult> ListAttachmentsInCommittee(int councilCommitteeId, int page, int pageSize)
        {
            try
            {
                bool hasAccess = await _councilCommitteeManager.HasCommitteeAccess(councilCommitteeId, UserId, CommitteePermissionDbEnum.CommitteeAttachments);
                if (hasAccess)
                {
                    var attachments = await _councilCommitteeManager.ListAttachmentsInCommitteeForGeneralInfoAsync(councilCommitteeId, page, pageSize, Language);
                    return Ok(new ApiResponseDto<GenericPaginationListDto<AttachmentListItemDto>>(attachments));
                }
                return NoContent();

            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("{councilCommitteeId}/{page}/{pageSize}/tasks")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommitteesGeneralInfo, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListCommitteeTasks(int councilCommitteeId, int page, int pageSize)
		{
			try
			{
				bool hasAccess = await _councilCommitteeManager.HasCommitteeAccess(councilCommitteeId, UserId, CommitteePermissionDbEnum.CommitteeRecommendation);
				if (hasAccess)
				{
					var tasks = await _councilCommitteeManager.ListCommitteeTasks(councilCommitteeId, page, pageSize, Language);

					return Ok(new ApiResponseDto<GenericPaginationListDto<MeetingAgendaRecommendationFollowUpListItemDto>>(tasks));
				}
				return NoContent();
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpGet("{councilCommitteeId}/{page}/{pageSize}/meetings")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommitteesGeneralInfo, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListCommitteeMeetings(int councilCommitteeId, int page, int pageSize)
		{
			try
			{
				bool hasAccess = await _councilCommitteeManager.HasCommitteeAccess(councilCommitteeId, UserId, CommitteePermissionDbEnum.CommitteeMeetings);
				if (hasAccess)
				{
					var meetings = await _councilCommitteeManager.ListCommitteeMeetings(councilCommitteeId, page, pageSize, Language);

					return Ok(new ApiResponseDto<GenericPaginationListDto<MeetingListItemDto>>(meetings));
				}
				return NoContent();
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}



        [HttpPut("duty/{committeeDutyId}")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> UpdateCommitteeDuty(int committeeDutyId, CommitteeDutyDto committeeDutyObj)
        {
            try
            {
                await _councilCommitteeManager.UpdateDutyAsync(committeeDutyId, committeeDutyObj);

                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPost("add-user")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> AddUserToCommittee(CommitteeUserPostDro committeeUserPostDro)
        {
            try
            {
                (bool added , string Message)=await _councilCommitteeManager.AddUserToCommitteeAsync(committeeUserPostDro, Language);

                return Ok(new ApiResponseDto<bool>(added,Success: added,Message: Message));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPost("{councilCommitteeId}/duty")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> AddCommitteeDuty(int councilCommitteeId, CommitteeDutyDto CommitteeDutyObj)
        {
            try
            {
                await _councilCommitteeManager.AddCommitteeDutyAsync(councilCommitteeId, CommitteeDutyObj);

                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPost("{councilCommitteeId}/activity")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Write)]
        public async Task<IActionResult> AddCommitteeActivity(int councilCommitteeId, CommitteeDutyDto CommitteeDutyObj)
        {
            try
            {
                await _councilCommitteeManager.AddCommitteeActivityAsync(councilCommitteeId, CommitteeDutyObj);

                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPut("activity/{committeeActivityId}")]
        [RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Write)]
        public async Task<IActionResult> UpdateCommitteeActivity(int committeeActivityId, CommitteeDutyDto committeeDutyObj)
        {
            try
            {
                await _councilCommitteeManager.UpdateActivityAsync(committeeActivityId, committeeDutyObj);

                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("{councilCommitteeId}/duties")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListCommitteeDuties(int councilCommitteeId)
        {
            try
            {
              var committeeDuties =  await _councilCommitteeManager.ListCommitteeDutiesAsync(councilCommitteeId);

                return Ok(new ApiResponseDto<List<CommitteeDutyListItemDto>>(committeeDuties));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("{councilCommitteeId}/Activities")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Read)]
        public async Task<IActionResult> listCommitteeActivities(int councilCommitteeId)
        {
            try
            {
                var committeeDuties = await _councilCommitteeManager.ListCommitteeActivitiesAsync(councilCommitteeId);

                return Ok(new ApiResponseDto<List<CommitteeDutyListItemDto>>(committeeDuties));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("user-committees")]
		public async Task<IActionResult> ListUserCommittees()
        {
            try
            {
                var userCommittees = await _councilCommitteeManager.ListUserCommitteesAsync(UserId, Language);

                return Ok(new ApiResponseDto<List<ListItemDto>>(userCommittees));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }
		[HttpGet("meeting-user-committees")]
		public async Task<IActionResult> ListUserCommitteesForMeeting()
		{
			try
			{
				var userCommittees = await _councilCommitteeManager.ListUserCommitteesForMeeting(UserId, Language);

				return Ok(new ApiResponseDto<List<CommitteeListItemDto>>(userCommittees));
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpDelete("{councilCommitteeId}/user/{userId}")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> RemoveUserFromCommittee(int councilCommitteeId, string userId)
        {
            try
            {
                await _councilCommitteeManager.RemoveUserFromCommitteeAsync(councilCommitteeId, userId);

                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpDelete("duty/{dutyId}")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> RemoveCommitteeDuty(int dutyId)
        {
            try
            {
                await _councilCommitteeManager.RemoveCommitteeDutyAsync(dutyId);

                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }
        [HttpDelete("activity/{activityId}")]
        [RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Write)]
        public async Task<IActionResult> RemoveCommitteeActivity(int activityId)
        {
            try
            {
                await _councilCommitteeManager.RemoveCommitteeActivityAsync(activityId);

                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPost("user-committees/general-info")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommitteesGeneralInfo, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> listUserCouncilsAndCommitteesForGeneralInfo([FromBody] CouncilsAndCommitteesSearchCriteriaPostDto searchCriteria)
		{
			try
			{
				var userCommittees = await _councilCommitteeManager.listUserCouncilsAndCommitteesForGeneralInfo(UserId, Language, searchCriteria);

				return Ok(new ApiResponseDto<List<ComitteesGeneralInfoListItemDto>>(userCommittees));
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpGet("{councilCommitteeId}/user-committees")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommitteesGeneralInfo, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> listUserCommitteesByCouncilForGeneralInfo(int councilCommitteeId)
		{
			try
			{
				var userCommittees = await _councilCommitteeManager.ListUserCommitteesByCouncil(councilCommitteeId,UserId, Language);

				return Ok(new ApiResponseDto<List<ComitteesGeneralInfoListItemDto>>(userCommittees));
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpGet("{councilCommitteeId}/my-permissions")]
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommitteesGeneralInfo, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetMyCommitteePermissions(int councilCommitteeId)
		{
			try
			{
				var userPermissions = await _councilCommitteeManager.GetUserCommitteePermission(councilCommitteeId, UserId);

				return Ok(new ApiResponseDto<UserCommitteePermissions>(userPermissions));
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpGet("user-committees-list")]
		//Moi Embedded Service
		[RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListUserCommitteesForListing()
		{
			try
			{
				var userCommittees = await _councilCommitteeManager.ListUserCommitteesForListingAsync(UserId, Language);

				return Ok(new ApiResponseDto<List<CommitteeListItemDto>>(userCommittees));
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
        [HttpGet("{councilCommitteeId}/attachments")]
        [RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Read)]
        public async Task<IActionResult> ListCommitteeAttachments(int councilCommitteeId)
        {
            try
            {
                var parents = await _councilCommitteeManager.ListCommitteeAttachments(councilCommitteeId, Language);

                return Ok(new ApiResponseDto<List<AttachmentListItemDto>>(parents));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }
        [HttpPost("{councilCommitteeId}/attachment")]
        [RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Write)]
        public async Task<IActionResult> AddMeetingAttachment(int councilCommitteeId, [FromQuery] short privacyId)
        {
            try
            {
                    FileStatusEnum status = FileStatusEnum.Valid;
                    IFormCollection formCollection = await Request.ReadFormAsync();
                    if (formCollection.Files.Count > 0)
                    {
                        (status, _) = base.AreValidAttachments(formCollection.Files);

                    }
                    if (status == FileStatusEnum.Valid)
                    {
                        var atachments = await _councilCommitteeManager.AddCommitteeAttachments(councilCommitteeId, formCollection.Files, UserId, privacyId, Language);

                        return Ok(new ApiResponseDto<List<AttachmentListItemDto>>(atachments));
                    }
                    else
                    {
                        return Ok(new ApiResponseDto<bool>(Success: false, Message: status.GetDisplayName()));
                    }
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        [HttpDelete("{AttachmentId}/attachments")]
        [RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Read)]
        public async Task<IActionResult> DeleteCommitteeAttachments(int AttachmentId)
        {
            try
            {
                var deleted= await _councilCommitteeManager.DeleteCommitteeAttachments(AttachmentId);

                return Ok(new ApiResponseDto<bool>(deleted));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPost("{councilCommitteeId}/Financial-Compensation")]
        [RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Write)]
        public async Task<IActionResult> SaveFinancialCompensation(int councilCommitteeId, [FromQuery] bool hasFinancialCompensation)
        {
            try
            {
                var hasFinancialComp = await _councilCommitteeManager.SaveFinancialCompensation(councilCommitteeId, hasFinancialCompensation);

                return Ok(new ApiResponseDto<bool>(hasFinancialComp));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("{councilCommitteeId}/Committee-Financial-Compensation")]
        [RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Read)]
        public async Task<IActionResult> GetCommitteeFinancialCompensation(int councilCommitteeId)
        {
            try
            {
                var hasFinancialComp = await _councilCommitteeManager.GetCommitteeFinancialCompensationAsync(councilCommitteeId);

                return Ok(new ApiResponseDto<bool>(hasFinancialComp));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("my-admin-committees")]
        public async Task<IActionResult> GetMyAdminCommittees()
        {
            try
            {
                var result = await _councilCommitteeManager.GetUserAdminCommitteesAsync(UserId);

                return Ok(new ApiResponseDto<UserAdminCommitteesDto>(result));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("user-admin-committees/{userId}")]
        [RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Read)]
        public async Task<IActionResult> GetUserAdminCommittees(string userId)
        {
            try
            {
                var result = await _councilCommitteeManager.GetUserAdminCommitteesAsync(userId);

                return Ok(new ApiResponseDto<UserAdminCommitteesDto>(result));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPost("update-bulk-admin")]
        [RequiredPermission(PermissionDbEnum.CouncilsAndCommittees, PermissionLevelDbEnum.Write)]
        public async Task<IActionResult> UpdateBulkCommitteeAdmin([FromBody] BulkCommitteeAdminDto dto)
        {
            try
            {
                var updated = await _councilCommitteeManager.UpdateBulkCommitteeAdminAsync(dto);

                return Ok(new ApiResponseDto<bool>(updated, Success: updated));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }
    }
}
