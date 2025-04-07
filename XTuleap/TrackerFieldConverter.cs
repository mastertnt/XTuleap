using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Text;

namespace XTuleap
{
    class TrackerFieldConverter : Newtonsoft.Json.Converters.CustomCreationConverter<ITrackerField>
    {
        public override ITrackerField Create(Type objectType)
        {
            throw new NotImplementedException();
        }

        public ITrackerField Create(Type objectType, JObject jObject)
        {
            var type = (string)jObject.Property("type");

            switch (type)
            {
                case "rb":
                case "sb":
                case "msb":
                case "cb":
                    return new TrackerFieldEnum();
                default:
                    return new TrackerField();
            }

            throw new ApplicationException(String.Format("The animal type {0} is not supported!", type));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load JObject from stream 
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject 
            var target = Create(objectType, jObject);

            // Populate the object properties 
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }
    }
}
