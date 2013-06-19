using System;

namespace Sms.Services
{
    public interface ISerializer
    {
        string Name { get; }

        string Serialize(object item);

        object Deserialize(Type type, string text);
    }
}