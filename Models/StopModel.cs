using System.ComponentModel;
using System.Runtime.CompilerServices;
using static MobileDeliveryGeneral.Definitions.enums;

namespace MobileDeliveryMVVM.Models
{
    public class StopModel
    {
       // public static IList<StopModel> Stops { get; private set; }

        
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool Equals(StopModel other)
        {
            return this.StopNum == other.StopNum & this.State == other.State && this.RouteCode == other.RouteCode;

        }
        public stopstate State { get; set; }
       
      //  public List<OrderDetailModel> Orders { get; set; }
      //  public Dictionary<long, long> dDealerNumtoOrderNum = new Dictionary<long, long>();
        //TRKDTL.DSP_SEQ
        public short StopNum { get; set; }

        public string Address { get; set; }

//        public string OrderNum { get; set; }
        public string RouteCode { get; set; }


        public StopModel()
        {
            this.State = stopstate.Incomplete;
            //this.StopNum = 1;
            //this.TruckId = "2";
            //this.Address = "123 Main St.";
           // this.Orders = new List<OrderDetailModel>();
            //this.dDealerNumtoOrderNum = new Dictionary<long, long>();
            OnPropertyChanged("StopCompleted");
        }
        public StopModel(stopstate state, short stopnum, string driver, string address, string truck, string Notes)
        {
            //this.TruckId = truck;
            //this.Notes = Notes;
            this.State = state;
            this.StopNum = stopnum;
            this.Address = address;
            //this.Orders = new List<OrderDetailModel>();
            //this.dDealerNumtoOrderNum = new Dictionary<long, long>();
            //this.Trucks.Add(new Truck { TRK_CDE=truck, driver=driver, State=stopstate.Loaded});
            OnPropertyChanged("StopCompleted");
        }
    }
}
