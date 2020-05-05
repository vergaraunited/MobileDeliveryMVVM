using MobileDeliveryGeneral.Data;
using MobileDeliveryGeneral.Settings;
using MobileDeliveryMVVM.BaseClasses;
using MobileDeliveryMVVM.Command;
using System;
using System.Collections.Generic;
using System.Text;

namespace MobileDeliveryMVVM.ViewModel
{
    public class ClosesStopVM : BaseViewModel<OrderData>
    {
        string name;
        public string Name
        {
            get { return name; }
            set { SetProperty<string>(ref name, value); }
        }
        private DelegateCommand dismissButtonClicked;
        public DelegateCommand OnDismissButtonClicked
        { get { return dismissButtonClicked ?? (dismissButtonClicked = new DelegateCommand(OnDismiss)); } }

        private void OnDismiss(object obj)
        {
            // throw new NotImplementedException();
        }

        public ClosesStopVM(SocketSettings set, string name) : base(set, name)
        {
            Name = name;
        }
        public ClosesStopVM() : base(new SocketSettings()
        {
            url = "localhost",
            port = 81,
            srvurl = "localhost",
            srvport = 81,
            clienturl = "localhost",
            clientport = 8181,
            name = "CloseStopVM"
        }, "CloseStopVM")
        {
        }

    }
}