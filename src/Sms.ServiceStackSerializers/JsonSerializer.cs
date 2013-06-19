using System;

namespace Sms.Services
{
    public class JsonSerializer : ISerializer
    {
        public string Name { get { return "json"; } }

        public string Serialize(object item)
        {
            return ServiceStack.Text.JsonSerializer.SerializeToString(item);
        }

        public object Deserialize(Type type, string text)
        {
            return ServiceStack.Text.JsonSerializer.DeserializeFromString(text, type);
        }
    }
}
