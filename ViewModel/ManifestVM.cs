using MobileDeliveryMVVM.BaseClasses;
using MobileDeliveryMVVM.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using MobileDeliveryGeneral.Data;
using static MobileDeliveryGeneral.Definitions.MsgTypes;
using System.Threading.Tasks;
using MobileDeliveryGeneral.Threading;
using MobileDeliveryGeneral.Interfaces.DataInterfaces;
using System.Collections.Generic;
using MobileDeliveryGeneral.Definitions;
using MobileDeliveryGeneral.Interfaces;
using MobileDeliveryLogger;
using System.ComponentModel;
using MobileDeliveryGeneral.Settings;

namespace MobileDeliveryMVVM.ViewModel
{
    public class ManifestVM : BaseViewModel<IMDMMessage>
    {
        #region fields
        ObservableCollection<string> loadManifestIdComplete = new ObservableCollection<string>();
        ObservableCollection<string> uploadManifestIdComplete = new ObservableCollection<string>();
        bool loadManifestCompleted;

        DateTime manifestDate;
        const int maxStackCount = 30;
        int routecount = 0;
        int totorders = 0;
        int ocnt = 0;
        int odcnt = 0;
        int oocnt = 0;
        int manId = 0;
        bool binitOD;
        bool binitOO;
        bool binitO;
        bool isSelected = false;
        #endregion

        #region properties
        public ICommand LoadFilesCommand { get; set; }
        public ICommand LoadManifestCommand { get; set; }
        public ICommand RouteSelectedCommand { get; set; }
        public ICommand ReleaseManifestCommand { get; set; }

        List<long> lstMissingIds = new List<long>();

        public ObservableCollection<string> LoadManifestIdComplete
        {
            get { return loadManifestIdComplete; }
            set { SetProperty<ObservableCollection<string>>(ref loadManifestIdComplete, value);}
        }

        public ObservableCollection<string> UploadManifestIdComplete
        {
            get { return uploadManifestIdComplete; }
            set { SetProperty<ObservableCollection<string>>(ref uploadManifestIdComplete, value); }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                SetProperty<bool>(ref isSelected, value);
            }
        }
        public DateTime ManifestDate {
            get { return manifestDate; }
            set {
                SetProperty<DateTime>(ref manifestDate, value);
                OnManifestManifestLoad(manifestDate);
            }
        }
        string loadManifestRequestComplete;
        public string LoadManifestRequestComplete { get { return loadManifestRequestComplete; } set { SetProperty<string>(ref loadManifestRequestComplete, value); } }
        public bool LoadManifestCompleted { get { return loadManifestCompleted; } set { SetProperty<bool>(ref loadManifestCompleted, value); } }
        public int TOTORDERS { get { return totorders; } set { SetProperty<int>(ref totorders, value); } }
        public int ROUTECOUNT { get { return routecount; } set { SetProperty<int>(ref routecount, value); }  }
        public int ManId { get { return manId; } set { SetProperty<int>(ref manId, value); } }
        #endregion

        #region observables

        BindingList<ManifestMasterData> manifestMasterData = new BindingList<ManifestMasterData>();


        public BindingList<ManifestMasterData> ManifestMaster
        {
            get { return manifestMasterData; }
            set
            {
                SetProperty<BindingList<ManifestMasterData>>(ref manifestMasterData, value);
            }
        }
        #endregion

        #region BackgroundWorkers
        UMBackgroundWorker<IMDMMessage> manifestMasterThread;
        UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage> pcManifestMaster;
        #endregion

        object olock =new Object();

        public ManifestVM(UMDAppConfig config) : base(config.srvSet, config.AppName)
        {
            Init();
        }

        public ManifestVM() : base(new SocketSettings() {url="localhost", port=81, srvurl="localhost", srvport=81, clienturl="localhost", clientport=8181,
            name ="ManifestVM" }, "ManifestVM")
        {
            Init();
        }

