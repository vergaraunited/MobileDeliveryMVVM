using DataCaching.Caching;
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
using MobileDeliverySettings;

namespace MobileDeliveryMVVM.ViewModel
{
    public class OrderOptionsVM : BaseViewModel<OrderOptionsData>
    {
        #region properties

        string loadOrderOptionRequestComplete;
        public string LoadOrderOptionRequestComplete
        {
            get { return loadOrderOptionRequestComplete; }
            set { SetProperty<string>(ref loadOrderOptionRequestComplete, value); }
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

        short optnum;
        public short OPT_NUM
        {
            get { return optnum; }
            set { SetProperty<short>(ref optnum, value); }
        }

        int dealerNo;
        public int DlrNo
        {
            get { return dealerNo; }
            set { SetProperty<int>(ref dealerNo, value); }
        }
        int ordno;
        public int ORD_NO
        {
            get { return ordno; }
            set { SetProperty<int>(ref ordno, value); }
        }
        int lineCount;
        public int LineCount
        {
            get { return lineCount; }
            set { SetProperty<int>(ref lineCount, value); }
        }

        private DelegateCommand _loadOrderOptionsCommand;
        public DelegateCommand LoadOrderOptionsCommand
        { get { return _loadOrderOptionsCommand ?? (_loadOrderOptionsCommand = new DelegateCommand(OnOrderOptionsLoad)); } }

        bool loadOrderOptionsComplete;
        public bool LoadOrderOptionsComplete
        {
            get { return loadOrderOptionsComplete; }
            set { SetProperty<bool>(ref loadOrderOptionsComplete, value); }
        }
        #endregion

        ObservableCollection<OrderOptionsData> orderOptionData = new ObservableCollection<OrderOptionsData>();

        public ObservableCollection<OrderOptionsData> OrderOptions
        {
            get { return orderOptionData; }
            set { SetProperty(ref orderOptionData, value); }
        }
        
        #region BackgroundWorkers
        /*
        UMBackgroundWorker<OrderOptionsData> orderOptionThread;
        UMBackgroundWorker<OrderOptionsData>.ProgressChanged<OrderOptionsData> pcOrderOptions;*/
        #endregion
        
        static CacheItem<OrderOptionsData> orderoptionsdatabase;
        public static CacheItem<OrderOptionsData> OrderOptionDatabase
        {
            get
            {
                if (orderoptionsdatabase == null)
                {
                    orderoptionsdatabase = new CacheItem<OrderOptionsData>(SettingsAPI.OrderOptionCachePath);
                }
                return orderoptionsdatabase;
            }
        }
        public OrderOptionsVM() : base()
        {
            OrderOptions.CollectionChanged += (s, e) =>
            {
                OrderOptions = orderOptionData;
            };

            //pcOrderOptions = new UMBackgroundWorker<OrderOptionsData>.ProgressChanged<OrderOptionsData>(ProcessMessage);
            //orderOptionThread = new UMBackgroundWorker<OrderOptionsData>(new UMBackgroundWorker<OrderOptionsData>.ProgressChanged<OrderOptionsData>(pcOrderOptions), rm, sm);
        }
        protected override void Clear(bool bForce=false)
        {
            LineCount = 0;
            loadOrderOptionRequestComplete = "";
            LoadOrderOptionsComplete = false;
            //if (orderOptionThread != null)
            //    dRequests.Keys.ToList().ForEach(a => orderOptionThread.Reset(a));
            base.Clear(bForce);
            OrderOptions.Clear();
        }
        //class OrdArgs { public int manId { get; set; } public int stopNo { get; set;} public int DSP_SEQ { get; set; } };

        public void OnOrderOptionsLoad(object arg)
        {
            // OrdArgs oa = new OrdArgs() { manId = ManifestId, stopNo = DSP_SEQ };
            OrderOptionsData oa = new OrderOptionsData() { ORD_NO=ORD_NO, OPT_NUM = OPT_NUM };

            if (arg != EventArgs.Empty)
                oa = (OrderOptionsData)arg;

            //ManifestId = oa.EMAILED;
            //DSP_SEQ = oa.DSP_SEQ;
            
            Clear();

            Logger.Debug("OnOrderOptionsLoad");
            LoadOrderOptions(new OrderOptionsData() { ORD_NO= ORD_NO, OPT_NUM = OPT_NUM });
        }
        void LoadOrderOptions(OrderOptionsData ord)
        {
            List<OrderOptionsData> ordOptList = OrderOptionDatabase.GetItems(ord);

            if (false && ordOptList != null && ordOptList.Count > 0)
            {
                //Load From Cache
                AddOrderOptions(ordOptList);
            }
            else
            {
                var req = new Request()
                {
                    reqGuid = NewGuid(),
                    LIds = new Dictionary<long, status>(),
                    LinkMid = new Dictionary<long, List<long>>(),
                    ChkIds = new Dictionary<long, status>()
                };

                SendMessage(new manifestRequest() { command = eCommand.OrderOptions, id = ORD_NO });
                
                //orderDetailThread.OnStartProcess(new manifestRequest() { command = eCommand.OrderDetails, id = ORD_NO }, req);
                //orderOptionThread.OnStartProcess(new manifestRequest() { command = eCommand.OrderOptions, id = ORD_NO}, req);
            }
            //umdSrv.SendMessage(req);
        }

        void ProcessMessage(OrderOptionsData ord, Func<byte[], Task> cbsend = null)
        {
            if (ord.Command == eCommand.OrderDetailsComplete)
            {
                LoadOrderOptionRequestComplete = ord.RequestId.ToString();
                //orderOptionThread.CompleteBackgroundWorker(ord.RequestId);
                LoadOrderOptionsComplete = true;
                return;
            }

            //OrderDetailDatabase.SaveItem(new OrderDetail(ord));
            AddOrderOptions(ord);
        }
        void AddOrderOptions(List<OrderOptionsData> orders)
        {
            //foreach (var ord in orders)
            //{
            //    AddOrderDetails(ord.OrderData());
            //}
        }
        void AddOrderOptions(OrderOptionsData od)
        {
            LineCount++;
            if (!OrderOptions.Contains(od))
                OrderOptions.Add(od);
            else
            {
                OrderOptions.Remove(od);
                OrderOptions.Add(od);
            }
        }
        protected override isaCommand ReceiveMessage(isaCommand cmd)
        {
            switch (cmd.command)
            {
                case eCommand.OrderOptions:
                case eCommand.OrdersLoad:
                    //orderOptionThread.ReportProgress(50, new object[] { cmd });
                    break;
                case eCommand.OrderOptionsComplete:
                    LoadOrderOptionsComplete = true;
                    //orderOptionThread.ReportProgress(100, new object[] { cmd });
                    break;
                default:
                    Logger.Debug("OrderOptionsVM::ReceiveMessageCB - Unhandled message.");
                    break;
            }
            return cmd;
        }

    }
}
