using MobileDeliveryClient.Logging;
using MobileDeliveryMVVM.BaseClasses;
using MobileDeliverySettings;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using UMDGeneral.Settings;

namespace MobileDeliveryMVVM.ViewModel
{
    public class SettingsVM : BaseViewModel<Settings>
    {
        //static ISettings AppSettings
        //{
        //    get
        //    {
        //        return CrossSettings.Current;
        //    }
        //}

        //static Settings settings;
        //public static Settings Current
        //{
        //    get { return settings ?? (settings = new Settings()); }
        //}
        public SettingsVM() : base(new UMDAppConfig() { AppName = "SettingsVM" })
        { }

        public SettingsVM( UMDAppConfig config) : base(new UMDAppConfig() { AppName = "SettingsVM" })
        {
            //settings = new Settings()
            //{
            //    Command = UMDGeneral.Definitions.MsgTypes.eCommand.LoadSettings,
            //    LogLevel = config.LogLevel.ToString(),
            //    Url = config.srvSet.url,
            //    Port  = config.srvSet.port,
            //    UMDUrl = config.srvSet.umdurl,
            //    UMDPort = config.srvSet.umdport,
            //    WinsysUrl = config.srvSet.WinSysUrl,
            //    WinsysPort = config.srvSet.WinSysPort                
            //};
        }
    }

}
