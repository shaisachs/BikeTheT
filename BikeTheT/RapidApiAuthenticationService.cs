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

        /// <summary>
        /// Determine if there is an authentication error, and if so return it.
        /// </summary>
        /// <param name="req">The incoming request</param>
        /// <returns>500 status code if no secret is configured
        /// 401 status code if the consumer provides the wrong credentials
        /// null if the user provided correct credentials</returns>
        public HttpResponseMessage GetAuthenticationError(HttpRequestMessage req)
        {
            var secretInRequest = GetRapidApiSecretInRequest(req);
            var correctSecret = GetCorrectRapidApiSecret();

            if (string.IsNullOrEmpty(correctSecret))
            {
                return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "No secret available");
            }

            if (!correctSecret.Equals(secretInRequest))
            {
                return req.CreateErrorResponse(HttpStatusCode.Unauthorized, "Credentials are not valid");
            }

            return null;
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
            return ConfigurationManager.AppSettings.Get(RapidApiConfigSetting);
        }
    }
}
