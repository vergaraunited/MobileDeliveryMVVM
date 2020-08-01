﻿using System;

namespace MobileDeliveryMVVM.Command
{
    public interface ICommand
    {
        void Execute(object parameter);
        bool CanExecute(object parameter);

        event EventHandler CanExecuteChanged;
        //Action Execute { get; set; }
    }
}
