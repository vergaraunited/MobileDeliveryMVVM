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

        
        bool loadOrdersComplete;
        public bool LoadOrdersComplete
        {
            get { return loadOrdersComplete; } set { SetProperty<bool>(ref loadOrdersComplete, value); }
        }
        #endregion

        ObservableCollection<OrderData> orderData = new ObservableCollection<OrderData>();
        
        public ObservableCollection<OrderData> Orders
        {
            get { return orderData; } set{ SetProperty(ref orderData, value);}
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
        void Clear(bool bFullRefresh=false)
        {
            loadOrderRequestComplete = "";
            LoadOrdersComplete = false;
            if (orderThread != null)
                dRequests.Keys.ToList().ForEach(a => orderThread.Reset(a));

            if (bFullRefresh)
            {
                LineCount = 0;
                orderData.Clear();
            }

        }

        public void OnCompleteStop(object arg)
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
                var lstSelectedOrders = Orders.Select(a => a).Where(b => b.IsSelected == true && OrderStatus.Delivered.CompareTo(b.Status)!=0).ToList();
                foreach (var mm in lstSelectedOrders)
                {
                    mm.status = status.Releasing;
                    UploadOrderData(mm);
                }
            }
        }
        void ResetOrder(OrderData ord)
        {
            ord.prevstate = ord.IsSelected;
            ord.Command = eCommand.LoadFilesComplete;
            //AddOrder(ord);
        }
        void UploadOrderData(OrderData ord)
        {
            Logger.Info($"OrderVM UploadStopOrder: {ord.ToString()}");
            OrderData od = null;
            if (Orders.Contains(ord))
                foreach (var odi in Orders.Select(a => a).Where(o => o == ord))
                    od = odi;

            if (ord.status == status.Pending && od != null && od.IsSelected == true) //|| ord.status == status.Pending)
            {
                ResetOrder(od);
                return;
            }
            if (ord.Status == OrderStatus.Delivered && ord.Command == eCommand.OrdersLoad &&
                !( (!ord.IsSelected && ord.prevstate) ^ (ord.IsSelected && !ord.prevstate)) )
            {
                ResetOrder(ord);
                return;
            }

            if ((
                ((ord.IsSelected && !ord.prevstate) && ord.Status == OrderStatus.Delivered) || 
                ((!ord.IsSelected && ord.prevstate) && ord.Status == OrderStatus.Shipped )
                && ord.Command == eCommand.OrdersLoad))
            {
                ResetOrder(ord);
                //return;
            }

            var req = new Request()
            {
                reqGuid = NewGuid(),
                LIds = new Dictionary<long, status>(),
                LinkMid = new Dictionary<long, List<long>>(),
                ChkIds = new Dictionary<long, status>()
            };
            dRequests.Add(req.reqGuid, req);

            orderMaster ordUp = new orderMaster(ord, ord.ManifestId);

            if (ord.IsSelected && !ord.prevstate)
                ordUp.Status = OrderStatus.Shipped;
            else if (!ord.IsSelected && ord.prevstate)
                ordUp.Status = OrderStatus.Delivered;

            if ((ord.IsSelected && !ord.prevstate) || (!ord.IsSelected && ord.prevstate))
            {
                ordUp.command = eCommand.CompleteStop;

                ProcessMsgDelegateRXRaw pmRx = new ProcessMsgDelegateRXRaw(ProcessMessage);
                Logger.Info($"Upload Sopt Order reqid: {req.reqGuid}");
                orderThread.OnStartProcess((new manifestRequest()
                {
                    requestId = req.reqGuid.ToByteArray(),
                    command = ordUp.command,
                    bData = ordUp.ToArray()
                }), req, pmRx);
            }
        }


        public void ProcessMessage(byte[] bcmd, Func<byte[], Task> cbsend = null)
        { }

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
                OrderMasterData omd = (OrderMasterData)ord;
                OrderData od1=null;
                foreach (OrderData od in Orders.Select(a => a).Where(b => b.ORD_NO == omd.ORD_NO).ToList())
                {
                    od1 = od;
                }
                if (od1 != null)
                {
                    lock (olock)
                    {
                        Orders.Remove(od1);

                        Logger.Info($"OrderVM ProcessMessage: replace order in collection {od1.ToString()}");
                        od1.Status = ((OrderMasterData)ord).Status;
                        AddOrder(od1);
                    }
                }
            }
            else
            {
                ord.status = status.Uploaded;
                //OrderDatabase.SaveItem(new Order(ord));
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
                //OrderDatabase.SaveItem(new Order(od));

                if (!Orders.Contains(od))
                {
                    if (od.Status != OrderStatus.Delivered)
                    {
                        od.status = status.Init;
                        Orders.Add(od);
                    }
                    else
                    {
                        if (CompletedOrders.Contains(od))
                            CompletedOrders.Remove(od);
                        CompletedOrders.Add(od);
                    }
                }
                else
                {
                    Orders.Remove(od);

                    if (od.Status != OrderStatus.Delivered)
                    {
                        od.status = status.Init;
                        Orders.Add(od);
                    }
                    else
                    {
                        if (CompletedOrders.Contains(od))
                            CompletedOrders.Remove(od);
                        od.status = status.Completed;
                        CompletedOrders.Add(od);
                    }
                }
                od.OnSelectionChanged = new OrderData.cmdFireOnSelected(UploadOrderData);
            }
        }
        public override isaCommand ReceiveMessageCB(isaCommand cmd)
        {
            switch (cmd.command)
            {
                case eCommand.Orders:
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
