using DataCaching.Caching;
using DataCaching.Data;
using MobileDeliveryLogger;
using MobileDeliveryMVVM.BaseClasses;
using MobileDeliveryMVVM.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MobileDeliveryGeneral.Data;
using MobileDeliveryGeneral.Definitions;
using MobileDeliveryGeneral.Settings;
using MobileDeliveryGeneral.Threading;
using static MobileDeliveryGeneral.Definitions.MsgTypes;
using MobileDeliveryGeneral.Interfaces.DataInterfaces;
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
        UMBackgroundWorker<IMDMMessage> stopThread;
        UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage> pcStops;
        #endregion

        static CacheItem<Stop> stopdatabase;
        public static CacheItem<Stop> StopDatabase
        {
            get
            {
                if (stopdatabase == null)
                {
                    stopdatabase = new CacheItem<Stop>(Settings.StopCachePath);
                }
                return stopdatabase;
            }
        }

        public StopVM() : base(new SocketSettings()
        {
            url = "localhost",
            port = 81,
            srvurl = "localhost",
            srvport = 81,
            clienturl = "localhost",
            clientport = 8181,
            name = "StopVM"
        }, "StopVM")
        {
            Stops.CollectionChanged += (s, e) =>
            {
                Stops = stopData;
            };

            StopSelectedCommand = new DelegateCommand(OnStopSelected);
            pcStops = new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(ProcessMessage);
            stopThread = new UMBackgroundWorker<IMDMMessage>(new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(pcStops), rm, sm);
        }
        void Clear()
        {
            LoadStopsComplete = false;
            if (stopThread != null)
                dRequests.Keys.ToList().ForEach(a => stopThread.Reset(a));
            STOPCOUNT = 0;
            stopData.Clear();
        }

        Stop GetStop(StopData stopD)
        {
            var stop = new Stop(stopD);

            var info = StopDatabase.GetMapping();

            Stop stp = StopDatabase.GetItem(stop);
            return stp;
        }

        void SaveStop(StopData stopD)
        {
            var stop = new Stop(stopD);
            //truck.Date = DateTime.UtcNow;
            StopDatabase.SaveItem(stop);
            //await Navigation.PopAsync();
        }

        void DeleteStop(StopData stopD)
        {
            var stop = new Stop(stopD);
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
            LoadStops(new Stop() { ManifestId = mid }, bForce);
        }

        private void OnStopSelected(object arg)
        {
            Clear();
            int mid = manifestId;

            if (arg != EventArgs.Empty)
                mid = (int)arg;

            Logger.Debug("OnStopSelected - Load ");
            LoadStops(new Stop() { Id = mid });
        }
        void LoadStops(Stop st, bool bForceLoad = false)
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

                stopThread.OnStartProcess(new manifestRequest() { command = eCommand.Stops, id = st.ManifestId }, reqInfo);
            }
        }
        void ProcessMessage(byte[] cmd, Func<byte[], Task> cbsend = null)
        {
        }

        void ProcessMessage(IMDMMessage std, Func<byte[], Task> cbsend = null)
        {
            if (std.Command == eCommand.StopsLoadComplete)
            {
                LoadStopRequestComplete = std.RequestId.ToString();
                //stopThread.CompleteBackgroundWorker(std.RequestId);
                //LoadStopsComplete = true;
                return;
            }

            AddStop((StopData)std);
        }

        void AddStop(StopData sd)
        {
            //Do we know about this in ther DB?
            var stop = GetStop(sd);
            Stop si;
            if (stop != null)
                si = new Stop(stop);
            else
                si = new Stop(sd);


            if (stop != null)
                stopdatabase.DeleteItem(si);
            //Cache and Add
            stopdatabase.SaveItem(si);

            if (!Stops.Contains(sd))
                Stops.Add(sd);
            else
            {
                Stops.Remove(sd);
                Stops.Add(sd);
            }
        }
        void AddStops(List<Stop> stops)
        {
            foreach (var it in stops)
            {
                AddStop(it.StopData());
            }
        }
        public override isaCommand ReceiveMessageCB(isaCommand cmd)
        {
            switch (cmd.command)
            {
                case eCommand.Stops:
                    stopThread.ReportProgress(50, new object[] { cmd });
                    break;
                case eCommand.StopsLoadComplete:
                    LoadStopsComplete = true;
                    stopThread.ReportProgress(100, new object[] { cmd });
                    break;
            }
            return cmd;
        }
    }
}
