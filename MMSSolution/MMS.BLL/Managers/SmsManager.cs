using Intalio.Tools.Common;
using Intalio.Tools.Common.Extensions.StringExtensions;
using Intalio.Tools.Common.Sms;
using Microsoft.Extensions.Configuration;
using MMS.BLL.Constants;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Models.MMS;
using System.Security.Cryptography;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
    public class SmsManager
    {
        private readonly IProcessUnitOfWork _processUnitOfWork;
        private readonly IUserManagementUnitOfWork _userManagementUnitOfWork;
        private readonly SmsSettings _smsSettings;
        private readonly bool SmsNotificationEnabled;

        public SmsManager(IConfiguration configuration, IProcessUnitOfWork processUnitOfWork, IUserManagementUnitOfWork userManagementUnitOfWork)
        {
            _processUnitOfWork = processUnitOfWork;
            _userManagementUnitOfWork = userManagementUnitOfWork;
            _smsSettings = configuration.GetSection(AppSettingsConstants.SmsSectionName).Get<SmsSettings>() ?? new();
            SmsNotificationEnabled = configuration.GetValue<bool>(AppSettingsConstants.EnableEmailNotification);
        }

        public async Task<(bool success, string validation)> SendOtpAsync(string mobile)
        {
            // NCA Compliance: Use cryptographically secure random number generator (NCS-1:2020 Section 7.1)
            string code = RandomNumberGenerator.GetInt32(0, 10000).ToString("D4");
            var smsWrapper = new SmsService(code, _smsSettings.Sender, _smsSettings.Bearer, _smsSettings.Api);
            string hashForValidation = StringManipulation.SHA_384(code, mobile, DateTime.Now.ToString("yyyyMMdd"));
            bool success = await smsWrapper.SendSmsAsync(mobile);
            return (success, hashForValidation);
        }

        public async Task SendSms(int completedActivityInstanceId)
        {
            // CaseManagement dependency removed - this method previously used _caseUnitOfWork to resolve activity/workflow instances
            await Task.CompletedTask;
        }
    }
}
