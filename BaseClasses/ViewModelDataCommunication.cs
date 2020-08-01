using MobileDeliveryGeneral.Interfaces;
using MobileDeliveryGeneral.Interfaces.DataInterfaces;
using static MobileDeliveryGeneral.Definitions.MsgTypes;
using MobileDeliveryLogger;
using MobileDeliveryGeneral.Events;
using MobileDeliveryMVVM.Models;
using System;
using MobileDeliveryGeneral.Definitions;
using MobileDeliveryClient.API;
using MobileDeliveryGeneral.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MobileDeliverySettings.Settings;
using MobileDeliveryGeneral.Data;

namespace MobileDeliveryMVVM.BaseClasses
{
    public abstract class ViewModelDataCommunication : Notification
    {
        ClientToServerConnection srv;
        ReceiveMsgDelegate rm;
        //static SendMsgDelegate smWinsys;
        SendMsgDelegate sm;
        string name;
        string umdurl;
        ushort umdport;
        protected int count;
        protected Dictionary<Guid, Request> dRequests = new Dictionary<Guid, Request>();
        //protected Dictionary<Guid, Request> dWinsysRequests = new Dictionary<Guid, Request>();

        //~ViewModelBase() {
        //    Shutdown();

        //}
        //public ViewModelDataCommunication(ReceiveMsgDelegate rm) {
        //    rmUMD = rm; InitConnections();
        //}
        protected virtual void Shutdown() {
            Disconnect();
            //Shutdown();
        }
        SocketSettings GetSettings()
        {
        //    MobileDeliverySettings.Settings.UMDAppConfig.dSettings["AppName"] = name;
            return new SocketSettings();
        }
        public ViewModelDataCommunication()
        {
            InitConnections();
        }

        virtual public void InitVM() { }
        virtual public void SendVMMesssage(manifestRequest mreq, Request req) { }

        //Progress Changed - Generic and overridable
        protected UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage> pcProcessMessage;
        protected UMBackgroundWorker<IMDMMessage> msgThread;

        public void StartProcess(manifestRequest mreq, ProcessMsgDelegateRXRaw cbsend = null)
        {
            var req = new Request()
            {
                reqGuid = NewGuid(mreq.requestId),
                LIds = new Dictionary<long, status>(),
                LinkMid = new Dictionary<long, List<long>>(),
                ChkIds = new Dictionary<long, status>()
            };
            dRequests.Add(req.reqGuid, req);
            msgThread.OnStartProcess(mreq, req, cbsend);
        }
        //public virtual void InitConnections(string umdurl = "localhost", ushort umdport = 81, string winurl="localhost", ushort winport=8181)
        public virtual void InitConnections(SocketSettings settings = null, Boolean bForceWin = false, Boolean bForceUmd = false)
        {
            if (settings==null)
                settings = GetSettings();

            
            name = settings.name;

            if (srv == null || bForceUmd || rm == null)
            {
                this.umdurl = settings.srvurl;
                this.umdport = settings.srvport;
                settings.url = this.umdurl;
                settings.port = this.umdport;
                rm = new ReceiveMsgDelegate(ReceiveMessage);
                pcProcessMessage = new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(ProcessMessage);
                srv = new ClientToServerConnection(settings, ref sm, rm);
                msgThread = new UMBackgroundWorker<IMDMMessage>(new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(pcProcessMessage), rm, sm);
                //Connect to UMD Server
             
            }

            Connect();
        }

        public void Connect()
        {
            if (srv.IsConnected)
            {
                Logger.Debug($"Already Connected {srv.Name}");
                return;
            }
              
            //Disconnect();
            //if (winsys !=null)
            //    if (!winsys.IsConnected)
            //        winsys.Connect();
            if (srv!=null)
                if (!srv.IsConnected)
                    srv.Connect();
        }
        public void Disconnect()
        {
            //if (winsys != null)
            //    if (winsys.IsConnected)
            //        winsys.Disconnect();
            if (srv!=null)
                if (srv.IsConnected)
                    srv.Disconnect();
        }
        protected virtual void Clear(object obj) { }
        protected virtual void Clear(bool bForce = false) {
            if (msgThread != null)
                dRequests.Keys.ToList().ForEach(a => msgThread.Reset(a));
        }
        public virtual void Refresh(object obj) {
            SettingsModel set = (SettingsModel)obj;
            InitConnections(set.SocketSettings(), true, true);
        }

        protected virtual isaCommand ReceiveMessage(isaCommand cmd) {
            switch (cmd.command)
            {
                case eCommand.Trucks:
                    //msgThread.bgWorker_DoWork()
                    trucks trk = (trucks)cmd;
                    var td = new TruckData(trk);
                    //AddTruck(new TruckData(trk));
                    var ocmd = new object[] { td };
                    msgThread.ReportProgress(50, ocmd);
                    //pcProcessMessage.Ta
                    // msgThread.ReportProgress(50, new object[] { cmd });

                    Logger.Info("eCommand.Trucks.");
                    break;
                case eCommand.TrucksLoadComplete:
                    Logger.Error("UMD SQL Server TrucksLoadComplete.");
                    msgThread.ReportProgress(100, new object[] { cmd });
                    break;
                default:
                    Logger.Error($"ViewModelBase::ReceiveMessageUMD Command not handled. {cmd.command.ToString()}");
                    break;
            }
            return cmd;
        }


