using MobileDeliveryMVVM.BaseClasses;
using MobileDeliveryMVVM.Command;
using MobileDeliveryMVVM.Models;
using MobileDeliverySettings;
using System;
using Windows.ApplicationModel;
using DataCaching.Caching;
using System.IO;
using MobileDeliveryGeneral.Settings;
using MobileDeliverySettings.Settings;
using MobileDeliveryGeneral.Data;

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

        string srvurl;
        public string UMDURL { get { return settings.UMDUrl; }
            set {
                SetProperty<string>(ref srvurl, value);
                settings.UMDUrl = value;
            }
        } 

        string srvport;
        public string UMDPORT { get { return settings.UMDPort.ToString(); } set
            {
                int res;
                if (Int32.TryParse(value, out res))
                {
                    SetProperty<string>(ref srvport, value);

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

        /// timeouts
        /// ushort ukeepalive;
        //ushort.TryParse(keepAlive, out ukeepalive);
        //ushort uretry;
        //ushort.TryParse(retry, out uretry);
        //ushort urecontimeout;
        //ushort.TryParse(recontimeout, out urecontimeout);
        //ushort uerrrecontimeout;
        //ushort.TryParse(errrecontimeout, out uerrrecontimeout);

        int keepAlive;
        public int KeepAlive
        {
            get { return settings.KeepAlive; }
            set
            {
                SetProperty<int>(ref keepAlive, value);
                settings.KeepAlive = value;
            }
        }

        int retry;
        public int Retry
        {
            get { return settings.Retry; }
            set
            {
                SetProperty<int>(ref retry, value);
                settings.Retry = value;
            }
        }

        int recontimeout;
        public int ReconTimeout
        {
            get { return settings.ReconTimeout; }
            set
            {
                SetProperty<int>(ref recontimeout, value);
                settings.ReconTimeout = value;
            }
        }

        int errrecontimeout;
        public int ErrReconTimeout
        {
            get { return settings.ErrReconTimeout; }
            set
            {
                SetProperty<int>(ref errrecontimeout, value);
                settings.ErrReconTimeout = value;
            }
        }


        /// <SQLite>
        /// SQLite cache paths
        /// </SQLite>

        string trucks;
        public string TruckCachePath
        {
            get { return settings.TruckCachePath.ToString(); }
            set
            {
                SetProperty<string>(ref port, value);
                settings.TruckCachePath = value;
            }
        }

        string orders;
        public string OrderCachePath
        {
            get { return settings.OrderCachePath; }
            set
            {
                SetProperty<string>(ref orders, value);
                settings.OrderCachePath = value;
            }
        }

        string stops;
        public string StopCachePath
        {
            get { return settings.StopCachePath.ToString(); }
            set
            {
                SetProperty<string>(ref stops, value);
                settings.StopCachePath = value;
            }
        }

        string orderdetail;
        public string OrderDetailCachePath
        {
            get { return settings.OrderDetailCachePath; }
            set
            {
                SetProperty<string>(ref orderdetail, value);
                settings.OrderDetailCachePath = value;
            }
        }


        //ReInitialize
        private DelegateCommand _reInitialize;
        public DelegateCommand ReInitializeCommand
        { get { return _reInitialize ?? (_reInitialize = new DelegateCommand(ReInitialize)); } }

        public void ReInitialize(object arg)
        {
            var truck = new CacheItem<TruckData>(SettingsAPI.TruckCachePath);
            truck.BackupAndClearAll();
        }


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
                    srvport = (ushort)settings.UMDPort,
                    srvurl = settings.UMDUrl,
                    clienturl = settings.WinsysUrl,
                    clientport = (ushort)settings.WinsysPort,
                    retry = 60000,
                    name = "SettingsVM",
                    keepalive = 60000,
                    errrecontimeout = 30000,
                    recontimeout = 60000
                },
                // Version=
                // winsysFiles
            };
            settings.Url = URL;

            base.Refresh(settings);
        }

        public SettingsVM() : base(new SocketSettings()
        {
            url = "localhost",
            port = 81,
            srvurl = "localhost",
            srvport = 81,
            clienturl = "localhost",
            clientport = 8181,
            name = "SettingsVM",
            errrecontimeout = 60000,
            keepalive = 60000,
            recontimeout = 30000,
            retry = 60000

        }, "SettingsVM")
        {
        }
        public static UMDAppConfig LoadConfig()
        {
            string zt = SettingsAPI.Url;// set;

            var cf = new UMDAppConfig();
            cf.InitSrvSet();
            cf.AppName = SettingsAPI.AppName;
            cf.LogLevel = (MobileDeliveryLogger.LogLevel)Enum.Parse(typeof(MobileDeliveryLogger.LogLevel), SettingsAPI.LogLevel);
            cf.LogPath = SettingsAPI.LogPath;
            cf.SQLConn = SettingsAPI.SQLConn;
            cf.srvSet.port = (ushort)SettingsAPI.Port;
            cf.srvSet.url = SettingsAPI.Url;
            cf.srvSet.clientport = (ushort)SettingsAPI.WinsysPort;
            cf.srvSet.clienturl = SettingsAPI.WinsysUrl;
            cf.srvSet.srvurl = SettingsAPI.UMDUrl;
            cf.srvSet.srvport = (ushort)SettingsAPI.UMDPort;

            cf.Version = string.Format("Version: {0}.{1}.{2}.{3}",
                    Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor,
                    Package.Current.Id.Version.Build,
                    Package.Current.Id.Version.Revision);

            return cf;
        }
        public SettingsVM( UMDAppConfig config) : base(config.srvSet, config.AppName)
        {
            //settings = new Settings()
            //{
            //    Command = MobileDeliveryGeneral.Definitions.MsgTypes.eCommand.LoadSettings,
            //    LogLevel = config.LogLevel.ToString(),
            //    Url = config.srvSet.url,
            //    Port  = config.srvSet.port,
            //    UMDUrl = config.srvSet.srvurl,
            //    UMDPort = config.srvSet.srvport,
            //    WinsysUrl = config.srvSet.clienturl,
            //    WinsysPort = config.srvSet.clientport                
            //};
        }
    }

}
