using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace Intalio.Tools.Common.Sms
{
    public class SmsService
    {
        private readonly string _message = "";
        private readonly string _sender = "";
        private readonly string _smsBearer = "";
        private readonly string _smsApi = "";

        public SmsService(string message, string sender, string smsBearer, string smsApi)
        {
            _message = message;
            _sender = sender;
            _smsBearer = smsBearer;
            _smsApi = smsApi;
        }

        public async Task<bool> SendSmsAsync(params string[] recipients)
        {
            try
            {
                string body = System.Web.HttpUtility.UrlEncode(_message);
                StringContent content = new StringContent(JsonConvert.SerializeObject(new
                {
                    recipients,
                    body,
                    sender = _sender
                }), Encoding.UTF8, "application/json");

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.Expect100Continue = true;

                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _smsBearer);
                HttpResponseMessage response = await client.PostAsync(_smsApi, content);
                string responseContent = await response.Content.ReadAsStringAsync();
                var parsedResponse = JObject.Parse(responseContent);
                return ((dynamic)parsedResponse).statusCode == "201"
                    || ((dynamic)parsedResponse).StatusCode == "201";
            }
            catch
            {
                return false;
            }
        }
    }
}
