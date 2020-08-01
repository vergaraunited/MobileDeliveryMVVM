using System;

namespace MobileDeliveryMVVM.Command
{
    public interface IErrorHandler
    {
        void HandleError(Exception ex);
    }
}
