using MobileDeliveryMVVM.BaseClasses;
using MobileDeliveryMVVM.Command;
using MobileDeliveryMVVM.Models;
using MobileDeliverySettings;
using System;
using MobileDeliveryGeneral.Settings;
using Windows.ApplicationModel;

namespace MobileDeliveryMVVM.ViewModel
{
    public class SettingsVM : BaseViewModel<SettingsModel>
    {
        public SettingsModel settings = new SettingsModel();

        string url;
        public string URL { get { return settings.Url; }
            set {
                SetProperty<string>(ref url, value);
                settings.Url = value;
            }
        }

        string port;
        public string PORT { get { return settings.Port.ToString(); } set {
                
                int pout;
                if (Int32.TryParse(value, out pout))
                {
                    SetProperty<string>(ref port, value);
                    settings.Port = pout;
                }
                else throw new Exception("Integer Values for Port Setting");
            } }

        string umdurl;
        public string UMDURL { get { return settings.UMDUrl; }
            set {
                SetProperty<string>(ref umdurl, value);
                settings.UMDUrl = value;
            }
        } 

        string umdport;
        public string UMDPORT { get { return settings.UMDPort.ToString(); } set
            {
                int res;
                if (Int32.TryParse(value, out res))
                {
                    SetProperty<string>(ref umdport, value);

                    settings.UMDPort = res;
                }
                else throw new Exception("Integer Values for UMD Port Setting");
            }
        }

        string winsysurl;
        public string WINSYSURL { get { return settings.WinsysUrl; } set
            {
                SetProperty<string>(ref winsysurl, value);
                settings.WinsysUrl = value;
            }
        }

        string winsysport;
        public string WINSYSPORT { get { return settings.WinsysPort.ToString(); }
            set
            {
                int res;
                if (Int32.TryParse(value, out res))
                {
                    SetProperty<string>(ref winsysport, value);
                    settings.WinsysPort = res;
                }
                else throw new Exception("Integer Values for Winsys Port Setting");
            }
        }

        string appname;
        public string APPNAME { get { return settings.AppName; } set {
                SetProperty<string>(ref appname, value);
                settings.AppName = value;
            } }

        string logpath;
        public string LOGPATH { get { return settings.LogPath; } set {
                SetProperty<string>(ref logpath, value);
                settings.LogPath = value;
            } }

        string loglevel;
        public string LOGLEVEL { get { return settings.LogLevel; } set {
                SetProperty<string>(ref loglevel, value);
                settings.LogLevel = value;
            } }

        string sqlconn;
        public string SQLConn { get { return settings.SQLConn; } set {
                SetProperty<string>(ref sqlconn, value);
                settings.SQLConn = value;
            } }

        private DelegateCommand _saveSettings;

        public DelegateCommand SaveSettings
        { get { return _saveSettings ?? (_saveSettings = new DelegateCommand(OnSaveSettings)); } }

        private void OnSaveSettings(object obj)
        {
            // Command = MobileDeliveryGeneral.Definitions.MsgTypes.eCommand.LoadSettings,
            //jsonify the settings into UMDConfig and persist the obj.
            var umdcfg = new UMDAppConfig()
            {
                LogLevel = (MobileDeliveryLogger.LogLevel)Enum.Parse(typeof(MobileDeliveryLogger.LogLevel), settings.LogLevel),
                AppName = settings.AppName,
                LogPath = settings.LogPath,
                SQLConn = settings.SQLConn,
                srvSet = new SocketSettings()
                {
                    url = settings.Url,
                    port = (ushort)settings.Port,
                    umdport = (ushort)settings.UMDPort,
                    umdurl = settings.UMDUrl,
                    WinSysUrl = settings.WinsysUrl,
                    WinSysPort = (ushort)settings.WinsysPort
                },
                // Version=
                // winsysFiles
            };
            settings.Url = URL;

            base.Refresh(settings);
        }

        public SettingsVM() : base(LoadConfig()) 
        {
        }
        public static UMDAppConfig LoadConfig()
        {
            string zt = Settings.Url;// set;

            var cf = new UMDAppConfig();
            cf.InitSrvSet();
            cf.AppName = Settings.AppName;
            cf.LogLevel = (MobileDeliveryLogger.LogLevel)Enum.Parse(typeof(MobileDeliveryLogger.LogLevel), Settings.LogLevel);
            cf.LogPath = Settings.LogPath;
            cf.SQLConn = Settings.SQLConn;
            cf.srvSet.port = (ushort)Settings.Port;
            cf.srvSet.url = Settings.Url;
            cf.srvSet.WinSysPort = (ushort)Settings.WinsysPort;
            cf.srvSet.WinSysUrl = Settings.WinsysUrl;
            cf.srvSet.umdurl = Settings.UMDUrl;
            cf.srvSet.umdport = (ushort)Settings.UMDPort;

            cf.Version = string.Format("Version: {0}.{1}.{2}.{3}",
                    Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor,
                    Package.Current.Id.Version.Build,
                    Package.Current.Id.Version.Revision);

            return cf;
        }
        public SettingsVM( UMDAppConfig config) : base(new UMDAppConfig() { AppName = "SettingsVM" })
        {
            //settings = new Settings()
            //{
            //    Command = MobileDeliveryGeneral.Definitions.MsgTypes.eCommand.LoadSettings,
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
