using System;
using System.ComponentModel;
using MobileDeliveryMVVM.BaseClasses;
using MobileDeliveryMVVM.Command;
using System.Threading;
using MobileDeliveryGeneral.Interfaces.DataInterfaces;
using MobileDeliveryGeneral.Data;

namespace MobileDeliveryMVVM.ViewModel
{
    public class MainPageVM : BaseViewModel<ManifestMasterData>
    {
        string title;
        public string Title { get { return title; } set { SetProperty<string>(ref title, value); } }

        public ICommand ConnectCommand { get; set; }

        //static ProgressChanged<ManifestMasterData> main = null;
        public MainPageVM() : base(new MobileDeliveryGeneral.Settings.UMDAppConfig() { AppName = "MainPageVM" })
        {
            ViewContext = SynchronizationContext.Current;
        }
        private void OnConnect()
        {
            //InitConnections();
        }

        public void Connect() {
            //_connMod.Connect();
        }
        public void Disconnect()
        {
            //_connMod.Disconnect();
        }
        void RefreshCanExecutes()
        {
           // ConnectCommand.CanExecute(ConnectCommand);
          //  DisconnectCommand.CanExecute(DisconnectCommand);
        }

        public void ActionDlgProc(Action<object> dlg, PropertyChangedEventHandler OnDisconnectPropertyChanged)
        {
            PropertyChanged += OnDisconnectPropertyChanged;
          //  IsConnected = ConnectState.Connected;
            RefreshCanExecutes();
        }

    }
}
