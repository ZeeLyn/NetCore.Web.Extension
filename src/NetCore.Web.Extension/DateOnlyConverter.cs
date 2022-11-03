
#if NET6_0

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonConverter = Newtonsoft.Json.JsonConverter;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace NetCore.Web.Extension
{
    public class DateOnlyConverter : JsonConverter
    {
        private readonly string _format;

        public DateOnlyConverter(string format = "yyyy/MM/dd")
        {
            _format = format;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ((DateOnly)value).ToString(_format));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jt = JToken.ReadFrom(reader);
            return DateOnly.Parse(jt.ToString());
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(DateOnly) == objectType;
        }
    }
}


#endif