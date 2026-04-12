using MMS.DTO.LogActivity;

namespace MMS.BLL.Common.Logging
{
    interface IWrapLogHelper
    {
        CustomObjectForLogActivity GetCustomObjectForLog();
    }
}
