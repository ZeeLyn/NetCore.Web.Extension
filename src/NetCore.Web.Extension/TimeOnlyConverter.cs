
#if NET6_0

using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonConverter = Newtonsoft.Json.JsonConverter;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace NetCore.Web.Extension
{
    public class TimeOnlyConverter : JsonConverter
    {
        private readonly string _format;

        public TimeOnlyConverter(string format = "HH:mm:ss")
        {
            _format = format;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ((TimeOnly)value).ToString(_format));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jt = JToken.ReadFrom(reader);
            return TimeOnly.Parse(jt.ToString());
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(TimeOnly) == objectType;
        }
    }
}


#endif