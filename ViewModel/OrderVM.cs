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
using UMDGeneral.Data;
using UMDGeneral.Definitions;
using UMDGeneral.Threading;
using static UMDGeneral.Definitions.MsgTypes;
using System.Linq;
using UMDGeneral.Settings;

namespace MobileDeliveryMVVM.ViewModel
{
    public class OrderVM : BaseViewModel<OrderData>
    {
        #region properties

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
        #region BackgroundWorkers
        UMBackgroundWorker<OrderData> orderThread;
        UMBackgroundWorker<OrderData>.ProgressChanged<OrderData> pcOrders;
        #endregion
        static CacheItem<Order> orderdatabase;
        public static CacheItem<Order> OrderDatabase
        {
            get
            {
                if (orderdatabase == null)
                {
                    orderdatabase = new CacheItem<Order>(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UMDDB_Orders.db3"));
                }
                return orderdatabase;
            }
        }
        public OrderVM() : base(new UMDAppConfig() { AppName = "OrderVM"} )
        {
            Orders.CollectionChanged += (s, e) =>
            {
                Orders = orderData;
            };

            pcOrders = new UMBackgroundWorker<OrderData>.ProgressChanged<OrderData>(ProcessMessage);
            orderThread = new UMBackgroundWorker<OrderData>(new UMBackgroundWorker<OrderData>.ProgressChanged<OrderData>(pcOrders), rm, sm);
        }
        void Clear()
        {
            LineCount = 0;
            loadOrderRequestComplete = "";
            LoadOrdersComplete = false;
            if (orderThread != null)
                dRequests.Keys.ToList().ForEach(a => orderThread.Reset(a));
            
            Orders.Clear();
        }
        //class OrdArgs { public int manId { get; set; } public int stopNo { get; set;} public int DSP_SEQ { get; set; } };

        public void OnOrdersLoad(object arg)
        {
            // OrdArgs oa = new OrdArgs() { manId = ManifestId, stopNo = DSP_SEQ };
            Order oa = new Order(){ ManifestId = ManifestId, DSP_SEQ = DSP_SEQ };

            if (arg != EventArgs.Empty)
                oa = (Order)arg;

            ManifestId = oa.ManifestId;
            DSP_SEQ = oa.DSP_SEQ;

            Clear();
            
            Logger.Debug("OnOrdersLoad");
            LoadOrders(new Order() { DSP_SEQ = oa.DSP_SEQ, ManifestId = oa.ManifestId } );
        }
        void LoadOrders(Order ord)
        {
            List<Order> ordList = OrderDatabase.GetItems(ord);

            if (ordList != null && ordList.Count > 0)
            {
                //Load From Cache
                AddOrders(ordList);
            }
            else
            {
                Request req = new Request() { reqGuid = Guid.NewGuid() };
                dRequests.Add(req.reqGuid, req);
                orderThread.OnStartProcess(new manifestRequest() { command = eCommand.OrdersLoad, id = ord.ManifestId, Stop= ord.DSP_SEQ }, req);
            }
            //umdSrv.SendMessage(req);
        }

        void ProcessMessage(OrderData ord, Func<byte[], Task> cbsend = null)
        {
            if (ord.Command == eCommand.OrdersLoadComplete)
            {
                LoadOrderRequestComplete = ord.RequestId.ToString();
                orderThread.CompleteBackgroundWorker(ord.RequestId);
                LoadOrdersComplete = true;
                return;
            }

            OrderDatabase.SaveItem(new Order(ord));
            AddOrder(ord);
        }
        void AddOrders(List<Order> orders)
        {
            foreach( var ord in orders)
            {
                AddOrder(ord.OrderData());
            }
        }
        void AddOrder(OrderData od)
        {
            LineCount++;
            //Order ord = new Order(od);
            if (!Orders.Contains(od))
            {
                Orders.Add(od);
            }
            else
            {
                Orders.Add(od);
                Orders.Remove(od);
                Orders.Add(od);
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
