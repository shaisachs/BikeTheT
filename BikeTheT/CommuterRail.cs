using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace BikeTheT
{
    public static class CommuterRail
    {
        private static BikeTheTService _bikeTheTSvc;
        private static BikeTheTService BikeTheTSvc
        {
            get
            {
                if (_bikeTheTSvc == null)
                {
                    _bikeTheTSvc = new BikeTheTService();
                }
                return _bikeTheTSvc;
            }
        }

        private static RapidApiAuthenticationService _authSvc;
        private static RapidApiAuthenticationService AuthSvc
        {
            get
            {
                if (_authSvc == null)
                {
                    _authSvc = new RapidApiAuthenticationService();
                }
                return _authSvc;
            }
        }

        [FunctionName("CommuterRail")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "commuterRailTrains/{trainNum}")]
            HttpRequestMessage req,
            string trainNum, TraceWriter log)
        {
            AuthSvc.AuthenticateOrInvalidate(req);

            var bikesAllowed = BikeTheTSvc.BikesAllowedOnCommuterRailTrain(trainNum);

            var answer = new CommuterRailDto() { TrainNum = trainNum, BikesAllowed = bikesAllowed };

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(answer, jsonSerializerSettings), Encoding.UTF8, "application/json")
            };
        }

    }

    public class CommuterRailDto
    {
        public string TrainNum { get; set; }
        public bool BikesAllowed { get; set; }
    }
}
