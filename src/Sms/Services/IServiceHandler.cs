namespace Sms.Services
{
    public interface IServiceHandler<T>
    {
        void Process(T request);
    }
}