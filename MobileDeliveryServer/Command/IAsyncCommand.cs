﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileDeliveryMVVM.Command
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync();
        bool CanExecute();
    }

    public interface IAsyncCommand<T> : ICommand
    {
        Task ExecuteAsync(T parameter);
        bool CanExecute(T parameter);
    }

}
