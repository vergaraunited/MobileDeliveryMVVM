//using MobileDeliveryClient.API;
using MobileDeliveryLogger;
using System.Threading.Tasks;
using MobileDeliveryGeneral.Definitions;
using MobileDeliveryGeneral.Interfaces;
using MobileDeliveryGeneral.Settings;
using MobileDeliveryGeneral.Utilities;
using static MobileDeliveryGeneral.Definitions.MsgTypes;

namespace MobileDeliveryMVVM.MobileDeliveryServer
{
    public class ClientSocketConnection : isaMobileDeliveryClient
    {
        const ushort defPort = 8181;
        UMDServerConnection srvr;
        ReceiveMessages rmsg;
        SendMessages smsg;
        //SendMsgDelegate msgSnd;
        public string Url { get; set; }
        public ushort Port { get; set; }
        public string name { get; set; }

        public ClientSocketConnection(SocketSettings srvSet, ref SendMsgDelegate sm, ReceiveMsgDelegate rm = null)
        { Init(srvSet, ref sm, rm); }

        public void Init(SocketSettings srvSet, ref SendMsgDelegate sm, ReceiveMsgDelegate rm = null)
        {
            Url = srvSet.url;
            Port = srvSet.port;
            name = srvSet.name;

            Logger.Info("Client Socket Connection Init: " + name);

            if (rm == null)
                rm = new ReceiveMsgDelegate(MsgProcessor.ReceiveMessage);

            srvr = new UMDServerConnection(srvSet, ref sm, rm);
            smsg = new SendMessages(sm);
        }

        public bool Connect()
        {
            Logger.Debug("Client Socket Connect " + name);
            var task = Task.Run(async () => srvr.Connect());
          
            return true;
        }

        public bool Disconnect()
        {
            Logger.Debug("Client Socket Disconnect " + name);
            srvr.Disconnect();
            return true;
        }

        public isaCommand ReceiveMessage(isaCommand cmd)
        {
            switch (cmd.command)
            {
                case eCommand.Ping:
                    Logger.Debug("Command Ping recevied!");
                    SendMessage(new MsgTypes.Command { command = eCommand.Pong });
                    break;
                case eCommand.Pong:
                    Logger.Debug("Command Pong recevied!");
                    break;
                case eCommand.Manifest:
                    //msg.ReceiveMessage(cmd);
                    break;
                default:
                    //msg.ReceiveMessage(cmd);
                    break;
            }
            return cmd;
        }

        public bool SendMessage(isaCommand cmd)
        {
            return smsg.SendMessage(cmd);
        }
    }
}
