using MobileDeliveryGeneral.Data;
using MobileDeliveryMVVM.BaseClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MobileDeliveryMVVM.Models
{
    public class OrderGroup : ObservableCollection<Grouping<int, OrderModelData>>, INotifyPropertyChanged
    {
        private bool _expanded;

        public int ORD_NO { get; set; }

        public string OrderWithItemCount
        {
            get { return string.Format("{0} ({1})", ORD_NO, OrderCount); }
        }

        public string ShortName { get; set; }

        public bool Expanded
        {
            get { return _expanded; }
            set
            {
                if (_expanded != value)
                {
                    _expanded = value;
                    OnPropertyChanged("Expanded");
                    OnPropertyChanged("StateIcon");
                }
            }
        }

        public string StateIcon
        {
            get { return Expanded ? "expanded_blue.png" : "collapsed_blue.png"; }
        }

        public int OrderCount { get; set; }

        public OrderGroup()
        {
            AllOrders = new ObservableCollection<OrderMasterData>();
           // AllOrders.Add(new OrderGroup(or)); 
        }
        public void AddOrder(OrderMasterData or)
        {
            AllOrders.Add(or);
        }
        public ObservableCollection<OrderMasterData> AllOrders { private set; get; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
