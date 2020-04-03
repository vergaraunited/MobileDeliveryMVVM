using MobileDeliveryMVVM.BaseClasses;
using MobileDeliveryMVVM.Command;
using System;
using System.Collections.ObjectModel;
using MobileDeliveryGeneral.Data;
using static MobileDeliveryGeneral.Definitions.MsgTypes;
using MobileDeliveryLogger;
using System.Threading.Tasks;
using MobileDeliveryGeneral.Settings;

namespace MobileDeliveryMVVM.ViewModel
{
    public class DriverVM : BaseViewModel<DriverData>
    {
        # region fields
        const int maxStackCount = 30;
        //static Settings settings;
        #endregion

        #region properties
        public ICommand LoadDriversCommand { get; set; }
        bool loadDriversComplete;
        public bool LoadDriversComplete
        {
            get { return loadDriversComplete; }
            set { SetProperty<bool>(ref loadDriversComplete, value); }
        }
        #endregion

        #region observables
      
        ObservableCollection<DriverData> driverData = new ObservableCollection<DriverData>();

        public ObservableCollection<DriverData> Drivers
        {
            get { return driverData; }
            private set
            {
                SetProperty(ref driverData, value);
            }
        }

        #endregion
        
        public DriverVM() : base(new UMDAppConfig() { AppName = "DriverVM" })
        {
            LoadDriversCommand = new DelegateCommand(OnDriversLoad);
            Drivers.CollectionChanged += (s, e) =>
            {
                driverData= Drivers;
            };
        }

        public void OnDriversLoad(object arg)
        {
            DateTime dte = (DateTime)arg;
            Clear();

            Logger.Debug("OnDriversLoad");

            LoadDrivers(new manifestRequest() { command = eCommand.Drivers, date = dte.Date.ToString("yyyy-MM-dd") });
        }
        void Clear()
        {
            LoadDriversComplete = false;
            Drivers.Clear();
        }
        public void LoadDrivers(manifestRequest req)
        {
            winSys.SendMessage(req);
        }

        void ProcessMessage(DriverData cmd, Func<byte[], Task> cbsend=null)
        {
            if (cmd.Command == eCommand.DriversLoadComplete)
            {
                LoadDriversComplete = true;
                return;
            }
            var driver = cmd;
            if (driverData == null)
                driverData = new ObservableCollection<DriverData>();

            if (!driverData.Contains(driver))
            {
                driverData.Add(driver);
            }
            else
            {
                driverData.Add(driver);
                driverData.Remove(driver);
                driverData.Add(driver);
            }
        }

        public override isaCommand ReceiveMessageCB(isaCommand cmd)
        {
            switch (cmd.command)
            {
                case eCommand.Drivers:
                    ProcessMessage((DriverData)cmd);
                    break;
                case eCommand.DriversLoadComplete:
                    LoadDriversComplete = true;
                    break;
            }
            return cmd;
        }
    }
}
