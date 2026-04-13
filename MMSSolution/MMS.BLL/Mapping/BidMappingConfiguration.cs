using Mapster;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO.Bids;

namespace MMS.BLL.Mapping
{
    internal class BidMappingConfiguration : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<(Bid bid, LanguageDbEnum Language, DateTime Now), BidDto>()
                .Map(dest => dest.Id, src => src.bid.Id)
                .Map(dest => dest.CommitteeId, src => src.bid.CommitteeId)
                .Map(dest => dest.CommitteeName, src => src.bid.Committee != null
                    ? (src.Language == LanguageDbEnum.Arabic ? src.bid.Committee.NameAr : src.bid.Committee.NameEn)
                    : null)
                .Map(dest => dest.ReferenceNumber, src => src.bid.ReferenceNumber)
                .Map(dest => dest.ExternalMeetingNumber, src => src.bid.ExternalMeetingNumber)
                .Map(dest => dest.Subject, src => src.bid.Subject)
                .Map(dest => dest.Description, src => src.bid.Description)
                .Map(dest => dest.TeamLeaderUserId, src => src.bid.TeamLeaderUserId)
                .Map(dest => dest.TeamLeaderName, src => src.bid.TeamLeader != null
                    ? (src.Language == LanguageDbEnum.Arabic ? src.bid.TeamLeader.FullnameAr : src.bid.TeamLeader.FullnameEn)
                    : null)
                .Map(dest => dest.StatusId, src => src.bid.StatusId)
                .Map(dest => dest.StatusName, src => src.bid.Status != null
                    ? (src.Language == LanguageDbEnum.Arabic ? src.bid.Status.NameAr : src.bid.Status.NameEn)
                    : null)
                .Map(dest => dest.StatusStepOrder, src => src.bid.Status != null ? src.bid.Status.StepOrder : 0)
                .Map(dest => dest.StartDate, src => src.bid.StartDate)
                .Map(dest => dest.DueDate, src => src.bid.DueDate)
                .Map(dest => dest.MeetingId, src => src.bid.MeetingId)
                .Map(dest => dest.InitialMinutesPath, src => src.bid.InitialMinutesPath)
                .Map(dest => dest.FinalMinutesPath, src => src.bid.FinalMinutesPath)
                .Map(dest => dest.CreatedBy, src => src.bid.CreatedBy)
                .Map(dest => dest.CreatedByName, src => src.bid.CreatedByNavigation != null
                    ? (src.Language == LanguageDbEnum.Arabic ? src.bid.CreatedByNavigation.FullnameAr : src.bid.CreatedByNavigation.FullnameEn)
                    : null)
                .Map(dest => dest.CreatedDate, src => src.bid.CreatedDate)
                .Map(dest => dest.StakeholdersCount, src => src.bid.Stakeholders.Count)
                .Map(dest => dest.ItemsCount, src => src.bid.Items.Count)
                .Map(dest => dest.IsOverdue, src => src.bid.DueDate < src.Now
                    && src.bid.StatusId != (int)BidStatusDbEnum.Completed
                    && src.bid.StatusId != (int)BidStatusDbEnum.Returned);

            config.NewConfig<(BidStakeholder stakeholder, LanguageDbEnum Language), BidStakeholderDto>()
                .Map(dest => dest.Id, src => src.stakeholder.Id)
                .Map(dest => dest.BidId, src => src.stakeholder.BidId)
                .Map(dest => dest.UserId, src => src.stakeholder.UserId)
                .Map(dest => dest.ExternalMemberId, src => src.stakeholder.ExternalMemberId)
                .Map(dest => dest.Name, src => src.stakeholder.User != null
                    ? (src.Language == LanguageDbEnum.Arabic ? src.stakeholder.User.FullnameAr : src.stakeholder.User.FullnameEn)
                    : src.stakeholder.ExternalMember != null
                        ? (src.Language == LanguageDbEnum.Arabic ? src.stakeholder.ExternalMember.FullnameAr : src.stakeholder.ExternalMember.FullnameEn)
                        : string.Empty)
                .Map(dest => dest.Email, src => src.stakeholder.User != null
                    ? src.stakeholder.User.Email
                    : src.stakeholder.ExternalMember != null ? src.stakeholder.ExternalMember.Email : null)
                .Map(dest => dest.IsTeamLeader, src => src.stakeholder.IsTeamLeader)
                .Map(dest => dest.IsExternal, src => src.stakeholder.ExternalMemberId != null);

            config.NewConfig<(BidStatusHistory history, LanguageDbEnum Language), BidStatusHistoryDto>()
                .Map(dest => dest.Id, src => src.history.Id)
                .Map(dest => dest.BidId, src => src.history.BidId)
                .Map(dest => dest.FromStatusId, src => src.history.FromStatusId)
                .Map(dest => dest.FromStatusName, src => src.history.FromStatus != null
                    ? (src.Language == LanguageDbEnum.Arabic ? src.history.FromStatus.NameAr : src.history.FromStatus.NameEn)
                    : null)
                .Map(dest => dest.ToStatusId, src => src.history.ToStatusId)
                .Map(dest => dest.ToStatusName, src => src.Language == LanguageDbEnum.Arabic
                    ? src.history.ToStatus.NameAr
                    : src.history.ToStatus.NameEn)
                .Map(dest => dest.ChangedBy, src => src.history.ChangedBy)
                .Map(dest => dest.ChangedByName, src => src.history.ChangedByNavigation != null
                    ? (src.Language == LanguageDbEnum.Arabic ? src.history.ChangedByNavigation.FullnameAr : src.history.ChangedByNavigation.FullnameEn)
                    : string.Empty)
                .Map(dest => dest.ChangedDate, src => src.history.ChangedDate)
                .Map(dest => dest.Note, src => src.history.Note);

            config.NewConfig<BidStatus, BidStatusDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.NameAr, src => src.NameAr)
                .Map(dest => dest.NameEn, src => src.NameEn)
                .Map(dest => dest.StepOrder, src => src.StepOrder);
        }
    }
}
