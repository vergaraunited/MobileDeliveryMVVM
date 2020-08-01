using MobileDeliveryLogger;
using MobileDeliveryMVVM.BaseClasses;
using MobileDeliveryMVVM.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MobileDeliveryGeneral.Data;
using MobileDeliveryGeneral.Definitions;
using static MobileDeliveryGeneral.Definitions.MsgTypes;
using System.Linq;
using MobileDeliveryGeneral.Interfaces;
using MobileDeliveryMVVM.Models;
using MobileDeliveryGeneral.Interfaces.Interfaces;

namespace MobileDeliveryMVVM.ViewModel
{
    public class OrderVM : BaseViewModel<OrderModelData>
    {
        
        #region SubClasses
        /*
        IList<OrderView> _orders;
        public IList<OrderView> POrders
        {
            get { return _orders; }
            set { _orders = value; }
        }
        public class OrderView 
        {
            public IList<OrderDetailView> OrderDetailsView { get; set; }

            long ManifestId{ get; set; }
            public int ORD_NO { get; set; }
            public int DSP_SEQ { get; set; }
            public string ShpName { get; set; }
            public string ShpAddr { get; set; }
            public string ShpCSZ { get; set; }
            public string DlrAddr { get; set; }
            public string DlrCSZ { get; set; }
            public string DlrTel { get; set; }
            public int OrdNum { get; set; }
            public DateTime ShpDte { get; set; }
            public string Cmt1 { get; set; }
            public string Cmt2 { get; set; }
            public DateTime OrdDte { get; set; }
            public string OrdCnt { get; set; }
            public string DlrName { get; set; }
            public int DlrNo { get; set; }
            public int LineCount { get; set; }
            public DateTime ShipDate { get; set; }
        }
        public class OrderDetailView
        {
            public short WIN_QTY { get; set; }
            public short MDL_NO { get; set; }
            public short DESC { get; set; }
            public short CLR { get; set; }
            public short WIDTH { get; set; }
            public short HEIGHT { get; set; }
        }
      */
        #endregion
  
