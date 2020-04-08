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
    public class ManifestVM : BaseViewModel<ManifestMasterData>
    {
        #region fields
        ObservableCollection<string> loadManifestIdComplete = new ObservableCollection<string>();
        ObservableCollection<string> uploadManifestIdComplete = new ObservableCollection<string>();
        DateTime manifestDate;
        const int maxStackCount = 30;
        int routecount = 0;
        int totorders = 0;
        int manId = 0;
        bool isSelected = false;
        #endregion

        #region properties
        public ICommand LoadManifestCommand { get; set; }
        public ICommand RouteSelectedCommand { get; set; }
        public ICommand ReleaseManifestCommand { get; set; }
       
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
        UMBackgroundWorker<ManifestMasterData> manifestMasterThread;
        UMBackgroundWorker<ManifestMasterData>.ProgressChanged<ManifestMasterData> pcManifestMaster;
        #endregion

        public ManifestVM(UMDAppConfig config) : base(config)
        {
            Init();
        }

        public ManifestVM() : base(new UMDAppConfig() { AppName = "ManifestVM" })
        {
            Init();
        }

        void Init()
        {
            pcManifestMaster = new UMBackgroundWorker<ManifestMasterData>.ProgressChanged<ManifestMasterData>(ProcessMessage);
            manifestMasterThread = new UMBackgroundWorker<ManifestMasterData>(new UMBackgroundWorker<ManifestMasterData>.ProgressChanged<ManifestMasterData>(pcManifestMaster), rm, sm);

            //ManifestMaster.CollectionChanged += (s, e) =>
            //{
            //    ManifestMaster = manifestMasterData;
            //};
            //LoadManifestIdComplete.CollectionChanged += (s, e) =>
            //{
            //    LoadManifestIdComplete = loadManifestIdComplete;
            //};

            LoadManifestCommand = new DelegateCommand(OnManifestManifestLoad);

            RouteSelectedCommand = new DelegateCommand(OnRouteSelected);
            ReleaseManifestCommand = new DelegateCommand(OnReleaseManifestSelected);

            this.manifestDate = DateTime.Now;
        }

        public void ReleaseManifest(ManifestMasterData mmd) {
            //umdSrv.SendMessage(new manifestRequest() { command = eCommand.UploadManifest, bData = new manifestMaster(mmd).ToArray() });
           OnReleaseManifestSelected(mmd);
        }

        void UploadManifestMasterData(ManifestMasterData mm)
        {
            if (mm.IsSelected)
            {
                Logger.Info($"ManifestVM UploadManifestMasterData.  Link: {mm.LINK}  ReqId: {mm.RequestId}  Command: {mm.Command}");
                manifestMaster manUp = new manifestMaster(mm);
                manUp.command = eCommand.UploadManifest;
                umdSrv.SendMessage(new manifestRequest() { requestId = mm.RequestId.ToByteArray(), command = manUp.command, bData = manUp.ToArray() });
            }
        }

        bool CheckManifestStatus(manifestMaster md)
        {
            Logger.Info($"ManifestVM::CheckManifestStatus: link: {md.LINK} - reqid: {new Guid(md.requestId).ToString()} - {md.command.ToString()}");
            return umdSrv.SendMessage(md);
        }
        private void OnReleaseManifestSelected(object arg)
        {
            if (arg != null)
            {
                //Winform Manifest Master release
                ManifestMasterData mm = (ManifestMasterData)arg;
                if (mm.RequestId == null || mm.RequestId == Guid.Empty)
                    mm.RequestId = new Guid();

                UploadManifestMasterData(mm);
            }
            else
            {
                foreach (var mm in manifestMasterData)
                {
                    UploadManifestMasterData(mm);
                }
            }
        }

        public void LoadData(manifestRequest req)
        {
            winSys.SendMessage(req);
        }

        public override isaCommand ReceiveMessageCB(isaCommand cmd)
        {
            switch (cmd.command)
            {
                case eCommand.Manifest:
                    manifestMasterThread.ReportProgress(50, new object[] { cmd });
                    break;
                case eCommand.ManifestLoadComplete:
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
        public void OnManifestManifestLoad(object arg)
        {
            Clear();

            var req = new Request() {
                reqGuid = Guid.NewGuid(),
                LIds = new Dictionary<long, status>(),
                MIds = new Dictionary<long, status>()
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
        {
        }

        object olock = new object();
        public void ProcessMessage(IMDMMessage icmd, Func<byte[], Task> cbsend=null)
        {
            lock (olock)
            {
                ManifestMasterData mcmd = (ManifestMasterData)icmd;
                bool bTerminateThread = false;

                if (mcmd.ManifestId == 0 && icmd.Command == eCommand.Manifest)
                {
                    //These are updates to the original requestId
                    mcmd.Command = eCommand.GenerateManifest;
                    Logger.Info($"ManifestVM::Process Message: Add Init Query link: {mcmd.LINK} - {mcmd.RequestId} - {mcmd.Command.ToString()}");
                    if (!LoadManifestIdComplete.Contains(mcmd.LINK.ToString()))
                        LoadManifestIdComplete.Add(mcmd.LINK.ToString());

                    Request valReq;
                    if (dRequests.TryGetValue(mcmd.RequestId, out valReq))
                    {
                        if (!valReq.LIds.ContainsKey(mcmd.LINK))
                        {
                            Logger.Info($"ManifestVM::Add Init Query link: {mcmd.LINK} - {mcmd.RequestId} - {mcmd.Command.ToString()}");
                            valReq.LIds.Add(mcmd.LINK, status.Init);
                        }
                        else
                        {
                            Logger.Info($"ManifestVM::Update Init to Releasing for link: {mcmd.LINK} - {mcmd.RequestId} - {mcmd.Command.ToString()}");
                            valReq.LIds[mcmd.LINK] = status.Releasing;
                        }
                    }
                    else
                    {
                        Logger.Info($"ManifestVM::Add valReq for link: {mcmd.LINK} - {mcmd.RequestId} - {mcmd.Command.ToString()}");
                        dRequests.Add(mcmd.RequestId, new Request() { reqGuid = mcmd.RequestId, LIds = new Dictionary<long, status>() });
                    }

                    //Search the Manifest from the SQL Server to see if it was already released.
                    CheckManifestStatus(new manifestMaster(mcmd));
                }
                else if (mcmd.ManifestId != 0 && mcmd.Command == eCommand.ManifestLoadComplete)
                {
                    //this is the end of query results for requestId
                    mcmd.Command = eCommand.ManifestLoadComplete;
                    Logger.Info($"ManifestVM::Process Message: mmid: {mcmd.ManifestId} - {mcmd.LINK} - {mcmd.RequestId} - {mcmd.Command.ToString()}");

                    if (LoadManifestIdComplete.Contains(mcmd.LINK.ToString()))
                        LoadManifestIdComplete.Remove(mcmd.LINK.ToString());

                    if (!LoadManifestIdComplete.Contains(mcmd.ManifestId.ToString()))
                        LoadManifestIdComplete.Add(mcmd.ManifestId.ToString());
                    
                    Request valReq;
                    if (dRequests.TryGetValue(mcmd.RequestId, out valReq))
                    { 
                        var it = valReq.LIds.Select(b => b).Where(a => a.Key.CompareTo((Int32)mcmd.LINK) == 0).FirstOrDefault();

                        if (LoadManifestIdComplete.Contains(mcmd.LINK.ToString()))
                        {
                            LoadManifestIdComplete.Remove(mcmd.LINK.ToString());
                            LoadManifestIdComplete.Add(mcmd.ManifestId.ToString());
                        }
                        if ( valReq.MIds == null)
                            valReq.MIds = new Dictionary<long, status>();

                        valReq.MIds.Add(mcmd.ManifestId, status.Uploaded);
                        valReq.LIds.Remove(mcmd.LINK);

                        if (valReq.LIds.Select(a => a).Where(b => b.Value.CompareTo(status.Init) == 0).Count() == 0)
                        {
                            bTerminateThread = true;
                        }
                        if (valReq.LIds.Count == 0)
                        {
                            bTerminateThread = true;
                        }   
                    }
                }
                else if (mcmd.Command == eCommand.ManifestLoadComplete)
                {
                    Request valReq;
                    Logger.Info($"ManifestVM::Process Manifest Load Complete Message: reqId: {mcmd.RequestId} - " +
                        $"{mcmd.Command.ToString()} " +
                        $"- {mcmd.LINK} ");
                    if (mcmd.LINK == 0)
                    {
                        Logger.Info($"ManifestVM::Process Manifest Discard Load Complete Message: reqId: {mcmd.RequestId}");
                        return;
                    }
                    if (dRequests.TryGetValue(mcmd.RequestId, out valReq))
                    {
                        int cnt = valReq.LIds.Select(a => a).Where(b => b.Value.CompareTo(status.Init) == 0).Count();

                        if ((Int32)mcmd.ManifestId == 0)
                        {
                            var it = valReq.LIds.Select(b => b).Where(a => a.Key.CompareTo((Int32)mcmd.LINK) == 0).FirstOrDefault();
                            
                            status est = ((KeyValuePair<long, status>)it).Value;
                            if (est == status.Init)
                            {    
                                Logger.Info($"ManifestVM::Process Manifest decrementing from {cnt} Inits. Flipping {mcmd.LINK} Request to PENDING.  reqId: {mcmd.RequestId} - {mcmd.Command.ToString()}");
                                valReq.LIds[mcmd.LINK] = status.Pending;
                            }
                            else
                                valReq.LIds[mcmd.LINK] = status.Completed;

                            cnt = valReq.LIds.Select(a => a).Where(b => b.Value.CompareTo(status.Init) == 0).Count();
                            Logger.Info($"ManifestVM::Process Manifest decrementing New Count {cnt} Inits. Flipping {mcmd.LINK} Request to PENDING.  reqId: {mcmd.RequestId} - {mcmd.Command.ToString()}");
                        }
                        else
                        {
                            Logger.Info($"ManifestVM::Process Manifest Load Complete Terminate Thread.  " +
                                $"reqId: {mcmd.RequestId} - {mcmd.Command.ToString()} - {mcmd.LINK} - {mcmd.ManifestId}");
                            Logger.Info($"ManifestVM::Process Manifest Init count {cnt} Inits link: {mcmd.LINK}.  reqId: {mcmd.RequestId} - {mcmd.Command.ToString()}");

                            if (valReq.LIds.ContainsKey(mcmd.LINK))
                                valReq.LIds.Remove(mcmd.LINK);

                            if (valReq.MIds.ContainsKey(mcmd.ManifestId))
                                valReq.MIds[mcmd.ManifestId] = status.Uploaded;
                            else
                                valReq.MIds.Add(mcmd.ManifestId, status.Uploaded);

                            mcmd.Command = eCommand.ManifestLoadComplete;

                            LoadManifestIdComplete.Add(mcmd.ManifestId.ToString());
                        }
                        cnt = valReq.LIds.Select(a => a).Where(b => b.Value.CompareTo(status.Init) == 0).Count();
                        Logger.Info($"ManifestVM::Process Manifest {cnt} PENDING remaining.  reqId: {mcmd.RequestId} - {mcmd.LINK} - {mcmd.Command.ToString()}");

                        if (LoadManifestIdComplete.Contains(mcmd.LINK.ToString()))
                            LoadManifestIdComplete.Remove(mcmd.LINK.ToString());

                        if (cnt == 0)
                        {
                            Logger.Info($"ManifestVM::Process Manifest Load Complete Terminate Thread.  reqId: {mcmd.RequestId} - {mcmd.LINK} - {mcmd.Command.ToString()}");
                            bTerminateThread = true;
                        }
                    }
                }

                //Add to the collection
                Add(mcmd);
                //The request id has completed
                if (bTerminateThread)
                {
                    manifestMasterThread.CompleteBackgroundWorker(mcmd.RequestId);
                    LoadManifestRequestComplete = mcmd.RequestId.ToString();
                }
            }
        }

        void Add(ManifestMasterData mcmd)
        {
            if (!manifestMasterData.Contains(mcmd))
            {
                ROUTECOUNT++;
                mcmd.COUNT = 1;
                ManifestMaster.Add(mcmd);
            }
            else
            {
                var mcnt = manifestMasterData.Where(m => m.LINK == mcmd.LINK).FirstOrDefault();
                if (mcnt.ManifestId > 0 && mcmd.ManifestId == 0)
                    return;
                mcmd.COUNT = mcnt.COUNT + 1;
                ManifestMaster.Remove(mcnt);
                ManifestMaster.Add(mcmd);
            }
        }
        private void OnRouteSelected(object arg) { }
    }
}
