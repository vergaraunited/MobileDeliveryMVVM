using MobileDeliveryMVVM.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MobileDeliveryMVVM.BaseClasses
{
    public class UMDComparer : IEqualityComparer<StopModel>
    {
        public bool Equals(StopModel x, StopModel y)
        {
            return x.Address == y.Address && x.StopNum == y.StopNum;//&& x.Orders == y.Orders;
        }

        public int GetHashCode(StopModel obj)
        {
            throw new NotImplementedException();
        }
    }
}