        //  protected virtual isaCommand ReceiveMessageWinSys(isaCommand msg)
        //  {
        //      if (msg.command == eCommand.Ping) {
        //          Logger.Debug($"ReceiveMessage - Received Ping / Replying Pong..");
        //         // smWinsys(new MsgTypes.Command { command = eCommand.Pong });
        //      }
        //      else if (msg.command == eCommand.Pong)
        //          Logger.Debug($"ReceiveMessage - Received Pong");
        //      /*
        //      else if (msg.command == eCommand.TrucksLoadComplete || msg.command == eCommand.CheckManifestComplete ||
        //          msg.command == eCommand.DeliveryComplete || msg.command == eCommand.DriversLoadComplete || msg.command == eCommand.LoadFilesComplete
        //          || msg.command == eCommand.ManifestDetailsComplete || msg.command == eCommand.ManifestLoadComplete || msg.command == eCommand.OrderDetailsComplete ||
        //          msg.command == eCommand.OrderModelLoadComplete || msg.command == eCommand.OrderOptionsComplete || msg.command == eCommand.OrdersLoadComplete ||
        //          msg.command == eCommand.OrderUpdatesComplete || msg.command == eCommand.ScanFileComplete || msg.command == eCommand.StopsLoadComplete)
        //          umdMessageThread.CompleteBackgroundWorker(NewGuid(msg.requestId));
        //      else if (msg.command == eCommand.UploadManifestComplete)
        //          winsysMessageThread.CompleteBackgroundWorker(NewGuid(msg.requestId));
        //          */
        //      // Default behavior for load files 
        //      // Winsys Driver is telling us the TPS Clarion Files for this date are missing 
        //      // and need to be copied over.
        //      //switch (cmd.command)
        //      //{
        //      //    case eCommand.LoadFiles:
        //      //        Logger.Error("Winsys TPS Clarion file(s) missing.  Reload files, then try again.");
        //      //        break;
        //      //    default:
        //      //        Logger.Error($"ViewModelBase::RecieveMessageCB Command not handled. {cmd.command.ToString()}");
        //      //        break;
        //      //}
        //      return msg;
        //  }
        /*
            protected virtual isaCommand ReceiveMessage(isaCommand cmd)
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
        */
        protected void ProcessMessage(IMDMMessage msg, Func<byte[], Task> cbsend = null)
        {
            if (msg.Command == eCommand.TrucksLoadComplete || msg.Command == eCommand.CheckManifestComplete ||
                msg.Command == eCommand.DeliveryComplete || msg.Command == eCommand.DriversLoadComplete || msg.Command == eCommand.LoadFilesComplete
                || msg.Command == eCommand.ManifestDetailsComplete || msg.Command == eCommand.ManifestLoadComplete || msg.Command == eCommand.OrderDetailsComplete ||
                msg.Command == eCommand.OrderModelLoadComplete || msg.Command == eCommand.OrderOptionsComplete || msg.Command == eCommand.OrdersLoadComplete ||
                msg.Command == eCommand.OrderUpdatesComplete || msg.Command == eCommand.ScanFileComplete || msg.Command == eCommand.StopsLoadComplete)
                msgThread.CompleteBackgroundWorker(msg.RequestId);
            //else if (msg.Command == eCommand.UploadManifestComplete)
            //    winsysMessageThread.CompleteBackgroundWorker(msg.RequestId);

            //else if (msg.Command == eCommand.Trucks || msg.Command == eCommand.AccountReceivable || msg.Command == eCommand.CheckManifest ||
            //    msg.Command == eCommand.CompleteOrder || msg.Command == eCommand.CompleteStop || msg.Command == eCommand.ConfirmDelivery || 
            //    msg.Command == eCommand.CreateCustomerAccount || msg.Command == eCommand.CreateOrder || msg.Command == eCommand.DeliveryComplete ||
            //    msg.Command == eCommand.Drivers || msg.Command == eCommand.GetCustomerBalance || msg.Command == eCommand.ManifestDetails || 
            //    msg.Command == eCommand.OrderDetails || msg.Command == eCommand.OrderModel || msg.Command == eCommand.OrderOptions || 
            //    msg.Command == eCommand.OrdersLoad || msg.Command == eCommand.OrdersUpload || msg.Command == eCommand.ScanFile || 
            //    msg.Command == eCommand.Stops || msg.Command == eCommand.Trucks || msg.Command == eCommand.UploadManifest || 
            //    msg.Command == eCommand.UploadManifestDetails || msg.Command == eCommand.Withdraw)
            //    umdMessageThread.bgWorker_DoWork()
        }

        protected virtual bool SendMessage(isaCommand cmd)
        {
            switch (cmd.command)
            {
                case eCommand.GenerateManifest:
                case eCommand.LoadFiles:
                    //dWinsysRequests.Add(NewGuid(cmd.requestId), 
                    //    new Request() { reqGuid = NewGuid(cmd.requestId), ChkIds = new Dictionary<long, status>(),
                    //        LIds = new Dictionary<long, status>(), LinkMid = new Dictionary<long, List<long>>() }
                    //    );
                    //smWinsys(cmd);
                    dRequests.Add(NewGuid(cmd.requestId), new Request()
                    {
                        reqGuid = NewGuid(cmd.requestId),
                        ChkIds = new Dictionary<long, status>(),
                        LIds = new Dictionary<long, status>(),
                        LinkMid = new Dictionary<long, List<long>>()
                    });
                    sm(cmd);
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
                    //if (smUMDSrv == null)
                    //    InitConnections(socSet, false, true);
                    dRequests.Add(NewGuid(cmd.requestId), new Request() {
                        reqGuid = NewGuid(cmd.requestId), ChkIds = new Dictionary<long, status>(),
                            LIds = new Dictionary<long, status>(), LinkMid = new Dictionary<long, List<long>>() });
                    sm(cmd);
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}
