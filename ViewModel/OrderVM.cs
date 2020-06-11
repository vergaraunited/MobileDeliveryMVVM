using DataCaching.Caching;
using DataCaching.Data;
using MobileDeliveryLogger;
using MobileDeliveryMVVM.BaseClasses;
using MobileDeliveryMVVM.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MobileDeliveryGeneral.Data;
using MobileDeliveryGeneral.Definitions;
using MobileDeliveryGeneral.Threading;
using static MobileDeliveryGeneral.Definitions.MsgTypes;
using System.Linq;
using MobileDeliveryGeneral.Settings;
using MobileDeliveryGeneral.Interfaces;
using MobileDeliveryGeneral.Interfaces.DataInterfaces;
using MobileDeliverySettings;

namespace MobileDeliveryMVVM.ViewModel
{
    public class OrderVM : BaseViewModel<OrderMasterData>
    {
        #region properties
        // It's a backup variable for storing CountryViewModel objects
        // private ObservableCollection<OrderDetailsVM> orderDetailsVM = new ObservableCollection<OrderDetailsVM>();

        object olock = new object();

        //private DelegateCommand completeStopCommand;
        //public DelegateCommand CompleteStopCommand
        //{ get { return completeStopCommand ?? (completeStopCommand = new DelegateCommand(OnCompleteStop)); } }


        string loadOrderRequestComplete;
        public string LoadOrderRequestComplete {
            get { return loadOrderRequestComplete; } set { SetProperty<string>(ref loadOrderRequestComplete, value); }
        }

        long manifestId;
        public long ManifestId
        {
            get { return manifestId; } set { SetProperty<long>(ref manifestId, value); }
        }

        int dspseq;
        public int DSP_SEQ
        {
            get { return dspseq; } set { SetProperty<int>(ref dspseq, value); }
        }
        string dealerName;
        public string DlrName
        {
            get { return dealerName; }
            set { SetProperty<string>(ref dealerName, value); }
        }
        int dealerNo;
        public int DlrNo
        {
            get { return dealerNo; } set { SetProperty<int>(ref dealerNo, value); }
        }
        int lineCount;
        public int LineCount
        {
            get { return lineCount; } set { SetProperty<int>(ref lineCount, value); }
        }

        DateTime shipDate;
        public DateTime ShipDate
        {
            get { return shipDate; }
            set
            {
                SetProperty<DateTime>(ref shipDate, value);
            }
        }

        private DelegateCommand _loadCommand;
        public DelegateCommand LoadCommand
        { get { return _loadCommand ?? (_loadCommand = new DelegateCommand(OnOrdersLoad)); } }


        private DelegateCommand _closeStopCommand;
        public DelegateCommand CloseStopCommand
        { get { return _closeStopCommand ?? (_closeStopCommand = new DelegateCommand(OnCloseStop)); } }


        bool loadOrdersComplete;
        public bool LoadOrdersComplete
        {
            get { return loadOrdersComplete; } set { SetProperty<bool>(ref loadOrdersComplete, value); }
        }
        #endregion

        ObservableCollection<OrderData> orderData = new ObservableCollection<OrderData>();

        public ObservableCollection<OrderData> Orders
        {
            get { return orderData; } set { SetProperty(ref orderData, value); }
        }


        ObservableCollection<OrderData> completedOrderData = new ObservableCollection<OrderData>();

        public ObservableCollection<OrderData> CompletedOrders
        {
            get { return completedOrderData; }
            set { SetProperty(ref completedOrderData, value); }
        }

        #region BackgroundWorkers
        UMBackgroundWorker<IMDMMessage> orderThread;
        UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage> pcOrders;
        #endregion
        static CacheItem<Order> orderdatabase;
        public static CacheItem<Order> OrderDatabase
        {
            get
            {
                if (orderdatabase == null)
                {
                    orderdatabase = new CacheItem<Order>(Settings.OrderCachePath);
                }
                return orderdatabase;
            }
        }
        public OrderVM() : base(new SocketSettings()
        {
            url = "localhost",
            port = 81,
            srvurl = "localhost",
            srvport = 81,
            clienturl = "localhost",
            clientport = 8181,
            name = "OrderVM"
        }, "OrderVM")
        {
            Orders.CollectionChanged += (s, e) =>
            {
                Orders = orderData;
            };
            CompletedOrders.CollectionChanged += (s, e) =>
            {
                CompletedOrders = completedOrderData;
            };

            pcOrders = new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(ProcessMessage);
            orderThread = new UMBackgroundWorker<IMDMMessage>(new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(pcOrders), rm, sm);
        }
        void Clear(bool bFullRefresh = false)
        {
            loadOrderRequestComplete = "";
            LoadOrdersComplete = false;
            if (orderThread != null)
                dRequests.Keys.ToList().ForEach(a => orderThread.Reset(a));

            dShippedRequestPending.Clear();
            dDeliveredRequestPending.Clear();

            if (bFullRefresh)
            {
                LineCount = 0;
                orderData.Clear();
            }

        }

