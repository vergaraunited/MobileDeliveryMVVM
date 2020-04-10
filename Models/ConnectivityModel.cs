using MobileDeliveryMVVM.MobileDeliveryServer;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MobileDeliveryGeneral.Interfaces;
using MobileDeliveryGeneral.Settings;
using MobileDeliveryGeneral.Utilities;
using static MobileDeliveryGeneral.Definitions.MsgTypes;

namespace MobileDeliveryMVVM.Models
{
    public class ConnectivityModel : INotifyPropertyChanged
    {
        ReceiveMsgDelegate rm;
        SendMsgDelegate sm;
        SocketSettings srvSet;

        ClientSocketConnection winSys;
        ClientSocketConnection umdSrv;

        public enum ConnectState { Connected, Disconnected }
        public ConnectState IsConnectedWinSys { get; set; }
        public ConnectState IsConnectedAPI { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;        

        public ConnectivityModel(SocketSettings srv, ReceiveMsgDelegate rmCB, SendMsgDelegate smCB)
        {
            if (rmCB != null)
                rm = rmCB;
            else
                rm = new ReceiveMsgDelegate(MsgProcessor.ReceiveMessage);

            if (smCB != null)
                sm = smCB;
            else
                sm = new SendMsgDelegate(MsgProcessor.SendMessage);

            srvSet = srv;
            InitConnections();
        }
        public void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void InitConnections()
        {
            //Connect to UMD Server
            umdSrv = new ClientSocketConnection(srvSet, ref sm, rm);
            umdSrv.Connect();

            SocketSettings set = new SocketSettings();
            set.url = srvSet.clienturl;
            set.port = srvSet.clientport;
            set.name = srvSet.name + "_ToWinSys";

            //Connect to WinSys Server
            winSys = new ClientSocketConnection(set, ref sm, rm);
            winSys.Connect();
        }

        public void Disconnect()
        {
            try
            {
                bool bDis=false;

                if (IsConnectedWinSys == ConnectState.Connected)
                {
                    winSys.Disconnect();
                    bDis = true;
                    IsConnectedWinSys = ConnectState.Disconnected;
                }

                if (IsConnectedAPI == ConnectState.Connected)
                {
                    bDis = true;
                    umdSrv.Disconnect();
                    IsConnectedAPI = ConnectState.Disconnected;
                }   

                if (bDis)
                    OnPropertyChanged();
            }
            catch (Exception ex) { }
        }

        public void Connect() {
            winSys.Connect();
            umdSrv.Connect();
            IsConnectedWinSys = ConnectState.Connected;
            IsConnectedAPI = ConnectState.Connected;
            OnPropertyChanged();
        }

        public bool SendMsgWinsys(manifestRequest req)
        {
            if (IsConnectedWinSys == ConnectState.Connected)
                return winSys.SendMessage(req);
            else
                return winSys.Connect();

            
        }
        public bool SendMsgUMDAPI(manifestRequest req)
        {
            if (IsConnectedAPI == ConnectState.Connected)
                return umdSrv.SendMessage(req);
            else return false;
        }

        public void Send(isaCommand cmd)
        {
            //if (IsConnected == ConnectState.Disconnected)
            //    Connect();

            //ClientSocketConnection.isaServer().SendMessage(cmd);
        }
    }
}
