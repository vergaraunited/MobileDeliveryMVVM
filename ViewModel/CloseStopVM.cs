﻿using MobileDeliveryGeneral.Data;
using MobileDeliveryGeneral.Definitions;
using MobileDeliveryGeneral.Interfaces.DataInterfaces;
using MobileDeliveryGeneral.Settings;
using MobileDeliveryGeneral.Threading;
using MobileDeliveryMVVM.BaseClasses;
using MobileDeliveryMVVM.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using static MobileDeliveryGeneral.Definitions.MsgTypes;

namespace MobileDeliveryMVVM.ViewModel
{
    public class CloseStopVM : BaseViewModel<OrderData>
    {
        public Func<Task<byte[]>> SignatureFromStream { get; set; }
        public byte[] Signature { get; set; }

        public ICommand SaveSignature => new AsyncDelegateCommand(async () =>
        {
            Signature = await SignatureFromStream();
            // Signature should be != null
        });

        string name;
        public string Name
        {
            get { return name; }
            set { SetProperty<string>(ref name, value); }
        }

        private DelegateCommand _confirmPODCommand;
        public DelegateCommand ConfirmPODCommand
        { get { return _confirmPODCommand ?? (_confirmPODCommand = new DelegateCommand(OnSavePOD)); } }
        
        #region BackgroundWorkers
        UMBackgroundWorker<IMDMMessage> stopThread;
        UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage> pcStops;
        #endregion

        private void OnSavePOD(object obj)
        {
            manifestRequest mreq = new manifestRequest();
            mreq.command = eCommand.CompleteStop;

           // Stream bitmap =  Signature.GetImageStreamAsync(SignatureImageFormat.Png);

            foreach (var StopOrder in ShippedOrderCollection)
            {
               // mreq.bData = Signature;
                if (mreq.valist == null)
                    mreq.valist = new List<long>();
                mreq.valist.Add(StopOrder.ORD_NO);
                mreq.id = StopOrder.ManifestId;
                //We need the DSP_SEQ from the scnfle ORD_NO List
                // mreq.DATA = StopOrder.DSP_SEQ;
               // StopOrder.Status;
            }
            mreq.bData = Signature;
            Request reqInfo = new Request()
            {
                reqGuid = NewGuid(),
                LIds = new Dictionary<long, status>(),
                LinkMid = new Dictionary<long, List<long>>()
            };

            stopThread.OnStartProcess(mreq, reqInfo);

        }

        ObservableCollection<OrderData> orders = new ObservableCollection<OrderData>();
        public ObservableCollection<OrderData> ShippedOrderCollection
        {
            get { return orders; }
            set
            {
                SetProperty(ref orders, value);
            }
        }
        ObservableCollection<OrderData> borders = new ObservableCollection<OrderData>();
        public ObservableCollection<OrderData> BackOrderCollection
        {
            get { return borders; }
            set
            {
                SetProperty(ref borders, value);
            }
        }

        public CloseStopVM(SocketSettings set, string name) : base(set, name)
        {
            Name = name;
            ShippedOrderCollection.CollectionChanged += (s, e) =>
            {
                ShippedOrderCollection = orders;
            };
            BackOrderCollection.CollectionChanged += (s, e) =>
            {
                BackOrderCollection = borders;
            };

            pcStops = new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(ProcessMessage);
            stopThread = new UMBackgroundWorker<IMDMMessage>(new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(pcStops), rm, sm);

        }

        void ProcessMessage(IMDMMessage std, Func<byte[], Task> cbsend = null)
        {
            if (std.Command == eCommand.StopsLoadComplete)
            {
                //stopThread.CompleteBackgroundWorker(std.RequestId);
                //LoadStopsComplete = true;
                return;
            }

            //AddStop((StopData)std);
        }

        public CloseStopVM() : base(new SocketSettings()
        {
            url = "localhost",
            port = 81,
            srvurl = "localhost",
            srvport = 81,
            clienturl = "localhost",
            clientport = 8181,
            name = "CloseStopVM"
        }, "CloseStopVM")
        {

            pcStops = new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(ProcessMessage);
            stopThread = new UMBackgroundWorker<IMDMMessage>(new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(pcStops), rm, sm);

        }

    }
}