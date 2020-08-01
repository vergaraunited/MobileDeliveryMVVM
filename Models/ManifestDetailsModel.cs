using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static MobileDeliveryGeneral.Definitions.MsgTypes;

namespace MobileDeliveryMVVM.Models
{
    public class ManifestDetailsModel :INotifyPropertyChanged
    {

       // public manifestDetails ManifestDetails { get; protected set; }
        public string ManifestID { get; private set; }
        public string TruckId { get; set; }
        public string DriverId { get; set; }
        public string RouteCode { get; set; }

        public ObservableCollection<manifestDetails> ManifestDetails { get; set; }
        public ManifestDetailsModel(string manifestID) //, List<StopModel> stops) : base(stops)
        {
            ManifestID = manifestID;
         
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public override string ToString()
        {
            return ManifestID;
        }
        //public ObservableCollection<StopModel> getStops()
        //{
        //    return Stops;
        //}

        public void persist(StopModel stop)
        {
            // Send this to the server

        }
        public void persist(List<StopModel> stops)
        {
            // Send this to the server
        }


      
    }
}
