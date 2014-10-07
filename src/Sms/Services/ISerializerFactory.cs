namespace Sms.Services
{
    public interface ISerializerFactory
    {
		void Register(ISerializer serializer);
        ISerializer Get(string provider);
    }
}
