using MobileDeliveryLogger;
using System.Threading.Tasks;
using MobileDeliveryGeneral.Definitions;
using MobileDeliveryGeneral.Interfaces;
using MobileDeliveryGeneral.Settings;
using MobileDeliveryGeneral.Utilities;
using static MobileDeliveryGeneral.Definitions.MsgTypes;
using MobileDeliveryClient.API;
using MobileDeliverySettings.Settings;
using System;

namespace MobileDeliveryMVVM.MobileDeliveryServer
{
    public class ClientSocketConnection : isaMobileDeliveryClient
    {
        const ushort defPort = 8181;
        //ClientToServerConnection umdsrv;
        ReceiveMessages rmsg;
        SendMessages smsg;
        ClientToServerConnection srv; 
        public string name { get; private set; }
        public SocketSettings socSet { get; set; }
        public bool IsConnected {
            get { return srv.IsConnected; } private set { } }

        public ClientSocketConnection(SocketSettings sockSet, ref SendMsgDelegate sm, ReceiveMsgDelegate rm = null)
        { Init(sockSet, ref sm, rm); }

        public void Init(SocketSettings sockSet, ref SendMsgDelegate sm, ReceiveMsgDelegate rm = null)
        {
            if (srv == null)
            {
                name = sockSet.name;
                this.socSet = sockSet;
                Logger.Info($"Client Socket Connection Init: {name}");

                if (rm == null)
                    rm = new ReceiveMsgDelegate(MsgProcessor.ReceiveMessage);

                srv = new ClientToServerConnection(sockSet, ref sm, rm);
                name = srv.name;
                smsgWinsys = new SendMessages(sm);
            }
        }

        public void ReInit(ref SendMsgDelegate sm)
        {
            if (sm == null)
            {
                SendMsgDelegate msgSend = new SendMsgDelegate(SendMessage);
                sm = msgSend;
            }
            smsg = new SendMessages(sm);
        }

        public bool Connect()
        {
            Logger.Info($"Client {name} Socket Connecting to Server ws://{socSet.srvurl}:{socSet.srvport}.");
            var task = Task.Run(async () => srv.Connect());
            var task2 = Task.Run(async () => srv.Connect());
            return true;
        }

        void ConnectToWinsys(UMDAppConfig config, ev_name_hook e)
        {
            rm = new ReceiveMsgDelegate(ReceiveMessage);
            //pmRx = new ProcessMsgDelegateRXRaw(HandleClientCmd);
            string AppName = config.AppName;
            Logger.Debug($"{config.AppName} Connection init");

            if (config.srvSet == null)
            {
                Logger.Error($"{config.AppName} Missing Configuration Server Settings");
                throw new Exception($"{config.AppName} Missing Configuration Server Settings.");
            }

            //Connecting to the WinsysAPI
            config.srvSet.url = config.srvSet.clienturl;
            config.srvSet.port = config.srvSet.clientport;
            config.srvSet.name += " as a client To WinSys server.";
            winsysconn = new ClientToServerConnection(config.srvSet, ref sm, rm, ev);
            winsysconn.Connect();
        }

        void ConnectToUMDSrv(UMDAppConfig config, ev_name_hook e)
        {
            rm = new ReceiveMsgDelegate(ReceiveMessage);
            //pmRx = new ProcessMsgDelegateRXRaw(HandleClientCmd);
            AppName = config.AppName;
            Logger.Debug($"{config.AppName} Connection init");

            if (config.srvSet == null)
            {
                Logger.Error($"{config.AppName} Missing Configuration Server Settings");
                throw new Exception($"{config.AppName} Missing Configuration Server Settings.");
            }

            //Connecting to the WinsysAPI
            config.srvSet.url = config.srvSet.clienturl;
            config.srvSet.port = config.srvSet.clientport;
            config.srvSet.name += " as a client To WinSys server.";
            conn = new ClientToServerConnection(config.srvSet, ref sm, rm, ev);
            conn.Connect();
        }

        public bool Disconnect()
        {
            Logger.Info($"Client Socket {name} Disconnect from Server ws://{socSet.srvurl}:{socSet.srvport}.");
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
