using System;
using Sms.Services;

namespace Sms.ServiceStackSerializers
{
    public class XmlSerializer : ISerializer
    {
        public string Name { get { return "xml"; } }

        public string Serialize(object item)
        {
            return ServiceStack.Text.XmlSerializer.SerializeToString(item);
        }

        public object Deserialize(Type type, string text)
        {
            return ServiceStack.Text.XmlSerializer.DeserializeFromString(text, type);
        }
    }
}