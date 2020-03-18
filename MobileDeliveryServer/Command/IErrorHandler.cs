using System;
using System.Collections.Generic;
using System.Text;

namespace MobileDeliveryMVVM.Command
{
    public interface IErrorHandler
    {
        void HandleError(Exception ex);
    }
}
