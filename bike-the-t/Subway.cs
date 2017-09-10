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
        [FunctionName("Subway")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "subwayLines/{color}")] HttpRequestMessage req,
            string color,
            TraceWriter log)
        {
            // TODO: validate input
            // TODO: move logic into service
            // TODO: move serialization into service
            // TODO: enum for colors
            // TODO: consolidate with commuter rail?
            // TODO: OpenAPI Spec
            // TODO: functional tests

            string direction = GetRequestParam<string>(req, "direction");
            DateTime timeOfTravel = GetRequestParam<DateTime>(req, "timeOfTravel");
            bool? isWeekend = GetRequestParam<bool?>(req, "isWeekend");

            var colorCleaned = color.ToLower();

            if (colorCleaned.Equals("green") || colorCleaned.Equals("mattapan"))
            {
                return CreateResponse(color, direction, timeOfTravel, isWeekend, false);
            }

            if ((isWeekend.HasValue && isWeekend.Value) || IsWeekend(timeOfTravel))
            {
                return CreateResponse(color, direction, timeOfTravel, isWeekend, false);
            }

            var relevantEmbargoedHours = EmbargoedHours.Where(e => e.Item1.Equals(colorCleaned) && e.Item2.Equals(direction));

            var bikesAllowed = relevantEmbargoedHours == null ||
                relevantEmbargoedHours.All(t => !IsInTimeRange(timeOfTravel, t.Item3, t.Item4));
            return CreateResponse(color, direction, timeOfTravel, isWeekend, bikesAllowed);
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


        private static bool IsWeekend(DateTime timeOfTravel)
        {
            return (timeOfTravel.DayOfWeek == DayOfWeek.Saturday) || (timeOfTravel.DayOfWeek == DayOfWeek.Sunday);
        }

        private static bool IsInTimeRange(DateTime timeOfTravel, int startHour, int endHour)
        {
            return startHour <= timeOfTravel.Hour && timeOfTravel.Hour <= endHour;
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

        private static IList<Tuple<string, string, int, int>> EmbargoedHours = new Tuple<string, string, int, int>[]
        {
            new Tuple<string, string, int, int>("blue", "inbound", 7, 9),
            new Tuple<string, string, int, int>("blue", "outbound", 16,18),
            new Tuple<string, string, int, int>("red", "inbound", 7, 10),
            new Tuple<string, string, int, int>("red", "inbound", 16, 19),
            new Tuple<string, string, int, int>("red", "outbound", 7, 10),
            new Tuple<string, string, int, int>("red", "outbound", 16, 19),
            new Tuple<string, string, int, int>("orange", "inbound", 7, 10),
            new Tuple<string, string, int, int>("orange", "inbound", 16, 19),
            new Tuple<string, string, int, int>("orange", "outbound", 7, 10),
            new Tuple<string, string, int, int>("orange", "outbound", 16, 19)
        }.ToList();
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