        void Init()
        {
            pcManifestMaster = new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(ProcessMessage);
            manifestMasterThread = new UMBackgroundWorker<IMDMMessage>(new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(pcManifestMaster), rm, sm);

            LoadManifestCommand = new DelegateCommand(OnManifestManifestLoad);
            LoadFilesCommand = new DelegateCommand(OnLoadFilesLoad);
            RouteSelectedCommand = new DelegateCommand(OnRouteSelected);
            ReleaseManifestCommand = new DelegateCommand(OnReleaseManifestSelected);

            this.manifestDate = DateTime.Now;
            Clear();
        }

        public void ReleaseManifest(ManifestMasterData mmd) {
           OnReleaseManifestSelected(mmd);
        }

        void UploadManifestMasterData(ManifestMasterData mm)
        {
            if (mm.IsSelected)
            {
                Logger.Info($"ManifestVM UploadManifestMasterData: {mm.ToString()}");

                Clear();

                var req = new Request()
                {
                    reqGuid = mm.RequestId,
                    LIds = new Dictionary<long, status>(),
                    LinkMid = new Dictionary<long, List<long>>(),
                    ChkIds = new Dictionary<long, status>()
                };
                dRequests.Add(req.reqGuid, req);

                manifestMaster manUp = new manifestMaster(mm);
                manUp.command = eCommand.UploadManifest;

                ProcessMsgDelegateRXRaw pmRx = new ProcessMsgDelegateRXRaw(ProcessMessage);
                Logger.Info($"Upload Manifest reqie: {req.reqGuid}");
                manifestMasterThread.OnStartProcess((new manifestRequest()
                {
                    requestId = req.reqGuid.ToByteArray(),
                    command = manUp.command,
                    bData = manUp.ToArray(),
                    id = manUp.id
                }), req, pmRx);
            }
        }

        bool CheckManifestStatus(ManifestMasterData md)
        {
            bool bSuccess;
            try
            {
                Request valReq;
                if (dRequests.TryGetValue(md.RequestId, out valReq))
                {
                    Logger.Info($"ManifestVM::Update Init to Releasing for link: {md.LINK}");
                    valReq.LIds[md.LINK] = status.Pending;
                    valReq.ChkIds.Add(md.LINK, status.Init);
                }
                else
                    throw new Exception($"Why are we checking for an unknown request? {md.RequestId}");
                md.Command = eCommand.CheckManifest;
                md.status = status.Pending;
                Add(md);
                Logger.Info($"ManifestVM::CheckManifestStatus: {md.ToString()}");
                bSuccess = umdSrv.SendMessage(new manifestMaster(md));
            }
            catch (Exception ex) {
                bSuccess = false;
                Logger.Error($"Error Checking for Manifest Status {ex.Message}.");
            }
            return bSuccess;
        }
        private void OnReleaseManifestSelected(object arg)
        {
            if (arg != null)
            {
                //Winform Manifest Master release
                ManifestMasterData mm = (ManifestMasterData)arg;
                if (mm.RequestId == null || mm.RequestId == Guid.Empty)
                    mm.RequestId = NewGuid();
                
                mm.status = status.Releasing;
                Logger.Info($"OnReleaseManifestSlected {mm.ToString()}");
                Add(mm);
                UploadManifestMasterData(mm);
            }
            else
            {
                foreach (var mm in manifestMasterData)
                {
                    mm.status = status.Releasing;
                    Logger.Info($"OnReleaseManifestSlected {mm.ToString()}");
                    Add(mm);
                    UploadManifestMasterData(mm);
                }
            }
        }

        public void LoadData(manifestRequest req)
        {
            Logger.Info($"ManifestVM::LoadData: {req.ToString()}");
            winSys.SendMessage(req);
        }

