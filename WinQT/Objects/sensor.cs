using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WinQT.Objects
{
    internal class sensor
    {
        /*[JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("unique_id")]
        public string UniqueId { get; set; }

        [JsonProperty("device")]
        public object Device { get; set; }

        [JsonProperty("identifiers")]
        public string[] Identifiers { get; set; }

        [JsonProperty("state_topic")]
        public string StateTopic { get; set; }

        [JsonProperty("unit_of_measurement")]
        public string UnitOfMeasurement { get; set; }

        [JsonProperty("value_template")]
        public string ValueTemplate { get; set; }

        [JsonProperty("availability")]
        public Availability[] Availability { get; set; }

        [JsonProperty("payload_available")]
        public string PayloadAvailable { get; set; }

        [JsonProperty("payload_not_available")]
        public string PayloadNotAvailable { get; set; }*/
    }

    /*public partial class Availability
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
    }

    public partial class sensor
    {
        public static sensor FromJson(string json) => JsonConvert.DeserializeObject<sensor>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this sensor self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }*/
}
