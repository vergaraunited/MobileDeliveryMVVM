﻿using MobileDeliveryLogger;
using System.Threading.Tasks;
using MobileDeliveryGeneral.Definitions;
using MobileDeliveryGeneral.Interfaces;
using MobileDeliveryGeneral.Settings;
using MobileDeliveryGeneral.Utilities;
using static MobileDeliveryGeneral.Definitions.MsgTypes;
using MobileDeliveryClient.API;

namespace MobileDeliveryMVVM.MobileDeliveryServer
{
    public class ClientSocketConnection : isaMobileDeliveryClient
    {
        const ushort defPort = 8181;
        ClientToServerConnection srvr;
        ReceiveMessages rmsg;
        SendMessages smsg;
        public string Url { get; set; }
        public ushort Port { get; set; }
        public string name { get; set; }
        SocketSettings sockSet;

        public ClientSocketConnection(SocketSettings sockSet, ref SendMsgDelegate sm, ReceiveMsgDelegate rm = null)
        { Init(sockSet, ref sm, rm); }

        public void Init(SocketSettings sockSet, ref SendMsgDelegate sm, ReceiveMsgDelegate rm = null)
        {
            Url = sockSet.url;
            Port = sockSet.port;
            name = sockSet.name;
            this.sockSet = sockSet;
            Logger.Info($"Client Socket Connection Init: {name}");

            if (rm == null)
                rm = new ReceiveMsgDelegate(MsgProcessor.ReceiveMessage);

            srvr = new ClientToServerConnection(sockSet, ref sm, rm);
            smsg = new SendMessages(sm);
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
            Logger.Info($"Client {name} Socket Connecting to Server ws://{Url}:{Port}.");
            var task = Task.Run(async () => srvr.Connect());
          
            return true;
        }

        public bool Disconnect()
        {
            Logger.Info($"Client Socket {name} Disconnect from Server ws://{Url}:{Port}.");
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