        public override isaCommand ReceiveMessageCB(isaCommand cmd)
        {
            switch (cmd.command)
            {
                case eCommand.Manifest:
                case eCommand.CheckManifest:
                case eCommand.OrderOptions:
                case eCommand.OrderDetails:
                case eCommand.OrdersLoad:
                case eCommand.ScanFile:
                    manifestMasterThread.ReportProgress(50, new object[] { cmd });
                    break;
                case eCommand.ManifestLoadComplete:
                case eCommand.CheckManifestComplete:
                case eCommand.UploadManifestComplete:
                case eCommand.ManifestDetailsComplete:
                case eCommand.OrdersLoadComplete:
                case eCommand.OrderDetailsComplete:
                case eCommand.OrderOptionsComplete:
                case eCommand.LoadFilesComplete:
                    manifestMasterThread.ReportProgress(100, new object[] { cmd });
                    break;
                default:
                    break;
            
            }
            return cmd;
        }
        void Clear()
        {
            loadManifestRequestComplete = "";
            LoadManifestIdComplete.Clear();
            if (manifestMasterThread != null)
                dRequests.Keys.ToList().ForEach(a => manifestMasterThread.Reset(a));
            dRequests.Clear();
            ROUTECOUNT = 0;
            manifestMasterData.Clear();
            oocnt = odcnt = ocnt = 0;
            binitO = binitOO = binitOD = false;
        }
        void Clear(Guid key)
        {
            loadManifestRequestComplete = "";
            LoadManifestIdComplete.Clear();
            if (manifestMasterThread != null)
                manifestMasterThread.Reset(key);
            ROUTECOUNT = 0;
            manifestMasterData.Clear();
        }

        public void OnLoadFilesLoad(object arg)
        {
            LoadManifestCompleted = false;
            Clear();

            var req = new Request()
            {
                reqGuid = NewGuid(),
                LIds = new Dictionary<long, status>(),
                LinkMid = new Dictionary<long, List<long>>(),
                ChkIds = new Dictionary<long, status>()
            };
            dRequests.Add(req.reqGuid, req);

            ProcessMsgDelegateRXRaw pmRx = new ProcessMsgDelegateRXRaw(ProcessMessage);

            manifestMasterThread.OnStartProcess((new manifestRequest()
            {
                command = eCommand.LoadFiles
                }), req, pmRx);
        }
        public void OnManifestManifestLoad(object arg)
        {
            LoadManifestCompleted = false;
            Clear();

            var req = new Request() {
                reqGuid = NewGuid(),
                LIds = new Dictionary<long, status>(),
                LinkMid = new Dictionary<long, List<long>>(),
                ChkIds = new Dictionary<long,status>()
            };
            dRequests.Add(req.reqGuid, req);

            DateTime dt;
            if (arg == null)
                dt = DateTime.Now.Date;
            else
                dt = ((DateTime)arg).Date;

            ProcessMsgDelegateRXRaw pmRx = new ProcessMsgDelegateRXRaw(ProcessMessage);


            manifestMasterThread.OnStartProcess((new manifestRequest()
            {
                command = eCommand.GenerateManifest,
                date = dt.ToString("yyyy-MM-dd"),
                requestId = req.reqGuid.ToByteArray()
            }), req, pmRx);
        }
        public void ProcessMessage(byte[] bcmd, Func<byte[], Task> cbsend = null)
        {}

