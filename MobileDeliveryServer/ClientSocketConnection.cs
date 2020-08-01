namespace MobileDeliveryMVVM.MobileDeliveryServer
{/*
    public class ClientSocketConnection : isaMobileDeliveryClient
    {
        const ushort defPort = 8181;
        //ClientToServerConnection umdsrv;
        ReceiveMessages rm;
        SendMessages sm;
        ClientToServerConnection srv; 
        public string name { get; private set; }
        public SocketSettings socSet { get; set; }
        public bool IsConnected {
            get { return srv.IsConnected; } private set { } }

        public ClientSocketConnection(SocketSettings sockSet, ref SendMsgDelegate sm, ReceiveMsgDelegate rm = null)
        { Init(sockSet, ref sm, rm); }

        public void Init(SocketSettings sockSet, ref SendMsgDelegate smd, ReceiveMsgDelegate rmd = null)
        {
            if (srv == null)
            {
                name = sockSet.name;
                this.socSet = sockSet;
                Logger.Info($"Client Socket Connection Init: {name}");

                if (rmd == null)
                    rmd = new ReceiveMsgDelegate(MsgProcessor.ReceiveMessage);

                srv = new ClientToServerConnection(sockSet, ref smd, rmd);
                name = srv.name;
                sm = new SendMessages(smd);
            }
        }

        public void ReInit(ref SendMsgDelegate smd)
        {
            if (smd == null)
            {
                SendMsgDelegate msgSend = new SendMsgDelegate(SendMessage);
                smd = msgSend;
            }
            sm = new SendMessages(smd);
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
            var rmd = new ReceiveMsgDelegate(ReceiveMessage);
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
            var rm = new ReceiveMsgDelegate(ReceiveMessage);
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
            var conn = new ClientToServerConnection(config.srvSet, ref sm, rm, ev);
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
    }*/
}
