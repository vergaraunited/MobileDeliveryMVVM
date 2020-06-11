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
using MobileDeliveryGeneral.Threading;
using static MobileDeliveryGeneral.Definitions.MsgTypes;
using System.Linq;
using MobileDeliveryGeneral.Definitions;
using MobileDeliveryGeneral.Settings;
using MobileDeliveryGeneral.Interfaces.DataInterfaces;
using MobileDeliverySettings;

namespace MobileDeliveryMVVM.ViewModel
{
    public class TruckVM : BaseViewModel<TruckData>
    {
        int truckcount = 0;     
        public int TRUCKCOUNT { get { return truckcount; } set { SetProperty<int>(ref truckcount, value); } }

        private DelegateCommand _loadCommand;

        public DelegateCommand LoadCommand
        { get { return _loadCommand ?? (_loadCommand = new DelegateCommand(OnTrucksLoad)); } }


        #region fields
        DateTime truckDate;
        bool isSelected = false;
        #endregion
        
        #region properties
        public ICommand TruckSelectedCommand { get; set; }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                SetProperty<bool>(ref isSelected, value);
            }
        }
        string loadTruckRequestComplete;
        public string LoadTruckRequestComplete { get { return loadTruckRequestComplete; } set { SetProperty<string>(ref loadTruckRequestComplete, value); } }

        #endregion

        public DateTime TruckDate
        {
            get { return truckDate; }
            set
            {
                SetProperty<DateTime>(ref truckDate, value);
                OnTrucksLoad(truckDate);
            }
        }
        long loadTruckIdComplete;
        public long LoadTruckIdComplete
        {
            get { return loadTruckIdComplete; }
            set { SetProperty<long>(ref loadTruckIdComplete, value); }
        }

        ObservableCollection<TruckData> truckData = new ObservableCollection<TruckData>();
        public ObservableCollection<TruckData> Trucks
        {
            get { return truckData; }
            set
            {
                SetProperty(ref truckData, value);
            }
        }
        #region BackgroundWorkers
        UMBackgroundWorker<IMDMMessage> truckThread;
        UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage> pcTrucks;
        #endregion

        static CacheItem<Truck> truckdatabase;

        public TruckVM() : base(new SocketSettings()
        {
            url = "localhost",
            port = 81,
            srvurl = "localhost",
            srvport = 81,
            clienturl = "localhost",
            clientport = 8181,
            name = "TruckVM"
        }, "TruckVM")
        {
            pcTrucks = new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(ProcessMessage);
            truckThread = new UMBackgroundWorker<IMDMMessage>(new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(pcTrucks), rm, sm);
                
            Trucks.CollectionChanged += (s, e) =>
            {
                Trucks = truckData;
            };

            TruckSelectedCommand = new DelegateCommand(OnTruckSelected);

            this.TruckDate = DateTime.Now;
        }

        public static CacheItem<Truck> TruckDatabase
        {
            get
            {
                if (truckdatabase == null)
                {
                    truckdatabase = new CacheItem<Truck>(Settings.TruckCachePath);
                }
                return truckdatabase;
            }
        }
        public void ReInitialize(object arg)
        {
            if (truckThread != null)
            {
                TruckDatabase.BackupAndClearAll();
            }

        }
        void Clear()
        {
            LoadTruckRequestComplete = "";
            LoadTruckIdComplete = 0;
            if (truckThread != null)
                dRequests.Keys.ToList().ForEach(a => truckThread.Reset(a));

            TRUCKCOUNT = 0;
            truckData.Clear();
        }

        public void OnTrucksLoad(object arg)
        {
            bool bForce = false;
            if (arg == null)
                bForce = true;

            Clear();

            DateTime dte = TruckDate;

            if (arg != EventArgs.Empty && arg != null)
                dte = (DateTime)arg;

            Logger.Info("OnTrucksLoad");
            LoadTrucks(new Truck() { ShipDate = dte.ToString("yyyy-MM-dd") }, bForce);
        }

        private void OnTruckSelected(object arg)
        {
            Clear();
            DateTime dte = TruckDate;

            if (arg != null)
                dte = (DateTime)arg;

            Logger.Debug("OnTruckSelected - Load ");
            LoadTrucks(new Truck() { ShipDate = dte.ToString("yyyy-MM-dd") });
                //new manifestRequest() { command = eCommand.Trucks, date = dte.Date.ToString("yyyy-MM-dd") });
        }

        void LoadTrucks(Truck trk, bool bForceLoad = false)
        {
            if (trk.Command == eCommand.GenerateManifest)
            {
                if (truckThread != null)
                {
                    truckThread.OnStartProcess(new manifestRequest() { command = eCommand.Trucks, date = trk.ShipDate }, new Request()
                    {
                        reqGuid = NewGuid(),
                        LIds = new Dictionary<long, status>(),
                        LinkMid = new Dictionary<long, List<long>>()
                    });
                }
            }
            else
            {
                List<Truck> trkList = TruckDatabase.GetItems(trk);
                
                if ((trkList != null && trkList.Count> 0 ) || bForceLoad)
                {
                    //Load From Cache
                    AddTrucks(trkList);
                }
            }
        }

        public Truck OnGet(TruckData truckD)
        {
            var truck = new Truck(truckD);
            var info = TruckDatabase.GetMapping();

            Truck trk = TruckDatabase.GetItem(truck);
            return trk;
        }

        void OnSaveTruckClicked(TruckData truckD)
        {
            var truck = new Truck(truckD);
            //truck.Date = DateTime.UtcNow;
            TruckDatabase.SaveItem(truck);
            //await Navigation.PopAsync();
        }

        void OnDeleteTruckClicked(TruckData truckD)
        {
            var truck = new Truck(truckD);
            TruckDatabase.DeleteItem(truck);
           // await Navigation.PopAsync();
        }

        void ProcessMessage(IMDMMessage trk, Func<byte[], Task> cbsend = null)
        {
            if (trk.Command == eCommand.TrucksLoadComplete)
            {
                LoadTruckRequestComplete = trk.RequestId.ToString();
                truckThread.CompleteBackgroundWorker(trk.RequestId);
                return;
            }
                

            Truck newtruck = new Truck((TruckData)trk);
            Truck truck = TruckDatabase.GetItem(newtruck);
            if (truck == null)
                TruckDatabase.SaveItem(newtruck);
            AddTruck((TruckData)trk);
        }
        void AddTruck(TruckData td)
        {
            if (!Trucks.Contains(td))
                Trucks.Add(td);
            else
            {
                var tcnt = truckData.Where(m => m.ManifestId == td.ManifestId).FirstOrDefault();
                Trucks.Remove(tcnt);
                Trucks.Add(td);
            }
        }
        void AddTrucks(List<Truck> trucks)
        {
            foreach (var it in trucks)
            {
                AddTruck(it.TruckData());
            }
        }
        public override isaCommand ReceiveMessageCB(isaCommand cmd)
        {
            switch (cmd.command)
            {
                case eCommand.Trucks:
                    truckThread.ReportProgress(50, new object[] { cmd });
                    break;
                case eCommand.TrucksLoadComplete:
                    truckThread.ReportProgress(100, new object[] { cmd });
                    break;
                default:
                    break;

            }
            return cmd;
        }
    }
}
