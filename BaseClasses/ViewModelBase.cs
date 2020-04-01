using MobileDeliveryMVVM.MobileDeliveryServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UMDGeneral.Interfaces;                
using UMDGeneral.Interfaces.DataInterfaces;
using UMDGeneral.Settings;
using static UMDGeneral.Definitions.MsgTypes;
using MobileDeliveryLogger;
using UMDGeneral.Events;
using MobileDeliveryMVVM.Models;

namespace MobileDeliveryMVVM.BaseClasses
{
    public abstract class ViewModelBase<T> : Notification where T : IMDMMessage
    {
        protected ClientSocketConnection winSys;
        protected ClientSocketConnection umdSrv;

        protected SendMsgDelegate sm;
        protected ReceiveMsgDelegate rm;
        SendMsgDelegate smWinsys;
        SendMsgDelegate smUMDSrv;
        string name;
        string url;
        ushort port;
        string wurl;
        ushort wport;
        protected int count;

        public ViewModelBase(UMDAppConfig config)
        {
            if (config != null)
            {
                if (config.AppName != null)
                    this.name = config.AppName;
                if (config.srvSet == null)
                {
                    config.InitSrvSet();
                }

                InitConnections(config.srvSet.url, config.srvSet.port, config.srvSet.WinSysUrl, config.srvSet.WinSysPort);

                sm = new SendMsgDelegate(SendMessage);
            }           
        }

        public virtual void InitConnections(string url = "localhost", ushort port = 81, string wurl="localhost", ushort wport=8181)
        {
            this.url = url;
            this.port = port;
            this.wurl = wurl;
            this.wport = wport;
            rm = new ReceiveMsgDelegate(ReceiveMessageCB);
            //Connect to WinSys Server
            winSys = new ClientSocketConnection(new SocketSettings() { port = this.wport, url = this.wurl, WinSysUrl=wurl, WinSysPort=wport, name = name }, ref smWinsys, rm);
            winSys.Connect();

            
            //Connect to UMD Server
            umdSrv = new ClientSocketConnection(new SocketSettings() { port = this.port, url = this.url, WinSysUrl = wurl, WinSysPort = wport, name = name }, ref smUMDSrv, rm);
            umdSrv.Connect();
        }

        public virtual void Clear(object obj) { }
        public virtual void Refresh(object obj) {
            SettingsModel set = (SettingsModel)obj;
            winSys.Disconnect();
            umdSrv.Disconnect();
            InitConnections(set.UMDUrl, (ushort)set.UMDPort, set.WinsysUrl, (ushort)set.WinsysPort );
        }

        public virtual isaCommand ReceiveMessageCB(isaCommand cmd)
        {
            // Default behavior for load files 
            // Winsys Driver is telling us the TPS Clarion Files for this date are missing 
            // and need to be copied over.
            switch (cmd.command)
            {
                case eCommand.LoadFiles:
                    Logger.Error("Winsys TPS Clarion file(s) missing.  Reload files, then try again.");
                    break;
                default:
                    Logger.Error($"ViewModelBase::RecieveMessageCB Command not handled. {cmd.command.ToString()}");
                    break;
            }
            return cmd;
        }

        //public virtual isaCommand ProcessMessage(isaCommand cmd)
        //{
        //    return cmd;
        //}

        protected virtual bool SendMessage(isaCommand cmd)
        {
            switch (cmd.command)
            {
                case eCommand.GenerateManifest:
                    smWinsys(cmd);
                    break;
                case eCommand.Drivers:
                case eCommand.Stops:
                case eCommand.Trucks:
                case eCommand.OrdersLoad:
                case eCommand.OrderDetails:
                    smUMDSrv(cmd);
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}
