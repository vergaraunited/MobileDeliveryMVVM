using MobileDeliveryMVVM.MobileDeliveryServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using MobileDeliveryGeneral.Interfaces;                
using MobileDeliveryGeneral.Interfaces.DataInterfaces;
using MobileDeliveryGeneral.Settings;
using static MobileDeliveryGeneral.Definitions.MsgTypes;
using MobileDeliveryLogger;
using MobileDeliveryGeneral.Events;
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
        string umdurl;
        ushort umdport;
        string winurl;
        ushort winport;
        protected int count;

        public ViewModelBase(SocketSettings srvSet, string name)
        {
            if (srvSet != null)
            {
                this.name = name;
                InitConnections(srvSet.url, srvSet.port, srvSet.clienturl, srvSet.clientport);
                sm = new SendMsgDelegate(SendMessage);
            }           
        }

        public virtual void InitConnections(string umdurl = "localhost", ushort umdport = 81, string winurl="localhost", ushort winport=8181)
        {
            this.umdurl = umdurl;
            this.umdport = umdport;
            this.winurl = winurl;
            this.winport = winport;
            rm = new ReceiveMsgDelegate(ReceiveMessageCB);
            //Connect to WinSys Server
            winSys = new ClientSocketConnection(winurl, winport, name, ref smWinsys, rm);
            winSys.Connect();

            
            //Connect to UMD Server
            umdSrv = new ClientSocketConnection(umdurl, umdport, name, ref smUMDSrv, rm);
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
                case eCommand.LoadFiles:
                    smWinsys(cmd);
                    break;
                case eCommand.Drivers:
                case eCommand.Stops:
                case eCommand.Trucks:
                case eCommand.Orders:
                case eCommand.OrdersLoad:
                case eCommand.OrderDetails:
                case eCommand.UploadManifest:
                case eCommand.CompleteStop:
                    smUMDSrv(cmd);
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}
