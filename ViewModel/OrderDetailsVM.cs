using DataCaching.Caching;
using DataCaching.Data;
using MobileDeliveryLogger;
using MobileDeliveryMVVM.BaseClasses;
using MobileDeliveryMVVM.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using MobileDeliveryGeneral.Data;
using MobileDeliveryGeneral.Definitions;
using MobileDeliveryGeneral.Threading;
using static MobileDeliveryGeneral.Definitions.MsgTypes;
using System.Linq;
using MobileDeliveryGeneral.Settings;

namespace MobileDeliveryMVVM.ViewModel
{
    public class OrderDetailsVM : BaseViewModel<OrderDetailsData>
    {
        #region properties

        string loadOrderRequestComplete;
        public string LoadOrderRequestComplete
        {
            get { return loadOrderRequestComplete; }
            set { SetProperty<string>(ref loadOrderRequestComplete, value); }
        }

        long manifestId;
        public long ManifestId
        {
            get { return manifestId; }
            set { SetProperty<long>(ref manifestId, value); }
        }

        int dspseq;
        public int DSP_SEQ
        {
            get { return dspseq; }
            set { SetProperty<int>(ref dspseq, value); }
        }
        int dealerNo;
        public int DlrNo
        {
            get { return dealerNo; }
            set { SetProperty<int>(ref dealerNo, value); }
        }
        int lineCount;
        public int LineCount
        {
            get { return lineCount; }
            set { SetProperty<int>(ref lineCount, value); }
        }

        private DelegateCommand _loadCommand;
        public DelegateCommand LoadCommand
        { get { return _loadCommand ?? (_loadCommand = new DelegateCommand(OnOrderDetailsLoad)); } }

        bool loadOrderDetailsComplete;
        public bool LoadOrderDetailsComplete
        {
            get { return loadOrderDetailsComplete; }
            set { SetProperty<bool>(ref loadOrderDetailsComplete, value); }
        }
        #endregion

        ObservableCollection<OrderDetailsData> orderDetailData = new ObservableCollection<OrderDetailsData>();

        public ObservableCollection<OrderDetailsData> OrderDetails
        {
            get { return orderDetailData; }
            set { SetProperty(ref orderDetailData, value); }
        }
        #region BackgroundWorkers
        UMBackgroundWorker<OrderDetailsData> orderDetailThread;
        UMBackgroundWorker<OrderDetailsData>.ProgressChanged<OrderDetailsData> pcOrderDetails;
        #endregion
        static CacheItem<OrderDetail> orderdetaildatabase;
        public static CacheItem<OrderDetail> OrderDetailDatabase
        {
            get
            {
                if (orderdetaildatabase == null)
                {
                    orderdetaildatabase = new CacheItem<OrderDetail>(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UMDDB_OrderDetails.db3"));
                }
                return orderdetaildatabase;
            }
        }
        public OrderDetailsVM() : base(new UMDAppConfig() { AppName = "OrderDetailsVM" })
        {
            OrderDetails.CollectionChanged += (s, e) =>
            {
                OrderDetails = orderDetailData;
            };

            pcOrderDetails = new UMBackgroundWorker<OrderDetailsData>.ProgressChanged<OrderDetailsData>(ProcessMessage);
            orderDetailThread = new UMBackgroundWorker<OrderDetailsData>(new UMBackgroundWorker<OrderDetailsData>.ProgressChanged<OrderDetailsData>(pcOrderDetails), rm, sm);
        }
        void Clear()
        {
            LineCount = 0;
            loadOrderRequestComplete = "";
            LoadOrderDetailsComplete = false;
            if (orderDetailThread != null)
                dRequests.Keys.ToList().ForEach(a => orderDetailThread.Reset(a));

            OrderDetails.Clear();
        }
        //class OrdArgs { public int manId { get; set; } public int stopNo { get; set;} public int DSP_SEQ { get; set; } };

        public void OnOrderDetailsLoad(object arg)
        {
            // OrdArgs oa = new OrdArgs() { manId = ManifestId, stopNo = DSP_SEQ };
            OrderDetail oa = new OrderDetail() { ManifestId = ManifestId, DSP_SEQ = DSP_SEQ };

            if (arg != EventArgs.Empty)
                oa = (OrderDetail)arg;

            ManifestId = oa.ManifestId;
            DSP_SEQ = oa.DSP_SEQ;

            Clear();

            Logger.Debug("OnOrderDetailsLoad");
            LoadOrderDetails(new OrderDetail() { DSP_SEQ = oa.DSP_SEQ, ManifestId = oa.ManifestId });
        }
        void LoadOrderDetails(OrderDetail ord)
        {
            List<OrderDetail> ordList = OrderDetailDatabase.GetItems(ord);

            if (ordList != null && ordList.Count > 0)
            {
                //Load From Cache
                AddOrders(ordList);
            }
            else
            {
                Request req = new Request() { reqGuid = Guid.NewGuid() };
                dRequests.Add(req.reqGuid, req);
                orderDetailThread.OnStartProcess(new manifestRequest() { command = eCommand.OrderDetails, id = ord.ManifestId, Stop = ord.DSP_SEQ }, req);
            }
            //umdSrv.SendMessage(req);
        }

        void ProcessMessage(OrderDetailsData ord, Func<byte[], Task> cbsend = null)
        {
            if (ord.Command == eCommand.OrderDetailsComplete)
            {
                LoadOrderRequestComplete = ord.RequestId.ToString();
                orderDetailThread.CompleteBackgroundWorker(ord.RequestId);
                LoadOrderDetailsComplete = true;
                return;
            }

            OrderDetailDatabase.SaveItem(new OrderDetail(ord));
            AddOrder(ord);
        }
        void AddOrders(List<OrderDetail> orders)
        {
            foreach (var ord in orders)
            {
                AddOrder(ord.OrderData());
            }
        }
        void AddOrder(OrderDetailsData od)
        {
            LineCount++;
            //Order ord = new Order(od);
            if (!OrderDetails.Contains(od))
            {
                OrderDetails.Add(od);
            }
            else
            {
                OrderDetails.Add(od);
                OrderDetails.Remove(od);
                OrderDetails.Add(od);
            }
        }
        public override isaCommand ReceiveMessageCB(isaCommand cmd)
        {
            switch (cmd.command)
            {
                case eCommand.OrderDetails:
                case eCommand.OrdersLoad:
                    orderDetailThread.ReportProgress(50, new object[] { cmd });
                    break;
                case eCommand.OrderDetailsComplete:
                    LoadOrderDetailsComplete = true;
                    orderDetailThread.ReportProgress(100, new object[] { cmd });
                    break;
                default:
                    Logger.Debug("OrderDetailsVM::ReceiveMessageCB - Unhandled message.");
                    break;
            }
            return cmd;
        }

    }
}
