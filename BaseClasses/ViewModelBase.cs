using MobileDeliveryMVVM.MobileDeliveryServer;
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
        protected static ClientSocketConnection winSys;
        protected static ClientSocketConnection umdSrv;

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
        SocketSettings socSet;

        ~ViewModelBase() {
        }
        public ViewModelBase(SocketSettings srvSet, string name)
        {
            if (srvSet != null)
            {
                this.name = name;
                InitConnections(srvSet);
                sm = new SendMsgDelegate(SendMessage);
                socSet = srvSet;
            }           
        }

        //public virtual void InitConnections(string umdurl = "localhost", ushort umdport = 81, string winurl="localhost", ushort winport=8181)
        public virtual void InitConnections(SocketSettings srvSet)
        {
            if (winSys == null)
            {
                this.winurl = srvSet.clienturl; //winurl;
                this.winport = srvSet.clientport; // winport;
                rm = new ReceiveMsgDelegate(ReceiveMessageCB);
                //Connect to WinSys Server
                winSys = new ClientSocketConnection(srvSet, ref smWinsys, rm);
                winSys.Connect();
            }
            if (umdSrv == null)
            {
                this.umdurl = srvSet.url;
                this.umdport = srvSet.port;
                //Connect to UMD Server
                umdSrv = new ClientSocketConnection(srvSet, ref smUMDSrv, rm);
                umdSrv.Connect();
            }
            else
            {
                umdSrv.ReInit(ref smUMDSrv);
            }
        }

        public virtual void Clear(object obj) { }
        public virtual void Refresh(object obj) {
            SettingsModel set = (SettingsModel)obj;
            winSys.Disconnect();
            umdSrv.Disconnect();
            //InitConnections(set.UMDUrl, (ushort)set.UMDPort, set.WinsysUrl, (ushort)set.WinsysPort );
            InitConnections(socSet);
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
                    if (smUMDSrv == null)
                        InitConnections(socSet);
                    smUMDSrv(cmd);
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}
