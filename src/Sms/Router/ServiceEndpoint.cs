namespace Sms.Router
{
    public class ServiceEndpoint
    {
        public string MessageType { get; set; }

        public string ProviderName { get; set; }

        public string QueueIdentifier { get; set; }

        public string Version { get; set; }
    }
}