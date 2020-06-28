using MobileDeliveryMVVM.MobileDeliveryServer;
using MobileDeliveryGeneral.Interfaces;                
using MobileDeliveryGeneral.Interfaces.DataInterfaces;
using MobileDeliveryGeneral.Settings;
using static MobileDeliveryGeneral.Definitions.MsgTypes;
using MobileDeliveryLogger;
using MobileDeliveryGeneral.Events;
using MobileDeliveryMVVM.Models;
using System;
using MobileDeliveryGeneral.Definitions;
using MobileDeliveryClient.API;

namespace MobileDeliveryMVVM.BaseClasses
{
    public abstract class ViewModelDataCommunication : Notification
    {
        //static ClientSocketConnection winSys;
        //static ClientSocketConnection umdSrv;

        static ClientToServerConnection winsys;
        static ClientToServerConnection umdsrv;

        protected SendMsgDelegate sm;
        protected ReceiveMsgDelegate rm;
        static SendMsgDelegate smWinsys;
        static SendMsgDelegate smUMDSrv;
        string name;
        string umdurl;
        ushort umdport;
        string winurl;
        ushort winport;
        protected int count;
        
        //~ViewModelBase() {
        //    Shutdown();

        //}
        protected virtual void Shutdown() {
            Disconnect();
            //Shutdown();
        }
        SocketSettings GetSettings()
        {
            return new SocketSettings()
            {
                url = "localhost",
                port = 81,
                srvurl = "localhost",
                srvport = 81,
                clienturl = "localhost",
                clientport = 8181,
                name = "TruckVM",
                errrecontimeout = 60000,
                keepalive = 60000,
                recontimeout = 30000,
                retry = 60000
            };
        }
        public ViewModelDataCommunication()
        {
           //this.name = name;
            InitConnections(GetSettings());
            sm = new SendMsgDelegate(SendMessage);
        }

        virtual public void InitVM() { }
        virtual public void SendVMMesssage(manifestRequest mreq, Request req) { }

        //public virtual void InitConnections(string umdurl = "localhost", ushort umdport = 81, string winurl="localhost", ushort winport=8181)
        public virtual void InitConnections(SocketSettings srvSet, Boolean bForceWin = false, Boolean bForceUmd = false)
        {
            name = srvSet.name;

            if (winsys == null || bForceWin || rm == null)
            {
                this.winurl = srvSet.clienturl;
                this.winport = srvSet.clientport;
                //Set the Server URL and Port to connect to Winsys Server
                srvSet.url = this.winurl;
                srvSet.port = this.winport;
                rm = new ReceiveMsgDelegate(ReceiveMessageCB);
                //Connect to WinSys Server
                winsys = new ClientToServerConnection(srvSet, ref sm, rm);
            }

            if (umdsrv == null || bForceUmd || rm == null)
            {
                this.umdurl = srvSet.srvurl;
                this.umdport = srvSet.srvport;
                srvSet.url = this.umdurl;
                srvSet.port = this.umdport;
                //Connect to UMD Server
                umdsrv = new ClientToServerConnection(srvSet, ref smUMDSrv, rm);
            }

            Connect();
        }

        public void Connect()
        {
            Disconnect();
            if (winsys !=null)
                if (!winsys.IsConnected)
                    winsys.Connect();
            if (umdsrv!=null)
                if (!umdsrv.IsConnected)
                    umdsrv.Connect();
        }
        public void Disconnect()
        {
            if (winsys != null)
                if (winsys.IsConnected)
                    winsys.Disconnect();
            if (umdsrv!=null)
                if (umdsrv.IsConnected)
                    umdsrv.Disconnect();
        }
        public virtual void Clear(object obj) { }
        public virtual void Clear(bool bForce = false) { }
        public virtual void Refresh(object obj) {
            SettingsModel set = (SettingsModel)obj;
            
            //InitConnections(set.UMDUrl, (ushort)set.UMDPort, set.WinsysUrl, (ushort)set.WinsysPort );
            InitConnections(socSet, true, true);
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
                case eCommand.ScanFile:
                case eCommand.Drivers:
                case eCommand.Stops:
                case eCommand.Trucks:
                case eCommand.OrdersUpload:
                case eCommand.OrdersLoad:
                case eCommand.OrderDetails:
                case eCommand.OrderDetailsComplete:
                case eCommand.OrderOptions:
                case eCommand.OrderOptionsComplete:
                case eCommand.UploadManifest:
                case eCommand.CompleteOrder:
                case eCommand.CompleteStop:
                case eCommand.AccountReceivable:
                case eCommand.ScanFileComplete:
                    if (smUMDSrv == null)
                        InitConnections(socSet, false, true);
                    smUMDSrv(cmd);
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}
