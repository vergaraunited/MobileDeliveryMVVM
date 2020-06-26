using MobileDeliveryMVVM.Command;
using System;
using System.Collections.ObjectModel;
using MobileDeliveryGeneral.Interfaces.DataInterfaces;
using MobileDeliveryGeneral.Definitions;
using MobileDeliveryGeneral.Settings;
using MobileDeliveryMVVM.BaseClasses;

namespace MobileDeliveryMVVM.ViewModel
{
    public class OrderGroupVM : BaseViewModel<OrderGroupVM>, IMDMMessage
    {
        OrderOptionsVM _oldOrderVM;

        private ObservableCollection<OrderOptionsVM> items;
        public ObservableCollection<OrderOptionsVM> Items
        {
            get { return items; }

            set { SetProperty<OrderOptionsVM>(ref items, value); }
        }

        private void SetProperty<T>(ref ObservableCollection<T> items, ObservableCollection<T> value)
        {
            //throw new NotImplementedException();
        }

        public DelegateCommand LoadOrdersCommand { get; set; }
        public DelegateCommand<OrderOptionsVM> RefreshItemsCommand { get; set; }
        public MsgTypes.eCommand Command { get; set; }
        public Guid RequestId { get; set; }
        public status status { get; set; }

        public OrderGroupVM() : base(new SocketSettings()
        {
            url = "localhost",
            port = 81,
            srvurl = "localhost",
            srvport = 81,
            clienturl = "localhost",
            clientport = 8181,
            name = "OrderGroupVM"
        }, "OrderGroupVM")
        { }
    }
}
