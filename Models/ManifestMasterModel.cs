using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MobileDeliveryGeneral.Data;
using static MobileDeliveryGeneral.Definitions.MsgTypes;
using MobileDeliveryLogger;

namespace MobileDeliveryMVVM.Models
{
    public class ManifestMasterModel// : INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler PropertyChanged;

        //public void OnPropertyChanged([CallerMemberName]string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
        public int ManifestId { get; set; }
        public string Userid { get; set; }
        public string TRK_CDE { get; set; }
       
        public string Desc { get; set; }
        public string NOTES { get; set; }
        public long LINK { get; set; }
        public bool TRUCKISCLOSED { get; set; }
        public DateTime SEAL_DTE { get; set; }
        public DateTime SHIP_DTE { get; set; }
        public short SHP_QTY { get; set; }
        public int COUNT { get; set; }
        //public eCommand Command { get; set; }
    }
}