        public void OnCompleteOrder(object arg)
        {
            if (arg != null)
            {
                //Winform Manifest Master release
                Order ord = (Order)arg;
                if (ord.RequestId == null || ord.RequestId == Guid.Empty)
                    ord.RequestId = NewGuid();
                ord.status = status.Releasing;
                UploadOrderData(ord.OrderData());
            }
            else
            {
                var lstSelectedOrders = Orders.Select(a => a).Where(b => b.IsSelected == true && OrderStatus.Delivered.CompareTo(b.Status) != 0).ToList();
                foreach (var mm in lstSelectedOrders)
                {
                    mm.status = status.Releasing;
                    UploadOrderData(mm);
                }
            }
        }
        void ResetOrder(OrderData ord)
        {
            //bool tprevst = ord.prevstate;
            //ord.prevstate = ord.IsSelected;
            //ord.IsSelected = tprevst;
            ord.Command = eCommand.LoadFilesComplete;
            //AddOrder(ord);
        }
        List<long> dDeliveredRequestPending = new List<long>();
        List<long> dShippedRequestPending = new List<long>();

        void UploadOrderData(OrderData ord)
        {
            Logger.Info($"OrderVM UploadStopOrder: {ord.ToString()}");
            bool bExists = Orders.Select(a => a).Where(o => o == ord).ToList().Count>0;
            bool bCompExists = CompletedOrders.Select(a => a).Where(o => o == ord).ToList().Count > 0;

            if (((ord.Status == OrderStatus.Shipped && ord.IsSelected==false) && (bExists && !bCompExists) || dDeliveredRequestPending.Contains(ord.ORD_NO)) || (
                ord.Status == OrderStatus.Delivered && ord.IsSelected) && (!bExists && bCompExists) || dShippedRequestPending.Contains(ord.ORD_NO))
                return;
            
            var req = new Request()
            {
                reqGuid = NewGuid(),
                LIds = new Dictionary<long, status>(),
                LinkMid = new Dictionary<long, List<long>>(),
                ChkIds = new Dictionary<long, status>()
            };
            dRequests.Add(req.reqGuid, req);
            bool bShipped = !ord.IsSelected && ord.prevstate;
            bool bDelivered = ord.IsSelected && !ord.prevstate;

            if (bShipped)
            {
                ord.Status = OrderStatus.Shipped;
                ord.status = status.Releasing;
                //ord.IsSelected = false;
            }
            else if (bDelivered)
            { 
                ord.Status = OrderStatus.Delivered;
                ord.status = status.Completed;
            }
            orderMaster ordUp = new orderMaster(ord, ord.ManifestId);

            if (bShipped || bDelivered)
            {
                ordUp.command = eCommand.CompleteOrder;

                ProcessMsgDelegateRXRaw pmRx = new ProcessMsgDelegateRXRaw(ProcessMessage);
                Logger.Info($"Upload Sopt Order reqid: {req.reqGuid}");
                orderThread.OnStartProcess((new manifestRequest()
                {
                    requestId = req.reqGuid.ToByteArray(),
                    command = ordUp.command,
                    bData = ordUp.ToArray()
                }), req, pmRx);

                lock (olock)
                {
                    if (bShipped)
                        dShippedRequestPending.Add(ord.ORD_NO);
                    else if (bDelivered)
                        dDeliveredRequestPending.Add(ord.ORD_NO);
                }
            }
        }

        public void ProcessMessage(byte[] bcmd, Func<byte[], Task> cbsend = null)
        { }

        public void OnCloseStop(object arg)
        {
            Request req = new Request() { reqGuid = NewGuid() };
            dRequests.Add(req.reqGuid, req);
          //  orderThread.OnStartProcess(new manifestRequest() { command = eCommand.CompleteOrder, id = ord.ManifestId, Stop = ord.DSP_SEQ }, req);

        }
        public void OnOrdersLoad(object arg)
        {
            Order oa = new Order(){ ManifestId = ManifestId, DSP_SEQ = DSP_SEQ };

            if (arg != EventArgs.Empty)
                oa = (Order)arg;

            ManifestId = oa.ManifestId;
            DSP_SEQ = oa.DSP_SEQ;
            DlrName = "";
            Clear(true);

            Logger.Debug("OnOrdersLoad");
            LoadOrders(new Order() { status = status.Init, DSP_SEQ = oa.DSP_SEQ, ManifestId = oa.ManifestId } );
        }
        void LoadOrders(Order ord)
        {
            if(ord.Command == eCommand.GenerateManifest)
            {
                Request req = new Request() { reqGuid = NewGuid() };
                dRequests.Add(req.reqGuid, req);
                orderThread.OnStartProcess(new manifestRequest() { command = eCommand.OrdersLoad, id = ord.ManifestId, Stop = ord.DSP_SEQ }, req);
            }
            else
            {
                List<Order> ordList = OrderDatabase.GetItems(ord);
                AddOrders(ordList);
            }
        }

