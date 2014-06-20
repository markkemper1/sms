using Amazon.Runtime;
using Amazon.SQS;

namespace Sms.Aws
{
    public static class ClientFactory
    {
        public static AmazonSQSClient Create()
        {
            var config = new AmazonSQSConfig();
            config.ServiceURL = Config.Require("Aws.Sqs.ServiceURL").Value;
            var credentials = new BasicAWSCredentials(Config.Require("Aws.Sqs.AccessKey").Value, Config.Require("Aws.Sqs.SecrentKey").Value);
            var client = new AmazonSQSClient(credentials, config);
            return client;
        }

    }
}
