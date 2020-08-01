using MobileDeliveryGeneral.Data;
using MobileDeliveryGeneral.Definitions;
using MobileDeliveryGeneral.Interfaces.DataInterfaces;
using MobileDeliveryGeneral.Threading;
using MobileDeliveryMVVM.BaseClasses;
using MobileDeliveryMVVM.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        int manId;
        public Int32 ManId
        {
            get { return manId; }
            set { SetProperty<int>(ref manId, value); }
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
               //// mreq.bData = Signature;
                if (mreq.valist == null)
                    mreq.valist = new List<long>();
                mreq.valist.Add(StopOrder.ORD_NO);
                //mreq.id = StopOrder.MAN_ID;
                mreq.id = ManId;
               // //We need the DSP_SEQ from the scnfle ORD_NO List
               // // mreq.DATA = StopOrder.DSP_SEQ;
               //// StopOrder.Status;
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

        protected override isaCommand ReceiveMessage(isaCommand msg)
        {
            throw new NotImplementedException();
        }

        ObservableCollection<OrderDetailsModelData> orders = new ObservableCollection<OrderDetailsModelData>();
        public ObservableCollection<OrderDetailsModelData> ShippedOrderCollection
        {
            get { return orders; }
            set
            {
                SetProperty(ref orders, value);
            }
        }
        ObservableCollection<OrderDetailsModelData> borders = new ObservableCollection<OrderDetailsModelData>();
        public ObservableCollection<OrderDetailsModelData> BackOrderCollection
        {
            get { return borders; }
            set
            {
                SetProperty(ref borders, value);
            }
        }

        public CloseStopVM() : base()
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

            //pcStops = new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(ProcessMessage);
            //stopThread = new UMBackgroundWorker<IMDMMessage>(new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(pcStops), rm, sm);

        }

        //protected override void ProcessMessageUMD(IMDMMessage std, Func<byte[], Task> cbsend = null)
        //{
        //    if (std.Command == eCommand.StopsLoadComplete)
        //    {
        //        //stopThread.CompleteBackgroundWorker(std.RequestId);
        //        //LoadStopsComplete = true;
        //        return;
        //    }

        //    //AddStop((StopData)std);
        //}

        //public CloseStopVM() : base()
        //{

        //    pcStops = new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(ProcessMessage);
        //    stopThread = new UMBackgroundWorker<IMDMMessage>(new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(pcStops), rm, sm);

        //}

    }
}