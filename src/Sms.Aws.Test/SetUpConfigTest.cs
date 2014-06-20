using DotNetConfigHelper;
using NUnit.Framework;

namespace Sms.Aws.Test
{
    [SetUpFixture]
    public class SetUpConfigTest
    {
        [SetUp]
        public void Setup()
        {
            AppSettingsReplacer.Install(DotNetConfigHelper.ConfigProvider.CreateAndSetDefault());
        }
    }
}