        public void ProcessMessage(IMDMMessage icmd, Func<byte[], Task> cbsend=null)
        {
            ManifestMasterData mmcmd;
            ManifestDetailsData mdcmd;
            bool bTerminateThread = false;
            Request valReq;
            int cnt=0;
            IMDMMessage mcmd;

            if (icmd.Command == eCommand.Manifest)
            {
                mmcmd = (ManifestMasterData)icmd;
                mmcmd.status = status.Init;
                Add(mmcmd);

                //Winsys loading of the Manifest Master update to the original requestId for a ship date
                Logger.Info($"ManifestVM::Process Message from Winsys: ManifestMaster Query: {mmcmd.ToString()}");

                if (dRequests.TryGetValue(mmcmd.RequestId, out valReq))
                {
                    if (!valReq.LIds.ContainsKey(mmcmd.LINK))
                    {
                        Logger.Info($"ManifestVM::Add Init Query link {mmcmd.LINK}");
                        lock (olock)
                        {
                            valReq.LIds.Add(mmcmd.LINK, status.Init);
                            LoadManifestIdComplete.Add(mmcmd.LINK.ToString());
                        }
                    }
                    else
                    {
                        Logger.Info($"ManifestVM::Update Init to Releasing for link: {mmcmd.LINK}");
                        valReq.LIds[mmcmd.LINK] = status.Releasing;
                    }
                }
                else
                    Logger.Error($"Request not found, why are we processing an unkniown message? {mmcmd.RequestId}");

                //Search the Manifest from the SQL Server to see if it was already released.
                CheckManifestStatus(mmcmd);
            }
            else if (icmd.Command == eCommand.CheckManifest)
            {
                mmcmd = (ManifestMasterData)icmd;
                mmcmd.status = status.Init;
                Add(mmcmd);
                Logger.Info($"ManifestVM::Process Message: Check Manifest Result: {mmcmd.ToString()}");

                if (dRequests.TryGetValue(mmcmd.RequestId, out valReq))
                {

                    if (mmcmd.ManifestId != 0)
                    {
                        mmcmd.Command = eCommand.ManifestLoadComplete;
                        mmcmd.status = status.Released;
                        valReq.LIds[mmcmd.LINK] = status.Completed;
                        if (valReq.LinkMid.ContainsKey(mmcmd.LINK))
                            lock (olock)
                                valReq.LinkMid[mmcmd.LINK].Add(mmcmd.ManifestId);
                        else
                            lock (olock)
                                valReq.LinkMid.Add(mmcmd.LINK, new List<long>() { mmcmd.ManifestId });
                    }
                    else
                    {
                        mmcmd.Command = eCommand.Manifest;
                        mmcmd.status = status.Uploaded;
                        if (valReq.LIds.ContainsKey(mmcmd.LINK))
                            valReq.LIds[mmcmd.LINK] = status.Completed;
                        else
                            valReq.LIds.Add(mmcmd.LINK, status.Completed);
                    }
                }
                Add(mmcmd);
            }
            else if (icmd.Command == eCommand.CheckManifestComplete)
            {
                mmcmd = (ManifestMasterData)icmd;
                // if the man id exists update otherwise we are done here
                mmcmd.Command = eCommand.ManifestLoadComplete;

                if (LoadManifestIdComplete.Contains(mmcmd.LINK.ToString()))
                    lock (olock)
                        LoadManifestIdComplete.Remove(mmcmd.LINK.ToString());

                int mcnt = 0;

                if (dRequests.TryGetValue(mmcmd.RequestId, out valReq))
                {
                    if (valReq.LinkMid.ContainsKey(mmcmd.LINK))
                        mmcmd.status = status.Released;
                    else
                        mmcmd.status = status.Completed;

                    Add(mmcmd);

                    valReq.ChkIds.Remove(mmcmd.LINK);
                    if ((valReq.LIds.Select(a => a).Where(b => b.Value.CompareTo(status.Init) == 0).Count() == 0 ||
                        valReq.LIds.Count == 0) && valReq.ChkIds.Count == 0)
                        bTerminateThread = true;

                    cnt = valReq.LIds.Select(a => a).Where(b => b.Value.CompareTo(status.Init) == 0).Count();
                    Logger.Info($"ManifestVM::Process Manifest {cnt} PENDING remaining.");
                }

                if (cnt == 0 && (valReq != null && valReq.ChkIds.Count == 0) && mcnt == 0)
                {
                    Logger.Info($"ManifestVM::Process Manifest Load Complete May now Terminate Thread.");
                    bTerminateThread = true;
                }
            }
            else if (icmd.Command == eCommand.ManifestLoadComplete)
            {
                mmcmd = (ManifestMasterData)icmd;
                Logger.Info($"ManifestVM::Process Manifest Load Complete Message: {mmcmd.ToString()}");

                if (dRequests.TryGetValue(mmcmd.RequestId, out valReq))
                {
                    cnt = valReq.LIds.Select(a => a).Where(b => b.Value.CompareTo(status.Init) == 0).Count() + valReq.ChkIds.Count();

                    if (mmcmd.ManifestId == 0 && mmcmd.LINK != 0)
                    {
                        var it = valReq.LIds.Select(b => b).Where(a => a.Key.CompareTo((Int32)mmcmd.LINK) == 0).FirstOrDefault();

                        status est = ((KeyValuePair<long, status>)it).Value;
                        if (est == status.Init)
                        {
                            Logger.Info($"ManifestVM::Process Manifest decrementing from {cnt} Inits. Flipping {mmcmd.LINK} Request to PENDING.");
                            valReq.LIds[mmcmd.LINK] = status.Pending;
                        }
                        else
                            valReq.LIds[mmcmd.LINK] = status.Completed;

                        cnt = valReq.LIds.Select(a => a).Where(b => b.Value.CompareTo(status.Init) == 0).Count() + valReq.ChkIds.Count();
                        Logger.Info($"ManifestVM::Process Manifest decrementing New Count {cnt} Inits. Flipping {mmcmd.LINK} Request to PENDING.");
                    }
                    else if (mmcmd.LINK == 0)
                    {
                        // No trucks found in winsys
                        Logger.Info($"No truck manifest found for this date {mmcmd.SHIP_DTE}!");
                    }
                    else
                    {
                        Logger.Info($"ManifestVM::Process Manifest Load cnt: {cnt} Complete Terminate Thread.");

                        if (valReq.LIds.ContainsKey(mmcmd.LINK))
                            lock (olock)
                                valReq.LIds.Remove(mmcmd.LINK);

                        mmcmd.Command = eCommand.ManifestLoadComplete;

                        lock (olock)
                            LoadManifestIdComplete.Add(mmcmd.ManifestId.ToString());
                    }
                    cnt = valReq.LIds.Select(a => a).Where(b => b.Value.CompareTo(status.Init) == 0).Count();
                    Logger.Info($"ManifestVM::Process Manifest {cnt} PENDING remaining.");

                    if (cnt == 0 && valReq.ChkIds.Count == 0)
                    {
                        Logger.Info($"ManifestVM::Process Manifest Load Complete May now Terminate Thread.");
                        bTerminateThread = true;
                    }
                }
            }
            else if (icmd.Command == eCommand.UploadManifestComplete)
            {
                mmcmd = (ManifestMasterData)icmd;
                lock (olock)
                    uploadManifestIdComplete.Add(mmcmd.ManifestId.ToString());
            }
            else if (icmd.Command == eCommand.ManifestDetails)
            {
                //dlrnos
                //Set the status of tyhe manifestDetail id to received.
            }
            else if (icmd.Command == eCommand.ManifestDetailsComplete)
            {
                //Set the status of tyhe manifestDetail id to completed.
                mdcmd = (ManifestDetailsData)icmd;

                //check if there are zero remaining with status Pending
                // Orders + OrderDetails + ManifestDetails
            }
            else if (icmd.Command == eCommand.OrdersLoad)
            {

            }
            else if (icmd.Command == eCommand.OrderUpdatesComplete)
            {
                OrderMasterData omd = (OrderMasterData)icmd;

            }
            else if (icmd.Command == eCommand.OrderDetails)
            {
                OrderDetailsData odd = (OrderDetailsData)icmd;

                //int wncnt = odd.WIN_CNT;
                //if (wncnt < 0) wncnt = 0;

                long id = Int64.Parse(odd.MDL_NO.ToString() +
                        odd.MDL_CNT.ToString() + odd.OPT_NUM.ToString() + odd.ORD_NO.ToString());

                if (!LoadManifestIdComplete.Contains(id.ToString()))
                {
                    Logger.Info($"Add ODC LoadManifestIdComplete: {odd.ORD_NO.ToString()}");
                    lock (olock)
                    {
                        if (!lstMissingIds.Contains(id))
                            LoadManifestIdComplete.Add(id.ToString());
                        else
                        {
                            lstMissingIds.Remove(id);
                            Logger.Info($"Ignore already in the missing container, Remove it ODC LoadManifestIdComplete: {odd.ORD_NO.ToString()}");
                        }
                    }
                }
                if ((lstMissingIds.Count + LoadManifestIdComplete.Count - odcnt == 0) && binitOD && (oocnt == 0 && binitOO))
                    bTerminateThread = true;
            }
            else if (icmd.Command == eCommand.OrderDetailsComplete)
            {
                OrderDetailsData oddc = (OrderDetailsData)icmd;
                if (!binitOD)
                {
                    odcnt = oddc.LineNumber;
                    binitOD = true;
                }

                if (LoadManifestIdComplete.Contains(oddc.ORD_NO.ToString()))
                {
                    Logger.Info($"Remove ODC LoadManifestIdComplete: {oddc.ORD_NO.ToString()}");
                    lock (olock)
                    {
                        LoadManifestIdComplete.Remove(oddc.ORD_NO.ToString());

                        if (lstMissingIds.Contains(oddc.ORD_NO))
                        {
                            Logger.Info($"Also Remove ODC lstMissingIds {oddc.ORD_NO.ToString()}");
                            lstMissingIds.Remove(oddc.ORD_NO);
                        }
                    }
                }
                else
                {
                    Logger.Info($"Add ODC Add lstMissingIds:{oddc.ORD_NO.ToString()}");
                    lock (olock)
                        lstMissingIds.Add(oddc.ORD_NO);
                }

                Logger.Info($"ODO check missing:{lstMissingIds.Count} + complete{LoadManifestIdComplete.Count} - odcnt:{odcnt} oocnt:{oocnt} ");
                if (lstMissingIds.Count + LoadManifestIdComplete.Count - odcnt == 0 && (oocnt == 0 && binitOO))
                    bTerminateThread = true;
            }
            else if (icmd.Command == eCommand.OrderOptions)
            {
                OrderOptionsData ood = (OrderOptionsData)icmd;

                if (LoadManifestIdComplete.Contains(ood.ORD_NO.ToString()))
                {
                    Logger.Info($"Remove OO LoadManifestIdComplete:{ood.ORD_NO.ToString()} LoadManifestIdComplete.cnt:{LoadManifestIdComplete.Count}");
                    lock (olock)
                    {
                        LoadManifestIdComplete.Remove(ood.ORD_NO.ToString());

                        if (lstMissingIds.Contains(ood.ORD_NO))
                        {
                            Logger.Info($"Also Remove OO lstMissingIds {ood.ORD_NO.ToString()} lstMissingIds cnt:{lstMissingIds.Count}");
                            lstMissingIds.Remove(ood.ORD_NO);
                        }
                    }
                }
                else
                {
                    Logger.Info($"Add OO Add lstMissingIds: {ood.ORD_NO.ToString()} lstMissingIds cnt:{lstMissingIds.Count}");
                    lock (olock)
                        lstMissingIds.Add(ood.ORD_NO);
                }

                Logger.Info($"OO check missing:{lstMissingIds.Count} + complete{LoadManifestIdComplete.Count} - oocnt:{oocnt} odcnt:{odcnt} ");
                if ((lstMissingIds.Count + LoadManifestIdComplete.Count - oocnt == 0) && binitOO && (odcnt == 0 && binitOD))
                    bTerminateThread = true;
            }
            else if (icmd.Command == eCommand.OrderOptionsComplete)
            {
                OrderOptionsData oodc = (OrderOptionsData)icmd;
                if (!binitOO)
                {
                    oocnt = oodc.Count;
                    binitOO = true;
                }

                if (LoadManifestIdComplete.Contains(oodc.ORD_NO.ToString()))
                {
                    lock (olock)
                    {
                        Logger.Info($"Remove OOComplete LoadManifestIdComplete: {oodc.ORD_NO.ToString()}");
                        LoadManifestIdComplete.Remove(oodc.ORD_NO.ToString());
                    }
                }
                else
                {
                    Logger.Info($"Add OOComplete lstMissingIds:{oodc.ORD_NO.ToString()}");
                    lock (olock)
                        lstMissingIds.Add(oodc.ORD_NO);
                
                }

                Logger.Info($"OOComplete check: Missing:{lstMissingIds.Count} + Complete:{LoadManifestIdComplete.Count} - oocnt:{oocnt}");
                if (lstMissingIds.Count + LoadManifestIdComplete.Count - oocnt == 0 && (odcnt == 0 && binitOD))
                    bTerminateThread = true;
            }
            else if (icmd.Command == eCommand.Trucks)
            {
               // TruckData td = (TruckData)icmd;
            }
            else if (icmd.Command == eCommand.ScanFile)
            {
                 ScanFileData sf = (ScanFileData)icmd;

            }
            else if (icmd.Command == eCommand.TrucksLoadComplete)
            {
               // TruckData tdc = (TruckData)icmd;
                bTerminateThread = true;
            }
            else if (icmd.Command == eCommand.Stops)
            {
               // StopData sd = (StopData)icmd;
            }
            else if (icmd.Command == eCommand.StopsLoadComplete)
            {
              //  StopData sdc = (StopData)icmd;
            }
            else if (icmd.Command == eCommand.OrdersLoad)
            {
               // OrderData od = (OrderData)icmd;
            }
            else if (icmd.Command == eCommand.OrdersLoadComplete)
            {
               // OrderData odc = (OrderData)icmd;

//                long id = odc.ORD_NO;

                //lock (olock)
                //{
                //    if (LoadManifestIdComplete.Contains(id.ToString()))
                //    {
                //        Logger.Info($"Remove (Error Should skip) OComplete check {LoadManifestIdComplete.Count}");
                //        LoadManifestIdComplete.Remove(id.ToString());
                //    }
                //}

                //if (dRequests.TryGetValue(icmd.RequestId, out valReq))
                //{
                //    if (valReq.LinkMid.ContainsKey(id))
                //        odc.status = status.Released;
                //    else
                //        odc.status = status.Completed;

                //    Logger.Info($"ManifestVM::Process Manifest {cnt} PENDING remaining.");
                //}
            }
            else
                Logger.Error($"Unhandled command Backgrtound worker handler {Enum.GetName(typeof(eCommand), icmd.Command)}.");

            //The request id has completed
            if (bTerminateThread)
            {
                Logger.Info($"ManifestVM::Process Terminate Thread for {icmd.RequestId}.");
                manifestMasterThread.CompleteBackgroundWorker(icmd.RequestId);
                LoadManifestRequestComplete = icmd.RequestId.ToString();
                LoadManifestCompleted = true;

            }
            
        }

        void Add(IMDMMessage icmd)
        {
            if (icmd.GetType() == typeof(ManifestMasterData))
            {
                ManifestMasterData mcmd = (ManifestMasterData)icmd;
                if (!manifestMasterData.Contains(mcmd))
                {
                    ROUTECOUNT++;
                    mcmd.COUNT = 1;
                    lock (olock)
                        ManifestMaster.Add(mcmd);
                }
                else
                {
                    var mcnt = manifestMasterData.Where(m => m.LINK == mcmd.LINK).FirstOrDefault();
                    mcmd.COUNT = mcnt.COUNT + 1;
                    lock (olock)
                    {
                       
                        ManifestMaster.Remove(mcnt);
                        if (mcnt.ManifestId != 0)
                            mcmd.ManifestId = mcnt.ManifestId;
                        ManifestMaster.Add(mcmd);
                    }
                }
            }
            else if(icmd.GetType() == typeof(ManifestDetailsData))
            {

            }
            else if(icmd.GetType() == typeof(OrderData))
            {

            }
            else if (icmd.GetType() == typeof(OrderOptionsData))
            {

            }
        }

        private void OnRouteSelected(object arg) { }

    }
}