        #region Properties
        // private ObservableCollection<OrderDetailsVM> orderDetailsVM = new ObservableCollection<OrderDetailsVM>();
        string loadOrderRequestComplete;
        public string LoadOrderRequestComplete {
            get { return loadOrderRequestComplete; } set { SetProperty<string>(ref loadOrderRequestComplete, value); }
        }
        long manifestId;
        public long ManifestId
        {
            get { return manifestId; } set { SetProperty<long>(ref manifestId, value); }
        }
        int ordNo;
        public int ORD_NO
        {
            get { return ordNo; }
            set { SetProperty<int>(ref ordNo, value); }
        }
        int dspseq;
        public int DSP_SEQ
        {
            get { return dspseq; } set { SetProperty<int>(ref dspseq, value); }
        }
        //lblShpName.Text = stop.ShpName.ToString();
        string shpName;
        public string ShpName
        {
            get { return shpName; }
            set { SetProperty<string>(ref shpName, value); }
        }
        //lblShpAddr.Text = stop.ShpAddr.ToString();
        string shpAddr;
        public string ShpAddr
        {
            get { return shpAddr; }
            set { SetProperty<string>(ref shpAddr, value); }
        }
        //lblShpCSZ.Text = stop.ShpCSZ.ToString();
        string shpCSZ;
        public string ShpCSZ
        {
            get { return shpCSZ; }
            set { SetProperty<string>(ref shpCSZ, value); }
        }
        //lblDlrAddr.Text = stop.DealerAddr.ToString();
        string dlrAddr;
        public string DlrAddr
        {
            get { return dlrAddr; }
            set { SetProperty<string>(ref dlrAddr, value); }
        }
        //lblDlrCSZ.Text = stop.DealerCSZ.ToString();
        string dlrCSZ;
        public string DlrCSZ
        {
            get { return dlrCSZ; }
            set { SetProperty<string>(ref dlrCSZ, value); }
        }
        //lblDlrTel.Text = stop.DealerTel.ToString();
        string dlrTel;
        public string DlrTel
        {
            get { return dlrTel; }
            set { SetProperty<string>(ref dlrTel, value); }
        }
        DateTime shpDte;
        public DateTime ShpDte
        {
            get { return shpDte; }
            set { SetProperty<DateTime>(ref shpDte, value); }
        }
        //lblCmt1.Text = stop.DisplaySeq.ToString();
        string cmt1;
        public string Cmt1
        {
            get { return cmt1; }
            set { SetProperty<string>(ref cmt1, value); }
        }
        //lblCmt2.Text = stop.DisplaySeq.ToString();
        string cmt2;
        public string Cmt2
        {
            get { return cmt2; }
            set { SetProperty<string>(ref cmt2, value); }
        }
        //lblOrdDate.Text = 
        DateTime ordDte;
        public DateTime OrdDte
        {
            get { return ordDte; }
            set { SetProperty<DateTime>(ref ordDte, value); }
        }
        // lblLineCnt.Text = stop..ToString();
        int ordCnt;
        public int OrdCnt
        {
            get { return ordCnt; }
            set { SetProperty<int>(ref ordCnt, value); }
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
            get { return dealerNo; }
            set { SetProperty<int>(ref dealerNo, value); }
        }
        int lineCount;
        public int LineCount
        {
            get { return lineCount; }
            set { SetProperty<int>(ref lineCount, value); }
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
        #endregion

        #region LocalVars
        object olock = new object();
        List<long> dDeliveredRequestPending = new List<long>();
        List<long> dShippedRequestPending = new List<long>();
        #endregion

        #region Delegates
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
        //private DelegateCommand completeStopCommand;
        //public DelegateCommand CompleteStopCommand
        //{ get { return completeStopCommand ?? (completeStopCommand = new DelegateCommand(OnCompleteStop)); } }
        #endregion

        #region Observables

        public OrderGroup OrderModelGroup = new OrderGroup();
     //   GetOMG
        // Track the selected Order and display Order Details for selected order 
        ObservableCollection<OrderModelData> orderSelected = new ObservableCollection<OrderModelData>();

        public ObservableCollection<OrderModelData> OrderSelected
        {
            get { return orderSelected; }
            set
            {
                //orderSelected = (OrderModelData)value;
                // Orders. = Orders[0].orders.ItemsCollection;
                OnPropertyChanged("SelectedOrder");
                SetProperty(ref orderSelected, value);
            }
        }

        ObservableCollection<OrderModelData> orderData = new ObservableCollection<OrderModelData>();
        public ObservableCollection<OrderModelData> Orders
        {
            get { return orderData; } set { SetProperty(ref orderData, value); }
        }

        ObservableCollection<Grouping<string, OrderModelData>> orderDataGrouped;

        IList<OrderDetailsModelData> selectedOrderDetails;

        public IList<OrderDetailsModelData> SelectedOrderDetails
        {
            get { return selectedOrderDetails; }
            set { SetProperty(ref selectedOrderDetails, value); }
        }

        ObservableCollection<OrderDetailsModelData> completedOrderData = new ObservableCollection<OrderDetailsModelData>();

        public ObservableCollection<OrderDetailsModelData> CompletedOrders
        {
            get { return completedOrderData; }
            set { SetProperty(ref completedOrderData, value); }
        }
        #endregion

        #region BackgroundWorkers
        //UMBackgroundWorker<OrderModelData> orderThread;
        //UMBackgroundWorker<OrderModelData>.ProgressChanged<OrderModelData> pcOrders;
        #endregion

        //static CacheItem<OrderMasterData> orderdatabase;
        //public static CacheItem<ScanFileData> OrderDatabase
        //{
        //    get
        //    {
        //        if (orderdatabase == null)
        //        {
        //            orderdatabase = new CacheItem<ScanFileData>(SettingsAPI.OrderCachePath);
        //        }
        //        return orderdatabase;
        //    }
        //}
        public OrderVM() : base()
        {
            //Orders.CollectionChanged += (s, e) =>
            //{
            //    Orders = orderData;
            //};
            //CompletedOrders.CollectionChanged += (s, e) =>
            //{
            //    CompletedOrders = completedOrderData;
            //};

            //pcOrders = new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(ProcessMessage);
            //orderThread = new UMBackgroundWorker<IMDMMessage>(new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(pcOrders), rm, sm);

            //var sorted = from order in Orders
            //             orderby order.ORD_NO
            //             group order by order.OrderSort into orderGroup
            //             select new Grouping<int, Orders>(orderGroup.Key, orderGroup);
            //orderDataGrouped = new ObservableCollection<Grouping<string, OrderModelData>>(sorted);
        }
        protected override void Clear(bool bFullRefresh = false)
        {
            loadOrderRequestComplete = "";
            LoadOrdersComplete = false;
            base.Clear(bFullRefresh);

            dShippedRequestPending.Clear();
            dDeliveredRequestPending.Clear();

            if (bFullRefresh)
            {
                LineCount = 0;
                orderData.Clear();
                SetVars();
            }
            
        }
        void InitGroupings() {
        //    var sorted = from order in omg
        //                 orderby order.
        //                 group order by order.ORD_NO into orderGroup
        //                 select new Grouping<string, OrderMasterData>(orderGroup.Key, orderGroup);
        }
        void SetVars(OrderModelData ord=null)
        {
            //ManifestId, DSP_SEQ, , ORD_NO, , ShipDate,  OrdDate,  ShpName,  ShpAddr, 
            //ShpCSZ, ShpTel, DealerName, DlrAddr, DlrCSZ, DlrTel, DlrNo,  Cmt1, Cmt2
            if (ord == null)
            {
                DlrName = "";
                DlrAddr = "";
                ShpAddr = "";
                ShpCSZ = "";
                ORD_NO = 0;
                Cmt1 = "";
                Cmt2 = "";
                DlrCSZ = "";
                DlrNo = 0;
                DlrTel = "";
                OrdCnt = 0;
            }
            else
            { 
                ShpDte = ord.SHP_DTE;
                ShpAddr = ord.SHP_ADDR;
                ShpCSZ = ord.SHP_CSZ;
                DlrName = ord.DLR_NME;
                DlrNo = ord.DLR_NO;
                DlrAddr = ord.DLR_ADDR;
                DlrCSZ = ord.DLR_CSZ;
                DlrTel = ord.DLR_TEL;
                ORD_NO = ord.ORD_NO;
                Cmt1 = ord.CMT1;
                Cmt2 = ord.CMT2;
                OrdDte = ord.ORD_DTE;
                OrdCnt += Orders.Count;
            }
        }

        #region handlers
        public override void InitVM()
        {
            //OrderModelGroup.CollectionChanged += (s, e){ OrderModelGroup = }
            Orders.CollectionChanged += (s, e) =>
            {
                Orders = orderData;
            };
            CompletedOrders.CollectionChanged += (s, e) =>
            {
                CompletedOrders = completedOrderData;
            };

            //pcOrders = new UMBackgroundWorker<OrderModelData>.ProgressChanged<OrderModelData>(ProcessMessage);
            //orderThread = new UMBackgroundWorker<OrderModelData>(new UMBackgroundWorker<OrderModelData>.ProgressChanged<OrderModelData>(pcOrders), rm, sm);
            base.InitVM();
        }
        override public void SendVMMesssage(manifestRequest mreq, Request req)
        {
            InitVM();
            //if (orderThread != null)
            //{
            //    orderThread.OnStartProcess(mreq, req);
            //}
        }

        public void OnCompleteOrder(object arg)
        {
            if (arg != null)
            {
                //Winform Manifest Master release
                OrderDetailsModelData ord = (OrderDetailsModelData)arg;
                if (ord.RequestId == null || ord.RequestId == Guid.Empty)
                    ord.RequestId = NewGuid();
                ord.status = status.Releasing;
                UploadOrderData(ord);
            }
            else
            {
                //var lstSelectedOrders = Orders.Select(a => a).Where(b => b.IsSelected == true && OrderStatus.Delivered.CompareTo(b.Status) != 0).ToList();
                //foreach (var mm in lstSelectedOrders)
                //{
                //    mm.status = status.Releasing;
                //    UploadOrderData(mm);
                //}
            }
        }
        public void OrderSelectionChanged(OrderModelData ordData)
        {
            if (ordData != null)
            {
                ordData.IsVisible = true;
                SelectActiveOrderDetails(ordData);
            }
        }
        public void OnCloseStop(object arg)
        {
            Request req = new Request() { reqGuid = NewGuid() };
            //dRequests.Add(req.reqGuid, req);
            //SendVMMesssage(new manifestRequest() { command = eCommand.CompleteOrder, id = ord.ManifestId, Stop = ord.DSP_SEQ }, req);
            //  orderThread.OnStartProcess(new manifestRequest() { command = eCommand.CompleteOrder, id = ord.ManifestId, Stop = ord.DSP_SEQ }, req);

        }
        public void OnOrdersLoad(object arg)
        {
            //OrderMasterData omd = new OrderMasterData(){ status = status.Init, ManId = ManifestId,  ORD_NO=ORD_NO};
            OrderModelData omd = new OrderModelData() { Command = eCommand.OrdersLoad, ORD_NO = ORD_NO };

            if (arg != EventArgs.Empty)
                omd = (OrderModelData)arg;

            Clear(false);

            Logger.Debug("OnOrdersLoad");
            LoadOrders(omd);
        }
        #endregion

        void ResetOrder(OrderDetailsModelData ord)
        {
            //bool tprevst = ord.prevstate;
            //ord.prevstate = ord.IsSelected;
            //ord.IsSelected = tprevst;
            ord.Command = eCommand.LoadFilesComplete;
            //AddOrder(ord);
        }

        void UploadOrderData(OrderDetailsModelData ord)
        {
            Logger.Info($"OrderVM UploadStopOrder: {ord.ToString()}");
            //bool bExists = Orders.Select(a => a).Where(o => o == ord).ToList().Count>0;
            //bool bCompExists = CompletedOrders.Select(a => a).Where(o => o == ord).ToList().Count > 0;

            //if (((ord.Status == OrderStatus.Shipped && ord.IsSelected==false) && (bExists && !bCompExists) || dDeliveredRequestPending.Contains(ord.ORD_NO)) || (
            //    ord.Status == OrderStatus.Delivered && ord.IsSelected) && (!bExists && bCompExists) || dShippedRequestPending.Contains(ord.ORD_NO))
            //    return;
            
            var req = new Request()
            {
                reqGuid = NewGuid(),
                LIds = new Dictionary<long, status>(),
                LinkMid = new Dictionary<long, List<long>>(),
                ChkIds = new Dictionary<long, status>()
            };
            //dRequests.Add(req.reqGuid, req);
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
            
            //orderM
                //            a(ord);
            completeOrder ordUp = new completeOrder(ord);
            //orderMaster ordUp = new orderMaster(ord, ord.ManId);

            if (bShipped || bDelivered)
            {
                ordUp.command = eCommand.CompleteOrder;

                ProcessMsgDelegateRXRaw pmRx = new ProcessMsgDelegateRXRaw(ProcessMessage);
                Logger.Info($"Upload Stop Order reqid: {req.reqGuid}");

                SendMessage((new manifestRequest()
                {
                    requestId = req.reqGuid.ToByteArray(),
                    command = ordUp.command,
                    bData = ordUp.ToArray()
                }));
                
                lock (olock)
                {
                    if (bShipped)
                        dShippedRequestPending.Add(ord.ORD_NO);
                    else if (bDelivered)
                        dDeliveredRequestPending.Add(ord.ORD_NO);
                }
            }
        }
        //byte[] cmd, Action<byte[], IHandler, Task> cbsend
        public void ProcessMessage(byte[] bcmd, Func<byte[], Task> cbsend = null)
        { }

        
        void LoadOrders(OrderModelData ord)
        {
            if(ord.Command == eCommand.OrdersLoad)
            {
                Request req = new Request() { reqGuid = NewGuid() };
                //dRequests.Add(req.reqGuid, req);
                SendVMMesssage(new manifestRequest() { command = eCommand.OrdersLoad, id = ManifestId, Stop = DSP_SEQ }, req);
                //orderThread.OnStartProcess(new manifestRequest() { command = eCommand.OrdersLoad, id = ManifestId, Stop = DSP_SEQ }, req);
            }
        }
        protected override void Shutdown()
        {
            //orderThread.CleanEvents();
            Clear(true);
            base.Shutdown();
        }

        void ProcessMessage(OrderModelData ordc, Func<byte[], Task> cbsend = null)
        {
            if (ordc.Command == eCommand.OrdersLoadComplete)
            {
                LoadOrderRequestComplete = ordc.RequestId.ToString();
                //orderThread.CompleteBackgroundWorker(ordc.RequestId);
                Logger.Info($"OrdersLoadComplete: {ordc.ToString()}");
                LoadOrdersComplete = true;
                return;
            }
            var ord = (OrderModelData)ordc;
            SetVars(ord);
            ord.status = status.Uploaded;

            Add(ord);
        }

        void Add(OrderModelData odmd)
        {
            lock (olock)
            {
                if (!Orders.Contains(odmd))
                    Orders.Add(odmd);
                else
                {
                    var ord = (OrderModelData)Orders.Select(a => a).Where(b => b.ORD_NO == odmd.ORD_NO).FirstOrDefault();
                    ord.ordDetails.AddRange(odmd.ordDetails);
                }
            }
        }

        void SelectActiveOrderDetails(OrderModelData od)
        {
            //LineCount++;
            //lock (olock)
            {
                List<OrderModelData> lstOrdData = Orders.Select(a => a).
                    Where(b => b.ORD_NO == od.ORD_NO).ToList();
                
                //List<OrderDetailsModelData> lstCompOrdData = CompletedOrders.Select(a => a).
                //    Where(b => b.ORD_NO == od.ORD_NO).ToList();

                if (lstOrdData.Count == 0) // && lstCompOrdData.Count==0)
                    return;
                else if (lstOrdData.Count > 0)
                { 
                    var tmp = lstOrdData[0];
                    Orders.Remove(lstOrdData[0]);
                    foreach( var fndNewOrdDet in tmp.ordDetails.Select(a => a).Where(b => od.ordDetails.Contains(b)==false))
                        od.ordDetails.Add(fndNewOrdDet);
                    Orders.Add(od);
                    base.OnPropertyChanged();
                }

               /* OrderModelData tmpod = null;

                if (lstOrdData.Count > 0)
                    tmpod = lstOrdData[0];
                //else if (lstCompOrdData.Count > 0)
                //    tmpod = lstCompOrdData[0];

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

                if (tmpod != null)// && tmpod.Status == OrderStatus.Delivered)
                {
                    tmpod.status = status.Completed;
                    Add(tmpod);
                    //tmpod.IsSelected = true;
                    //  CompletedOrders.Add(tmpod);

                    //if (tmpod.OnSelectionChanged == null)
                    //    tmpod.OnSelectionChanged = new OrderDetailsModelData.cmdFireOnSelected(UploadOrderData);
                }
                else //if (tmpod != null ) //&& tmpod.Status == OrderStatus.Shipped)
                {
                    tmpod.status = status.Completed;
                   // tmpod.IsSelected = false;
                    //Orders.Add(tmpod);
                    
                    //if (tmpod.OnSelectionChanged == null)
                    //    tmpod.OnSelectionChanged = new OrderDetailsModelData.cmdFireOnSelected(UploadOrderData);
                //}
                //else //add to orders - new item
                //{
                    //if (od.Status == OrderStatus.Shipped)
                    //{
                    //    od.status = status.Completed;
                    //    od.IsSelected = false;
                    //    //Orders.Add(od);
                    //    Add(od);
                    //}
                    //else if (od.Status == OrderStatus.Delivered)
                    //{
                    //    od.status = status.Completed;
                    //    od.IsSelected = true;
                    //    CompletedOrders.Add(od);
                    //}
                    //else
                    //{
                    //    od.status = status.Completed;
                    //   // od.Status = OrderStatus.Shipped;
                    //    od.IsSelected = false;
                    //    //Orders.Add(od);
                    //    Add(od);
                    //}
                    if (dShippedRequestPending.Contains(od.ORD_NO))
                        dShippedRequestPending.Remove(od.ORD_NO);
                    if (dDeliveredRequestPending.Contains(od.ORD_NO))
                        dDeliveredRequestPending.Remove(od.ORD_NO);
                    //if (od.OnSelectionChanged == null)
                    //    od.OnSelectionChanged = new OrderMasterData.cmdFireOnSelected(UploadOrderData);
                } 
                */
                
            }
        }
        //protected override isaCommand ReceiveMessageWinSys(isaCommand cmd)
        //{
        //    switch (cmd.command)
        //    {
        //        case eCommand.OrdersLoad:
        //        case eCommand.OrdersUpload:
        //            Logger.Info($"Winsys TPS {cmd.command} - ReceiveMessageWinSys.");
        //            break;
        //        case eCommand.OrdersLoadComplete:
        //            Logger.Info("Winsys TPS OrdersLoadComplete - ReceiveMessageWinSys.");
        //            break;
        //        default:
        //            return base.ReceiveMessageWinSys(cmd);
        //    }
        //    return cmd;
        //}
        protected override isaCommand ReceiveMessage(isaCommand cmd)
        {
            switch (cmd.command)
            {
                case eCommand.OrdersUpload:
                case eCommand.OrdersLoad:
                case eCommand.OrderModel:
                    //orderThread.ReportProgress(50, new object[] { cmd });
                    break;
                case eCommand.OrdersLoadComplete:
               // case eCommand.OrderModelComplete:
                    LoadOrdersComplete = true;
                    //orderThread.ReportProgress(100, new object[] { cmd });
                    break;
                default:
                    Logger.Debug("OrderVM::ReceiveMessageCB - Unhandled message.");
                    break;
            }
            return cmd;
        }

    }
}
