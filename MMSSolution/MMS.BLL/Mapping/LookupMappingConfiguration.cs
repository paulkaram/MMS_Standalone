using Mapster;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.CommitteeRoles;
using MMS.DTO.DataSources;
using MMS.DTO.Roles;

namespace MMS.BLL.Mapping
{
    internal class LookupMappingConfiguration : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {

            config.NewConfig<Language, ListItemDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name);


            config.NewConfig<DataSource, ListItemDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Dbname);


            config.NewConfig<DataSource, DataSourceListItemDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.DbName, src => src.Dbname)
                .Map(dest => dest.Username, src => src.Username)
                .Map(dest => dest.InstanceName, src => src.InstanceName);

            config.NewConfig<Permission, ListItemDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name);

            config.NewConfig<(Role role, LanguageDbEnum Language), ListItemDto>()
                .Map(dest => dest.Id, src => src.role.Id)
                .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.role.RoleNameAr : src.role.RoleNameEn);

            config.NewConfig<(CommitteeType committeeType, LanguageDbEnum Language), ListItemDto>()
              .Map(dest => dest.Id, src => src.committeeType.Id)
              .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.committeeType.NameAr : src.committeeType.NameEn);

            config.NewConfig<RoleType, ListItemDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name);

            config.NewConfig<AccountType, ListItemDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name);


            config.NewConfig<Role, RoleListItemDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.RoleNameAr, src => src.RoleNameAr)
                .Map(dest => dest.RoleNameEn, src => src.RoleNameEn)
                .Map(dest => dest.TypeId, src => src.TypeId);

            config.NewConfig<CommitteeRole, CommitteeRoleListItemDto>()
              .Map(dest => dest.Id, src => src.Id)
              .Map(dest => dest.NameAr, src => src.NameAr)
              .Map(dest => dest.NameEn, src => src.NameEn);

            config.NewConfig<(Stamp stamp, LanguageDbEnum Language), ListItemDto>()
            .Map(dest => dest.Id, src => src.stamp.Id)
            .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.stamp.StampAr : src.stamp.StampEn);

            config.NewConfig<(Committee committee, LanguageDbEnum Language), ListItemDto>()
              .Map(dest => dest.Id, src => src.committee.Id)
              .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.committee.NameAr : src.committee.NameEn);

            config.NewConfig<(CommitteeRole committeeRole, LanguageDbEnum Language), ListItemDto>()
            .Map(dest => dest.Id, src => src.committeeRole.Id)
            .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.committeeRole.NameAr : src.committeeRole.NameEn);

            config.NewConfig<(VotingType votingType, LanguageDbEnum Language), ListItemDto>()
           .Map(dest => dest.Id, src => src.votingType.Id)
           .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.votingType.NameAr : src.votingType.NameEn);

            config.NewConfig<(MeetingStatus meetingStatus, LanguageDbEnum Language), ListItemDto>()
          .Map(dest => dest.Id, src => src.meetingStatus.Id)
          .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.meetingStatus.NameAr : src.meetingStatus.NameEn);

            config.NewConfig<(MeetingType meetingType, LanguageDbEnum Language), ListItemDto>()
         .Map(dest => dest.Id, src => src.meetingType.Id)
         .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.meetingType.NameAr : src.meetingType.NameEn);

            config.NewConfig<(MeetingAgendaRecommendationStatus status, LanguageDbEnum Language), ListItemDto>()
          .Map(dest => dest.Id, src => src.status.Id)
          .Map(dest => dest.Name, src =>
                        src.Language == LanguageDbEnum.Arabic ? src.status.NameAr : src.status.NameEn);

            config.NewConfig<(Priority priority, LanguageDbEnum Language), ListItemDto>()
          .Map(dest => dest.Id, src => src.priority.Id)
          .Map(dest => dest.Name, src =>
                        src.Language == LanguageDbEnum.Arabic ? src.priority.NameAr : src.priority.NameEn);

            config.NewConfig<Meeting, ListItemDto>()
             .Map(dest => dest.Id, src => src.Id)
             .Map(dest => dest.Name, src => src.Title);

            config.NewConfig<(Branch branch, LanguageDbEnum Language), ListItemDto>()
          .Map(dest => dest.Id, src => src.branch.Id)
          .Map(dest => dest.Name, src =>
                        src.Language == LanguageDbEnum.Arabic ? src.branch.NameAr : src.branch.NameEn);

            config.NewConfig<(Privacy privacy, LanguageDbEnum Language), ListItemDto>()
          .Map(dest => dest.Id, src => src.privacy.Id)
          .Map(dest => dest.Name, src =>
                        src.Language == LanguageDbEnum.Arabic ? src.privacy.NameAr : src.privacy.Name);

            config.NewConfig<(CommitteePermission committeePermission, LanguageDbEnum Language), ListItemDto>()
          .Map(dest => dest.Id, src => src.committeePermission.CommitteeId)
          .Map(dest => dest.Name, src =>
                        src.Language == LanguageDbEnum.Arabic ? src.committeePermission.Committee.NameAr : src.committeePermission.Committee.NameEn);
            config.NewConfig<(CouncilSession session, LanguageDbEnum Language), ListItemDto>()
          .Map(dest => dest.Id, src => src.session.Id)
          .Map(dest => dest.Name, src =>
                        src.Language == LanguageDbEnum.Arabic ? src.session.NameAr : src.session.NameEn);

            config.NewConfig<(CommitteeClassification classification, LanguageDbEnum Language), ListItemDto>()
          .Map(dest => dest.Id, src => src.classification.Id)
          .Map(dest => dest.Name, src =>
                        src.Language == LanguageDbEnum.Arabic ? src.classification.NameAr : src.classification.NameEn);

            config.NewConfig<(CommitteeStyle committeeStyle, LanguageDbEnum Language), ListItemDto>()
                      .Map(dest => dest.Id, src => src.committeeStyle.Id)
                      .Map(dest => dest.Name, src =>
                                    src.Language == LanguageDbEnum.Arabic ? src.committeeStyle.NameAr : src.committeeStyle.NameEn);

            config.NewConfig<(CommitteeStatus committeeStatus, LanguageDbEnum Language), ListItemDto>()
                      .Map(dest => dest.Id, src => src.committeeStatus.Id)
                      .Map(dest => dest.Name, src =>
                                    src.Language == LanguageDbEnum.Arabic ? src.committeeStatus.NameAr : src.committeeStatus.NameEn);

        }

    }
}
