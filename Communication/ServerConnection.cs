namespace MobileDeliveryMVVM.Communication
{
    /* public class ServerConnection
     {
         protected static ClientSocketConnection winSys;
         protected static ClientSocketConnection umdSrv;

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
         SocketSettings socSet;

         //~ViewModelBase() {
         //    Shutdown();

         //}
         protected virtual void Shutdown()
         {
             Disconnect();
             //Shutdown();
         }
         public ServerConnection(SocketSettings srvSet, string name)
         {
             if (srvSet != null)
             {
                 this.name = name;
                 InitConnections(srvSet);
                 sm = new SendMsgDelegate(SendMessage);
                 socSet = srvSet;
             }
         }

         virtual public void InitVM() { }
         virtual public void SendVMMesssage(manifestRequest mreq, Request req) { }

         //public virtual void InitConnections(string umdurl = "localhost", ushort umdport = 81, string winurl="localhost", ushort winport=8181)
         public virtual void InitConnections(SocketSettings srvSet, Boolean bForceWin = false, Boolean bForceUmd = false)
         {
             Disconnect();

             if (winSys == null || bForceWin || rm == null)
             {
                 this.winurl = srvSet.clienturl;
                 this.winport = srvSet.clientport;
                 //Set the Server URL and Port to connect to Winsys Server
                 srvSet.url = this.winurl;
                 srvSet.port = this.winport;
                 rm = new ReceiveMsgDelegate(ReceiveMessageCB);
                 //Connect to WinSys Server
                 winSys = new ClientSocketConnection(srvSet, ref smWinsys, rm);
                 // winSys.Connect();
             }



             if (umdSrv == null || bForceUmd || rm == null)
             {
                 this.umdurl = srvSet.srvurl;
                 this.umdport = srvSet.srvport;
                 srvSet.url = this.umdurl;
                 srvSet.port = this.umdport;
                 //Connect to UMD Server
                 umdSrv = new ClientSocketConnection(srvSet, ref smUMDSrv, rm);
                 //umdSrv.Connect();
             }

             Connect();
         }

         public void Connect()
         {
             Disconnect();
             if (winSys != null)
                 if (!winSys.IsConnected)
                     winSys.Connect();
             if (umdSrv != null)
                 if (!umdSrv.IsConnected)
                     umdSrv.Connect();
         }
         public void Disconnect()
         {
             if (winSys != null)
                 if (winSys.IsConnected)
                     winSys.Disconnect();
             if (umdSrv != null)
                 if (umdSrv.IsConnected)
                     umdSrv.Disconnect();
         }
         public virtual void Clear(object obj) { }
         public virtual void Clear(bool bForce = false) { }
         public virtual void Refresh(object obj)
         {
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
     }*/
}
