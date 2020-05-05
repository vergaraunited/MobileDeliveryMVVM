using MobileDeliveryGeneral.Data;
using MobileDeliveryGeneral.Definitions;
using MobileDeliveryGeneral.Interfaces;
using MobileDeliveryGeneral.Interfaces.DataInterfaces;
using MobileDeliveryGeneral.Settings;
using MobileDeliveryGeneral.Threading;
using MobileDeliveryLogger;
using MobileDeliveryMVVM.BaseClasses;
using MobileDeliveryMVVM.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using static MobileDeliveryGeneral.Definitions.MsgTypes;

namespace MobileDeliveryMVVM.ViewModel
{
    public class AccountsReceivableVM : BaseViewModel<AccountsReceivableData>
    {
        public ICommand LoadARCommand { get; set; }
        public ICommand LoadOrderDetailsCommand { get; set; }
        public ICommand InvoiceCommand { get; set; }

        
            BindingList<OrderDetailsData> orderDetailsData = new BindingList<OrderDetailsData>();

        public BindingList<OrderDetailsData> OrderDetails
        {
            get { return orderDetailsData; }
            set
            {
                SetProperty<BindingList<OrderDetailsData>>(ref orderDetailsData, value);
            }
        }


        BindingList<AccountsReceivableData> aRData = new BindingList<AccountsReceivableData>();

        public BindingList<AccountsReceivableData> ARData
        {
            get { return aRData; }
            set
            {
                SetProperty<BindingList<AccountsReceivableData>>(ref aRData, value);
            }
        }

        #region BackgroundWorkers
        UMBackgroundWorker<IMDMMessage> manifestMasterThread;
        UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage> pcManifestMaster;
        #endregion

        object olock = new Object();

        public AccountsReceivableVM(UMDAppConfig config) : base(config.srvSet, config.AppName)
        {
            Init();
        }

        public AccountsReceivableVM() : base(new SocketSettings()
        {
            url = "localhost",
            port = 81,
            srvurl = "localhost",
            srvport = 81,
            clienturl = "localhost",
            clientport = 8181,
            name = "ARVM"
        }, "ARVM")
        {
            Init();
        }

        void Init()
        {
            pcManifestMaster = new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(ProcessMessage);
            manifestMasterThread = new UMBackgroundWorker<IMDMMessage>(new UMBackgroundWorker<IMDMMessage>.ProgressChanged<IMDMMessage>(pcManifestMaster), rm, sm);

            LoadARCommand = new DelegateCommand(OnARLoad);
            LoadOrderDetailsCommand = new DelegateCommand(OnOrderDetailsLoad);
            InvoiceCommand = new DelegateCommand(OnInvoiceSelected);

           // Clear();
        }

        private void OnInvoiceSelected(object obj)
        {
            throw new NotImplementedException();
        }
        private void OnOrderDetailsLoad(object obj)
        {
            throw new NotImplementedException();
        }
        
        private void OnARLoad(object obj)
        {
            throw new NotImplementedException();
        }

        public void ReleaseManifest(orderDetails mmd)
        {
            OnInvoiceSelected(mmd);
        }

        void UploadARData(orderDetails odd)
        {
          //  if (odd.IsSelected)
            {
                Logger.Info($"ARVM Upload_orderDetails: {odd.ToString()}");

                //Clear();

                var req = new Request()
                {
                    //reqGuid = odd.RequestId,
                    LIds = new Dictionary<long, status>(),
                    LinkMid = new Dictionary<long, List<long>>(),
                    ChkIds = new Dictionary<long, status>()
                };
                dRequests.Add(req.reqGuid, req);

                AccountsReceivableData manUp = new AccountsReceivableData(odd);
                //manUp.command = eCommand.UploadManifest;

                //ProcessMsgDelegateRXRaw pmRx = new ProcessMsgDelegateRXRaw(ProcessMessage);
                //Logger.Info($"Upload Manifest reqie: {req.reqGuid}");
                //manifestMasterThread.OnStartProcess((new manifestRequest()
                //{
                //    requestId = req.reqGuid.ToByteArray(),
                //    command = manUp.command,
                //    bData = manUp.ToArray()
                //}), req, pmRx);
            }
        }

        void Clear(Guid key)
        {
            if (manifestMasterThread != null)
                manifestMasterThread.Reset(key);
            ARData.Clear();
        }
        public void ProcessMessage(IMDMMessage icmd, Func<byte[], Task> cbsend = null)
        {
            OrderDetailsData odcmd;

            if (icmd.Command == eCommand.OrderDetails)
            {
                odcmd = (OrderDetailsData)icmd;
                odcmd.status = status.Init;
                Add(odcmd);

                //Winsys loading of the Manifest Master update to the original requestId for a ship date
                Logger.Info($"ARVM::Process Message from Winsys: Order Details Query: {odcmd.ToString()}");
                
            }
            else if (icmd.Command == eCommand.CheckManifest)
            {
                odcmd = (OrderDetailsData)icmd;
                odcmd.status = status.Init;
                Add(odcmd);
                Logger.Info($"ManifestVM::Process Message: Check Manifest Result: {odcmd.ToString()}");

            }
        }
        void Add(OrderDetailsData odd)
        {
            //LineCount++;
            lock (olock)
            {
                if (odd.status == status.Completed)
                {
                    ARData.Add(new AccountsReceivableData(new orderDetails(odd)));
                }
                if (!OrderDetails.Contains(odd))
                {
                    odd.status = status.Init;
                    OrderDetails.Add(odd);
                    odd.OnSelectionChanged = new OrderDetailsData.cmdFireOnSelected(InvoiceOrderData);
                }
                else
                {
                    OrderDetails.Remove(odd);
                    odd.status = status.Init;
                    OrderDetails.Add(odd);
                    odd.OnSelectionChanged = new OrderDetailsData.cmdFireOnSelected(InvoiceOrderData);
                }
            }
        }




        void InvoiceOrderData(OrderDetailsData ord)
        {
            Logger.Info($"ARVM UploadInvoiceOrder: {ord.ToString()}");
            OrderDetailsData od = null;
            if (OrderDetails.Contains(ord))
            {
                od = OrderDetails[OrderDetails.IndexOf(ord)];

                if (ord.status == status.Pending && od != null && od.IsSelected == true) //|| ord.status == status.Pending)
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

            //orderMaster ordUp = new orderMaster(ord, ord.ManifestId);
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
