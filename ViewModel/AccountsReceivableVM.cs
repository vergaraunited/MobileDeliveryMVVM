using MobileDeliveryGeneral.Data;
using MobileDeliveryGeneral.Definitions;
using MobileDeliveryGeneral.Interfaces;
using MobileDeliveryGeneral.Interfaces.DataInterfaces;
using MobileDeliveryGeneral.Threading;
using MobileDeliveryLogger;
using MobileDeliveryMVVM.BaseClasses;
using MobileDeliveryMVVM.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using static MobileDeliveryGeneral.Definitions.MsgTypes;

namespace MobileDeliveryMVVM.ViewModel
{
    public class AccountsReceivableVM : BaseViewModel<AccountsReceivableData>
    {
        public ICommand LoadInvoicesCommand { get; set; }
        public ICommand ReleaseInvoiceCommand { get; set; }

        ObservableCollection<AccountsReceivableData> invoiceData = new ObservableCollection<AccountsReceivableData>();

        public ObservableCollection<AccountsReceivableData> InvoiceData
        {
            get { return invoiceData; }
            set
            {
                SetProperty<ObservableCollection<AccountsReceivableData>>(ref invoiceData, value);
            }
        }

        #region BackgroundWorkers
        UMBackgroundWorker<IMDMMessage> arThread;
        UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage> pcAR;
        #endregion

        object olock = new Object();

        public AccountsReceivableVM() : base()
        {
            Init();
        }

        void Init()
        {
            pcAR = new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(ProcessMessage);
            arThread = new UMBackgroundWorker<IMDMMessage>(new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(pcAR), rm, sm);

            //LoadARCommand = new DelegateCommand(OnInvoiceLoad);
            LoadInvoicesCommand = new DelegateCommand(OnInvoiceLoad);
            ReleaseInvoiceCommand = new DelegateCommand(OnReleaseInvoice);

            InvoiceData.CollectionChanged += (s, e) =>
            {
                InvoiceData = invoiceData;
            };

            // Clear();
            UploadARData(new AccountsReceivableData());
        }

        private void OnInvoiceSelected(object obj)
        {
            throw new NotImplementedException();
        }
        private void OnOrderDetailsLoad(object obj)
        {
            throw new NotImplementedException();
        }
        
        private void OnInvoiceLoad(object obj)
        {
            UploadARData(new AccountsReceivableData());
            //throw new NotImplementedException();
        }

        public void OnReleaseInvoice(object mmd)
        {
            OnInvoiceSelected(mmd);
        }

        void UploadARData(AccountsReceivableData ard)
        {
          //  if (odd.IsSelected)
            {
                Logger.Info($"ARVM UploadARData: {ard.ToString()}");

                Clear(Guid.Empty);

                var req = new Request()
                {
                    //reqGuid = odd.RequestId,
                    LIds = new Dictionary<long, status>(),
                    LinkMid = new Dictionary<long, List<long>>(),
                    ChkIds = new Dictionary<long, status>()
                };
                dRequests.Add(req.reqGuid, req);

                accountReceivable manUp = new accountReceivable(ard);
                manUp.command = eCommand.AccountReceivable;

                ProcessMsgDelegateRXRaw pmRx = new ProcessMsgDelegateRXRaw(ProcessMessage);
                Logger.Info($"Upload Manifest reqie: {req.reqGuid}");
                arThread.OnStartProcess((new manifestRequest()
                {
                    requestId = req.reqGuid.ToByteArray(),
                    command = manUp.command,
                    bData = manUp.ToArray()
                }), req, pmRx);
            }
        }

        void Clear(Guid key)
        {
            if (arThread != null && key != Guid.Empty)
                arThread.Reset(key);
            InvoiceData.Clear();
            dRequests.Clear();
        }
        public override isaCommand ReceiveMessageCB(isaCommand cmd)
        {
            switch (cmd.command)
            {
                case eCommand.AccountReceivable:
                    arThread.ReportProgress(50, new object[] { cmd });
                    break;
                case eCommand.OrdersLoadComplete:
                    //LoadOrdersComplete = true;
                    arThread.ReportProgress(100, new object[] { cmd });
                    break;
                default:
                    Logger.Debug("AccountsReceivableVM::ReceiveMessageCB - Unhandled message.");
                    break;
            }
            return cmd;
        }
        public void ProcessMessage(IMDMMessage icmd, Func<byte[], Task> cbsend = null)
        {
            AccountsReceivableData odcmd;

            if (icmd.Command == eCommand.AccountReceivable)
            {
                odcmd = (AccountsReceivableData)icmd;
                odcmd.status = status.Init;
                Add(odcmd);

                //Winsys loading of the Manifest Master update to the original requestId for a ship date
                Logger.Info($"ARVM::Process Message For Accounts Receivables: {odcmd.ToString()}");
            }
            else
                Logger.Error($"ManifestVM::Process Message For Accounts Receivables: {icmd.ToString()}");
        }
        public void ProcessMessage(byte[] bcmd, Func<byte[], Task> cbsend = null)
        { }
        void Add(AccountsReceivableData odd)
        {
            if (odd.Signature == null || odd.ORD_NO == 0)
                return;
            //LineCount++;
            lock (olock)
            {
                if (odd.status == status.Completed)
                {
                    InvoiceData.Add(new AccountsReceivableData(new accountReceivable(odd)));
                }
                if (!InvoiceData.Contains(odd))
                {
                    odd.status = status.Init;
                    InvoiceData.Add(odd);
                }
                else
                {
                    //InvoiceData.Remove(odd);
                    odd.status = status.Init;
                    InvoiceData.Add(odd);
                }
            }
        }




        void InvoiceOrderData(AccountsReceivableData ord)
        {
            Logger.Info($"ARVM UploadInvoiceData: {ord.ToString()}");
            AccountsReceivableData od = null;
            if (InvoiceData.Contains(ord))
            {
                od = InvoiceData[InvoiceData.IndexOf(ord)];

                if (ord.status == status.Pending && od != null ) // && od.IsSelected == true) //|| ord.status == status.Pending)
                {
                    od.status = status.Uploaded;
                    return;
                }

                var req = new Request()
                {
                    reqGuid = NewGuid(),
                    LIds = new Dictionary<long, status>(),
                    LinkMid = new Dictionary<long, List<long>>(),
                    ChkIds = new Dictionary<long, status>()
                };
                dRequests.Add(req.reqGuid, req);
            }

          //  invoiceData ordUp = new invoiceData(ord); //, ord.ManifestId);
            //ordUp.command = eCommand.CompleteStop;
            //ordUp.Status = OrderStatus.Delivered;

            //ProcessMsgDelegateRXRaw pmRx = new ProcessMsgDelegateRXRaw(ProcessMessage);
            //Logger.Info($"Upload Sopt Order reqid: {req.reqGuid}");
            //orderThread.OnStartProcess((new manifestRequest()
            //{
            //    requestId = req.reqGuid.ToByteArray(),
            //    command = ordUp.command,
            //    bData = ordUp.ToArray()
            //}), req, pmRx);
        }

    }
}
