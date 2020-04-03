using MobileDeliveryMVVM.BaseClasses;
using MobileDeliveryMVVM.Command;
using MobileDeliveryMVVM.Models;
using MobileDeliveryGeneral.Interfaces;
using MobileDeliveryGeneral.Settings;
using static MobileDeliveryGeneral.Definitions.MsgTypes;

namespace MobileDeliveryMVVM.ViewModel
{
    public class ConnectionVM// : ViewModelBase
    {
        ConnectivityModel connectivityModel;
        SocketSettings srv;
        ReceiveMsgDelegate rm;
        SendMsgDelegate sm;

       // static UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage> main = null;
        public ConnectionVM(SocketSettings srvSet = null, ReceiveMsgDelegate rmsg=null, SendMsgDelegate smsg=null)// :base(main, "ConnectionVM")
        {

            if (srvSet != null)
                srv = srvSet;

            if (rmsg != null)
                rm = rmsg;

            if (smsg != null)
                sm = smsg;

            connectivityModel = new ConnectivityModel(srv, rm, sm);
        }

        ~ConnectionVM()
        {

        }
        private void OnConnect()
        {
            
            //InitConnections();
        }

        protected void InitConnections()
        {
            //Connect to WinSys Server

        }

        public void Disconnect()
        {
            connectivityModel.Disconnect();
        }
        public ICommand ConnectCommand { get; set; }
        public ICommand DisonnectCommand { get; set; }

        //public ICommand IsConnectedWinSys = ConnectState.Connected;


        public bool SendMsgWinsys(manifestRequest req)
        {
            return connectivityModel.SendMsgWinsys(req);
        }
        public bool SendMsgUMDAPI(manifestRequest req)
        {
            return connectivityModel.SendMsgUMDAPI(req);
        }

        //private void OnConnect() { }
    }
}