        void ProcessMessage(IMDMMessage ord, Func<byte[], Task> cbsend = null)
        {
            if (ord.Command == eCommand.OrdersLoadComplete)
            {
                LoadOrderRequestComplete = ord.RequestId.ToString();
                Logger.Info($"OrdersLoadComplete: {ord.ToString()}");
                LoadOrdersComplete = true;
                return;
            }

            // If we are Completing a stop the SP returns a OrderMasterData object
            if (ord.GetType() == typeof(OrderMasterData))
            {
                AddOrder(((OrderMasterData)ord).GetOrderData());
            }
            else
            {
                ord.status = status.Uploaded;
                AddOrder((OrderData)ord);
            }
        }
        void AddOrders(List<Order> orders)
        {
            foreach( var ord in orders)
                AddOrder(ord.OrderData());
        }
        void AddOrder(OrderData od)
        {
            LineCount++;
            lock (olock)
            {
                List<OrderData> lstOrdData = Orders.Select(a => a).Where(b => b.ORD_NO == od.ORD_NO).ToList();
                List<OrderData> lstCompOrdData = CompletedOrders.Select(a => a).Where(b => b.ORD_NO == od.ORD_NO).ToList();

                OrderData tmpod = null;

                if (lstOrdData.Count > 0)
                    tmpod = lstOrdData[0];
                else if (lstCompOrdData.Count > 0)
                    tmpod = lstCompOrdData[0];

                foreach (var o in lstOrdData)
                {
                    LineCount--;
                    Orders.Remove(o);
                }
                foreach (var c in lstCompOrdData)
                {
                    LineCount--;
                    CompletedOrders.Remove(c);
                }
                
                //OrderDatabase.SaveItem(new Order(od));

                if (tmpod != null && tmpod.Status == OrderStatus.Delivered)
                {
                    tmpod.status = status.Completed;
                    tmpod.IsSelected = true;
                    CompletedOrders.Add(tmpod);

                    if (tmpod.OnSelectionChanged == null)
                        tmpod.OnSelectionChanged = new OrderData.cmdFireOnSelected(UploadOrderData);
                }
                else if (tmpod != null && tmpod.Status == OrderStatus.Shipped)
                {
                    tmpod.status = status.Completed;
                    tmpod.IsSelected = false;
                    Orders.Add(tmpod);

                    if (tmpod.OnSelectionChanged == null)
                        tmpod.OnSelectionChanged = new OrderData.cmdFireOnSelected(UploadOrderData);
                }
                else //add to orders - new item
                {
                    if (od.Status == OrderStatus.Shipped)
                    {
                        od.status = status.Completed;
                        od.IsSelected = false;
                        Orders.Add(od);
                    }
                    else if (od.Status == OrderStatus.Delivered)
                    {
                        od.status = status.Completed;
                        od.IsSelected = true;
                        CompletedOrders.Add(od);
                    }
                    else
                    {
                        od.status = status.Completed;
                       // od.Status = OrderStatus.Shipped;
                        od.IsSelected = false;
                        Orders.Add(od);
                    }
                    if (dShippedRequestPending.Contains(od.ORD_NO))
                        dShippedRequestPending.Remove(od.ORD_NO);
                    if (dDeliveredRequestPending.Contains(od.ORD_NO))
                        dDeliveredRequestPending.Remove(od.ORD_NO);
                    if (od.OnSelectionChanged == null)
                        od.OnSelectionChanged = new OrderData.cmdFireOnSelected(UploadOrderData);
                }
                
            }
        }
        public override isaCommand ReceiveMessageCB(isaCommand cmd)
        {
            switch (cmd.command)
            {
                case eCommand.OrdersUpload:
                case eCommand.OrdersLoad:
                    orderThread.ReportProgress(50, new object[] { cmd });
                    break;
                case eCommand.OrdersLoadComplete:
                    LoadOrdersComplete = true;
                    orderThread.ReportProgress(100, new object[] { cmd });
                    break;
                default:
                    Logger.Debug("OrderVM::ReceiveMessageCB - Unhandled message.");
                    break;
            }
            return cmd;
        }

    }
}
