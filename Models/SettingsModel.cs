using MobileDeliverySettings;
using System;
using MobileDeliveryGeneral.Definitions;
using MobileDeliveryGeneral.Interfaces.DataInterfaces;
using MobileDeliverySettings.Settings;

namespace MobileDeliveryMVVM.Models
{
    public class SettingsModel : BaseModel, isaSettings
    {
       // private Settings settings =  Settings.;

        public string AppName { get { return SettingsAPI.AppName; } set { SettingsAPI.AppName = value; OnPropertyChanged("AppName"); } }
        public string LogLevel { get { return SettingsAPI.LogLevel; } set { SettingsAPI.LogLevel = value; OnPropertyChanged("LogLevel"); } }
        public string LogPath { get { return SettingsAPI.LogPath; } set { SettingsAPI.LogPath = value; OnPropertyChanged("LogPath"); } }
        public string Url { get { return SettingsAPI.Url; } set { SettingsAPI.Url=value; OnPropertyChanged("Url"); } }
        public int Port { get { return SettingsAPI.Port; } set { SettingsAPI.Port = value; OnPropertyChanged("Port"); } }
        public string WinsysUrl { get {return SettingsAPI.WinsysUrl; } set { SettingsAPI.WinsysUrl = value; OnPropertyChanged("WinsysUrl"); } }
        public int WinsysPort { get { return SettingsAPI.WinsysPort; } set { SettingsAPI.WinsysPort = value; OnPropertyChanged("WinsysPort"); } }
        public string UMDUrl { get{ return SettingsAPI.UMDUrl; } set { SettingsAPI.UMDUrl = value; OnPropertyChanged("UMDUrl"); } }
        public int UMDPort { get { return SettingsAPI.UMDPort; } set { SettingsAPI.UMDPort = value; OnPropertyChanged("UMDPort"); } }

        //SQLite Cache file Paths

        //trucks
        public string TruckCachePath { get { return SettingsAPI.TruckCachePath; } set { SettingsAPI.TruckCachePath = value; OnPropertyChanged("TruckCachePath"); } }
        public string OrderCachePath { get { return SettingsAPI.OrderCachePath; } set { SettingsAPI.OrderCachePath = value; OnPropertyChanged("OrderCachePath"); } }
        public string StopCachePath { get { return SettingsAPI.StopCachePath; } set { SettingsAPI.StopCachePath = value; OnPropertyChanged("StopCachePath"); } }
        public string OrderDetailCachePath { get { return SettingsAPI.OrderDetailCachePath; } set { SettingsAPI.OrderDetailCachePath = value; OnPropertyChanged("OrderDetailCachePath"); } }

        //   
        public int KeepAlive { get { return SettingsAPI.KeepAlive; } set { SettingsAPI.KeepAlive = value; OnPropertyChanged("KeepAlive"); } }
        public int Retry { get { return SettingsAPI.Retry; } set { SettingsAPI.Retry = value; OnPropertyChanged("Retry"); } }
        public int ReconTimeout { get { return SettingsAPI.ReconTimeout; } set { SettingsAPI.ReconTimeout = value; OnPropertyChanged("ReconTimeout"); } }
        public int ErrReconTimeout { get { return SettingsAPI.ErrReconTimeout; } set { SettingsAPI.ErrReconTimeout = value; OnPropertyChanged("ErrReconTimeout"); } }
        public int SocketBuffSize { get { return SettingsAPI.SockBuffSize; } set { SettingsAPI.SockBuffSize = value; OnPropertyChanged("SocketBufferSize"); } }


        public string SQLConn { get { return SettingsAPI.SQLConn; } set { SettingsAPI.SQLConn = value; OnPropertyChanged("SQLConn"); } }
        public MsgTypes.eCommand Command { get { return SettingsAPI.Command; } set { SettingsAPI.Command = value; OnPropertyChanged("Command"); } }
        public override Guid RequestId { get { return SettingsAPI.RequestId; } set { SettingsAPI.RequestId = value; OnPropertyChanged("RequestId"); } }

        public SocketSettings SocketSettings()
        {
            return new SocketSettings()
            {
                clientport = (ushort)Port,
                clienturl = Url,
                errrecontimeout = (ushort)ErrReconTimeout,
                keepalive = (ushort)KeepAlive,
                name = AppName,
                port = (ushort)Port,
                recontimeout = (ushort)ReconTimeout,
                socketbuffsize = (short)SocketBuffSize,
                retry = (ushort)Retry,
                srvport = (ushort)UMDPort,
                srvurl = UMDUrl,
                url = Url
            };
        }
    }
}
