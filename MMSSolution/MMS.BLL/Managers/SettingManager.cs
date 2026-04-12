using DocumentFormat.OpenXml.Spreadsheet;
using Intalio.Tools.Common.Extensions.FileExtensions;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MMS.BLL.Constants;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.AppSettings;
using MMS.DTO.Meetings;
using Intalio.Tools.Common.Extensions;

namespace MMS.BLL.Managers
{
    public class SettingManager
    {
        private readonly ISettingsUnitOfWork _settingsUnitOfWork;
        private readonly IMMSUnitOfWork _mmsUnitOfWork;
        private readonly StorageManager _storageManager;
        private readonly ConfigurationManager _configuration;

        private readonly IMapper _mapper;

        public SettingManager(ISettingsUnitOfWork settingsUnitOfWork, IMapper mapper, IMMSUnitOfWork mMSUnitOfWork, StorageManager storageManager, ConfigurationManager configuration)
        {
            _settingsUnitOfWork = settingsUnitOfWork;
            _mapper = mapper;
            _mmsUnitOfWork = mMSUnitOfWork;
            _storageManager= storageManager;
            _configuration = configuration;
        }

        public async Task<string?> GetApplicationName()
        {
            var settings = await _settingsUnitOfWork.AppSettings.GetAsync(x => x.Name == AppSettingsConstants.ApplicationName);
            return settings?.Value;
        }

        public async Task<ApplicationDictionaryDto> GetApplicationDictionary()
        {
            var dictionary = await _settingsUnitOfWork.Dictionary.ListAsync();

            var groupedByKeyword = dictionary.GroupBy(x => x.Keyword);

            return new ApplicationDictionaryDto(
                Ar: groupedByKeyword.ToDictionary(g => g.Key, g => g.First().Ar),
                En: groupedByKeyword.ToDictionary(g => g.Key, g => g.First().En)
            );
        }

        public async Task<ThemeColorsDto?> ListThemeColorsAsync()
        {
            var themeColors = await _settingsUnitOfWork.AppSettings.ListAsync(x => x.Category == "THEME");
            return new ThemeColorsDto
            {
                PrimaryColor = themeColors.FirstOrDefault(x => x.Name == "PrimaryColor")?.Value,
                SecondaryColor = themeColors.FirstOrDefault(x => x.Name == "SecondaryColor")?.Value,
                NavigationColor = themeColors.FirstOrDefault(x => x.Name == "NavigationColor")?.Value,
                ErrorColor = themeColors.FirstOrDefault(x => x.Name == "ErrorColor")?.Value,
            };
        }

