using MobileDeliverySettings;
using System;
using MobileDeliveryGeneral.Definitions;
using MobileDeliveryGeneral.Interfaces.DataInterfaces;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using MobileDeliveryGeneral.Data;

namespace MobileDeliveryMVVM.Models
{
    public class SettingsModel : BaseModel, isaSettings
    {
       // private Settings settings =  Settings.;

        public string AppName { get { return Settings.AppName; } set { Settings.AppName = value; OnPropertyChanged("AppName"); } }
        public string LogLevel { get { return Settings.LogLevel; } set { Settings.LogLevel = value; OnPropertyChanged("LogLevel"); } }
        public string LogPath { get { return Settings.LogPath; } set { Settings.LogPath = value; OnPropertyChanged("LogPath"); } }
        public string Url { get { return Settings.Url; } set {Settings.Url=value; OnPropertyChanged("Url"); } }
        public int Port { get { return Settings.Port; } set { Settings.Port = value; OnPropertyChanged("Port"); } }
        public string WinsysUrl { get {return Settings.WinsysUrl; } set {Settings.WinsysUrl = value; OnPropertyChanged("WinsysUrl"); } }
        public int WinsysPort { get { return Settings.WinsysPort; } set {Settings.WinsysPort = value; OnPropertyChanged("WinsysPort"); } }
        public string UMDUrl { get{ return Settings.UMDUrl; } set {Settings.UMDUrl = value; OnPropertyChanged("UMDUrl"); } }
        public int UMDPort { get { return Settings.UMDPort; } set { Settings.UMDPort = value; OnPropertyChanged("UMDPort"); } }

        //SQLite Cache file Paths

        //trucks
        public string TruckCachePath { get { return Settings.TruckCachePath; } set { Settings.TruckCachePath = value; OnPropertyChanged("TruckCachePath"); } }
        public string OrderCachePath { get { return Settings.OrderCachePath; } set { Settings.OrderCachePath = value; OnPropertyChanged("OrderCachePath"); } }
        public string StopCachePath { get { return Settings.StopCachePath; } set { Settings.StopCachePath = value; OnPropertyChanged("StopCachePath"); } }
        public string OrderDetailCachePath { get { return Settings.OrderDetailCachePath; } set { Settings.OrderDetailCachePath = value; OnPropertyChanged("OrderDetailCachePath"); } }


        public string SQLConn { get { return Settings.SQLConn; } set { Settings.SQLConn = value; OnPropertyChanged("SQLConn"); } }
        public MsgTypes.eCommand Command { get { return Settings.Command; } set { Settings.Command = value; OnPropertyChanged("Command"); } }
        public Guid RequestId { get { return Settings.RequestId; } set { Settings.RequestId = value; OnPropertyChanged("RequestId"); } }
    }
}
