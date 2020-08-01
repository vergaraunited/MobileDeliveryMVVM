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
    public class StopVM : BaseViewModel<StopData>
    {

        public ICommand StopSelectedCommand { get; set; }

        ObservableCollection<StopData> stopData = new ObservableCollection<StopData>();

        #region properties
        int manifestId;
        public int ManifestId
        {
            get { return manifestId; }
            set
            {
                //if (
                SetProperty<int>(ref manifestId, value);
                //)
                  //  if (manifestId > 0)
                    //    OnStopsLoad(manifestId);
            }
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

        int stopCount;
        public int STOPCOUNT
        {
            get { return stopCount; } set { SetProperty<int>(ref stopCount, value); }
        }

        private DelegateCommand _loadCommand;
        public DelegateCommand LoadCommand
        { get { return _loadCommand ?? (_loadCommand = new DelegateCommand(OnStopsLoad)); } }

        string loadStopRequestComplete;
        public string LoadStopRequestComplete { get { return loadStopRequestComplete; }
            set { SetProperty<string>(ref loadStopRequestComplete, value); } }

        bool loadStopsComplete;
        public bool LoadStopsComplete
        {
            get { return loadStopsComplete; }
            set { SetProperty<bool>(ref loadStopsComplete, value); }
        }

        public OrderModelData OMD { get; set; }

        #endregion

        public ObservableCollection<StopData> Stops
        {
            get { return stopData; }
            set
            {
                SetProperty(ref stopData, value);
            }
        }

        #region BackgroundWorkers
        //Moved Background Worker (used for communication to the server) to the base class (one socket set instance).
        //UMBackgroundWorker<IMDMMessage> stopThread;
        //UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage> pcStops;
        #endregion

        static CacheItem<StopData> stopdatabase;
        public static CacheItem<StopData> StopDatabase
        {
            get
            {
                if (stopdatabase == null)
                {
                    stopdatabase = new CacheItem<StopData>(SettingsAPI.StopCachePath);
                }
                return stopdatabase;
            }
        }

        public StopVM() : base()
        {
            Stops.CollectionChanged += (s, e) =>
            {
                Stops = stopData;
            };

            StopSelectedCommand = new DelegateCommand(OnStopSelected);
            //pcStops = new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(ProcessMessage);
            //stopThread = new UMBackgroundWorker<IMDMMessage>(new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(pcStops), rm, sm);
        }
        protected override void Clear(bool bForce=false)
        {
            LoadStopsComplete = false;
            //if (stopThread != null)
             //   dRequests.Keys.ToList().ForEach(a => stopThread.Reset(a));
            STOPCOUNT = 0;
            stopData.Clear();
        }

        StopData GetStop(StopData stop)
        {
            var info = StopDatabase.GetMapping();
            StopData stp = StopDatabase.GetItem(stop);
            return stp;
        }

        void SaveStop(StopData stop)
        {
            //truck.Date = DateTime.UtcNow;
            StopDatabase.SaveItem(stop);
            //await Navigation.PopAsync();
        }

        void DeleteStop(StopData stop)
        {
            StopDatabase.DeleteItem(stop);
            // await Navigation.PopAsync();
        }
        public void OnStopsLoad(object arg)
        {
            bool bForce = false;

            if (arg == null)
                bForce = true;

            Clear();

            int mid = manifestId;

            if (arg!=null && arg != EventArgs.Empty)
                mid = (int)arg;

            Logger.Debug("OnStopsLoad");
            LoadStops(new StopData() { ManifestId = mid }, bForce);
        }

        private void OnStopSelected(object arg)
        {
            Clear();
            int mid = manifestId;

            if (arg != EventArgs.Empty)
                mid = (int)arg;

            Logger.Debug("OnStopSelected - Load ");
            LoadStops(new StopData() { Id = mid });
        }
        void LoadStops(StopData st, bool bForceLoad = false)
        {
            //List<Stop> stpList = StopDatabase.GetItems(st);

            //if (stpList != null && stpList.Count > 0 && !bForceLoad)
            //{
            //    //Load From Cache
            //    AddStops(stpList);
            //}
            //else
            {
                //Fetch from the server
                Request reqInfo = new Request()
                {
                    reqGuid = NewGuid(),
                    LIds = new Dictionary<long, status>(),
                    LinkMid = new Dictionary<long, List<long>>()
                };
                SendMessage(new manifestRequest() { command = eCommand.Stops, id = st.ManifestId });
                //stopThread.OnStartProcess(new manifestRequest() { command = eCommand.Stops, id = st.ManifestId }, reqInfo);
            }
        }
        void ProcessMessage(byte[] cmd, Func<byte[], Task> cbsend = null)
        {
        }
        protected override isaCommand ReceiveMessage(isaCommand cmd)
        {
            switch (cmd.command)
            {
                case eCommand.Stops:
                    Logger.Info("StopVM eCommand.Stops - ReceiveMessage.");
                    break;
                case eCommand.CompleteStop:
                    Logger.Info("StopVM eCommand.CompleteStop - ReceiveMessage.");
                    break;
                default:
                    Logger.Info("StopVM Unknown command - ReceiveMessage.");
                    break;
                    //return base.ReceiveMessage(cmd);
            }
            return cmd;
        }

        //        protected override void ProcessMessageUMD(IMDMMessage std, Func<byte[], Task> cbsend = null)
        //        {
        //            if (std.Command == eCommand.StopsLoadComplete)
        //            {
        //                LoadStopRequestComplete = std.RequestId.ToString();
        //                //stopThread.CompleteBackgroundWorker(std.RequestId);
        //                LoadStopsComplete = true;
        //                return;
        //            }

        //            AddStop((StopData)std);
        //        }

        //        protected override void ProcessMessageWinsys(IMDMMessage std, Func<byte[], Task> cbsend = null)
        //        {
        //            if (std.Command == eCommand.StopsLoadComplete)
        //            {
        //  //              LoadStopRequestComplete = std.RequestId.ToString();
        //                //stopThread.CompleteBackgroundWorker(std.RequestId);
        //    //            LoadStopsComplete = true;
        //                return;
        //            }

        ////            AddStop((StopData)std);
        //        }

        void AddStop(StopData sd)
        {
            //Do we know about this in ther DB?
            var stop = GetStop(sd);

            StopData si = stop==null ? sd : stop;

            if (stop != null)
                stopdatabase.DeleteItem(si);
            //Cache and Add
            //stopdatabase.SaveItem(si);

            if (!Stops.Contains(sd))
                Stops.Add(sd);
            else
            {
                Stops.Remove(sd);
                Stops.Add(sd);
            }
        }
        void AddStops(List<StopData> stops)
        {
            foreach (var stop in stops)
            {
                AddStop(stop);
            }
        }
        //public override isaCommand ReceiveMessage(isaCommand cmd)
        //{
        //    switch (cmd.command)
        //    {
        //        case eCommand.Stops:
        //            //stopThread.ReportProgress(50, new object[] { cmd });
        //            break;
        //        case eCommand.StopsLoadComplete:
        //            LoadStopsComplete = true;
        //            //stopThread.ReportProgress(100, new object[] { cmd });
        //            break;
        //    }
        //    return cmd;
        //}

        protected override void Shutdown()
        {
            Clear();
            //stopThread.CleanEvents();
            LoadStopsComplete = true; ;
            base.Shutdown();
        }
    }
}