        public async Task<bool> UpdateThemeColorsAsync(ThemeColorsDto themeColors)
        {
            var existingColors = await _settingsUnitOfWork.AppSettings.ListWithTrackAsync(x => x.Category == "THEME");
            if (existingColors != null)
            {
                existingColors.FirstOrDefault(x => x.Name == "PrimaryColor").Value = themeColors.PrimaryColor;
                existingColors.FirstOrDefault(x => x.Name == "SecondaryColor").Value = themeColors.SecondaryColor;
                existingColors.FirstOrDefault(x => x.Name == "NavigationColor").Value = themeColors.NavigationColor;
                return await _settingsUnitOfWork.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<List<SettingsListItemDto>?> ListAsync()
        {
            var settings = await _settingsUnitOfWork.AppSettings.ListAsync();
            return settings.GroupBy(x => x.Category, (key, group) => new SettingsListItemDto
            {
                CategoryName = key,
                Items = group.Select(g => new SettingsListItemSubDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Value = g.Value
                }).ToList(),
            }).ToList();
        }

        public async Task<bool> UpdateSettingsAsync(SettingsListItemDto setting)
        {
            if (setting.Items != null)
            {
                var itemIds = setting.Items.Select(x => x.Id);
                var items = await _settingsUnitOfWork.AppSettings.ListWithTrackAsync(x => itemIds.Contains(x.Id));
                foreach (var item in items)
                {
                    var incomingItem = setting.Items.FirstOrDefault(x => x.Id == item.Id);
                    item.Value = incomingItem != null ? incomingItem.Value : null;
                }

                return await _settingsUnitOfWork.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<bool> AddVotingType(VotingTypeListItemDto votingType)
        {
            var vote = _mapper.Map<VotingType>(votingType);
            await _settingsUnitOfWork.VotingTypes.AddAsync(vote);
            return await _settingsUnitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateVotingType(VotingTypeListItemDto votingType)
        {
            if (votingType.Id == 0) return false;
            var vote = _mapper.Map<VotingType>(votingType);
            _settingsUnitOfWork.VotingTypes.Update(vote);
            return await _settingsUnitOfWork.SaveChangesAsync() > 0;
        }
        public async Task<List<VotingTypeListItemDto>?> ListVotingTypes()
        {
            var types = await _settingsUnitOfWork.VotingTypes.ListAsync();
            return types.Select(x => _mapper.Map<VotingTypeListItemDto>(x)).ToList();
        }

        public async Task<(bool success, string Message)> DeleteVotingType(int votingTypeId, LanguageDbEnum language)
        {
            var votingType = await _settingsUnitOfWork.VotingTypes.GetAsync(x => x.Id == votingTypeId);
            if (votingType != null)
            {
                if (await _mmsUnitOfWork.MeetingAgendas.AnyAsync(x => x.VotingTypeId == votingTypeId))
                {
                    var related = await _settingsUnitOfWork.Dictionary.GetAsync(
                         x => x.Keyword == DictionaryConstansts.DeleteFailedRelated);
                    return (false, language == LanguageDbEnum.Arabic ? related.Ar : related.En);

                }
                _settingsUnitOfWork.VotingTypes.Remove(votingType);
                await _settingsUnitOfWork.SaveChangesAsync();
                return (true, "");
            }
            var notFound = await _settingsUnitOfWork.Dictionary.GetAsync(x => x.Keyword == DictionaryConstansts.NotFound);

            return (false, language == LanguageDbEnum.Arabic ? notFound.Ar : notFound.En);
        }

        public async Task<List<VotingOptionsListItemDto>> ListVotingTypesOptions(int votingTypeId)
        {
            var options = await _settingsUnitOfWork.VotingTypeOptions.ListAsync(x => x.VotingTypeId == votingTypeId);
            return options.Select(x => _mapper.Map<VotingOptionsListItemDto>(x)).ToList();
        }

        public async Task<bool> AddVotingTypeOption(VotingOptionsListItemDto votingOption)
        {
            var option = _mapper.Map<VotingOption>(votingOption);
            await _settingsUnitOfWork.VotingTypeOptions.AddAsync(option);
            return await _settingsUnitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<(bool success, string Message)> DeleteVotingTypeOption(int votingTypeOptionId, LanguageDbEnum language)
        {
            var option = await _settingsUnitOfWork.VotingTypeOptions.GetAsync(x => x.Id == votingTypeOptionId);
            if (option != null)
            {
                if (await _mmsUnitOfWork.MeetingUserVotes.AnyAsync(x => x.VottingOptionId == votingTypeOptionId))
                {
                    var related = await _settingsUnitOfWork.Dictionary.GetAsync(
                         x => x.Keyword == DictionaryConstansts.DeleteFailedRelated);
                    return (false, language == LanguageDbEnum.Arabic ? related.Ar : related.En);

                }
                _settingsUnitOfWork.VotingTypeOptions.Remove(option);
                await _settingsUnitOfWork.SaveChangesAsync();
                return (true, "");

            }
            var notFound = await _settingsUnitOfWork.Dictionary.GetAsync(x => x.Keyword == DictionaryConstansts.NotFound);
            return (false, language == LanguageDbEnum.Arabic ? notFound.Ar : notFound.En);
        }

        public async Task<bool> UpdateVotingTypeOption(VotingOptionsListItemDto votingOption)
        {
            if (votingOption.Id == 0) return false;
            var option = _mapper.Map<VotingOption>(votingOption);
            _settingsUnitOfWork.VotingTypeOptions.Update(option);
            return await _settingsUnitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<List<SettingsListItemSubDto>?> ListSystemTemplates()
        {
            var appsettings = (await _settingsUnitOfWork.AppSettings.ListAsync(x => x.Name.ToLower().Contains("template")));
            return appsettings.Select(x => new SettingsListItemSubDto()
            {
                Id = x.Id,
                Name = x.Name,
                Value = x.Value,

            }).ToList();
        }

        public async Task<bool> UpdateSystemTemplate(int appSettingId, IFormFileCollection files)
        {
            try
            {
                var appSetting = await _settingsUnitOfWork.AppSettings.Find(appSettingId);
                if (appSetting != null)
                {
                    var templateAttachmentId = int.Parse(appSetting.Value);
                    var templateFile = files[0];

                    return  await _storageManager.UpdateDatabaseAttachment(templateFile.ToBytes(), templateAttachmentId);
                   


                }
            }
            catch (Exception ex)
            {
            }

            return false;
        }

        public bool ReloadSystemSettings()
        {
            try
            {
                //_configuration.Sources.Clear();
                string? dbConfigurationConnectionString = _configuration.GetConnectionString("MMS");

                if (!string.IsNullOrWhiteSpace(dbConfigurationConnectionString))
                {
                    _configuration.Sources.Clear();
                    _configuration.AddDbConfiguration(dbConfigurationConnectionString);
                }
            }
            catch (Exception ex) {
                return false;
            }
            return true;

        }
    }
}
