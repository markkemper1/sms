namespace Sms.Services
{
    public interface ISerializerFactory
    {
        ISerializer Get(string provider);
    }
}
