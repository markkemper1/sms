using System;
using DotNetConfigHelper;

[assembly: WebActivatorEx.PreApplicationStartMethod(
    typeof(Sms.Test.App_Start.DotNetConfigHelperStartup), "PreStart")]

namespace Sms.Test.App_Start {
    public static class DotNetConfigHelperStartup {
        public static void PreStart()
        {
            AppSettingsReplacer.Install(DotNetConfigHelper.ConfigProvider.CreateAndSetDefault());
        }
    }
}