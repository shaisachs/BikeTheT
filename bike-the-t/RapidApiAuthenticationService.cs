using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Configuration;

namespace BikeTheT
{
    public class RapidApiAuthenticationService
    {
        private const string RapidApiConfigSetting = "RapidApiSecret";
        private const string RapidApiCustomHeader = "X-Mashape-Proxy-Sec";

        public void AuthenticateOrInvalidate(HttpRequestMessage req)
        {
            var secretInRequest = GetRapidApiSecretInRequest(req);
            var correctSecret = GetCorrectRapidApiSecret();

            if (string.IsNullOrEmpty(correctSecret))
            {
                var responseMsg = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                responseMsg.Content = new StringContent("No secret available");
                throw new HttpResponseException(responseMsg);
            }

            if (!correctSecret.Equals(secretInRequest))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            return;
        }

        private string GetRapidApiSecretInRequest(HttpRequestMessage req)
        {
            var answer = string.Empty;

            if (req.Headers.Contains(RapidApiCustomHeader))
            {
                IEnumerable<string> headerValues = req.Headers.GetValues(RapidApiCustomHeader);
                answer = headerValues.FirstOrDefault();
            }

            return answer;
        }

        private string GetCorrectRapidApiSecret()
        {
            return ConfigurationSettings.AppSettings.Get(RapidApiConfigSetting);
        }
    }
}
