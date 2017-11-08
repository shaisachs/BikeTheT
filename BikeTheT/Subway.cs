using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.ComponentModel;

namespace BikeTheT
{
    public static class Subway
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

        [FunctionName("Subway")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/subwayLines/{color}")]
            HttpRequestMessage req,
            string color,
            TraceWriter log)
        {
            // TODO: validate input
            // TODO: move serialization into service
            // TODO: enum for colors
            // TODO: consolidate with commuter rail?
            // TODO: OpenAPI Spec
            // TODO: functional tests
            // move auth error into functional tests

            var authError = AuthSvc.GetAuthenticationError(req);
            if (authError != null)
            {
                return authError;
            }

            var colorCleaned = color.ToLower();
            string directionCleaned = GetRequestParam<string>(req, "direction").ToLower();
            DateTime timeOfTravel = GetRequestParam<DateTime>(req, "timeOfTravel");
            bool? isWeekend = GetRequestParam<bool?>(req, "isWeekend");

            var answer = BikeTheTSvc.BikesAllowedOnSubway(colorCleaned, directionCleaned, timeOfTravel, isWeekend);

            return CreateResponse(colorCleaned, directionCleaned, timeOfTravel, isWeekend, answer);
        }

        private static T GetRequestParam<T>(HttpRequestMessage req, string paramName) 
        {
            string val = req.GetQueryNameValuePairs()
              .FirstOrDefault(q => string.Compare(q.Key, paramName, true) == 0)
              .Value;

            return val.Convert<T>();
        }

        private static T Convert<T>(this string input)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    return (T)converter.ConvertFromString(input);
                }
                return default(T);
            }
            catch (NotSupportedException)
            {
                return default(T);
            }
        }

        private static HttpResponseMessage CreateResponse(string color, string direction, DateTime timeOfTravel, bool? isWeekend, bool bikesAllowed)
        {
            var answer = new SubwayDto()
            {
                Color = color,
                Direction = direction,
                TimeOfTravel = timeOfTravel,
                IsWeekend = isWeekend,
                BikesAllowed = bikesAllowed
            };

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

    public class SubwayDto
    {
        public string Color { get; set; }
        public string Direction { get; set; }
        public DateTime TimeOfTravel { get; set; }
        public bool? IsWeekend { get; set; }
        public bool BikesAllowed { get; set; }
    }
}
