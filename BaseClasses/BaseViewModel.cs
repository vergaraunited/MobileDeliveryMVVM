using MobileDeliveryMVVM.Command;
using System;
using System.Collections.Generic;
using MobileDeliveryGeneral.Definitions;
using MobileDeliveryGeneral.Interfaces.DataInterfaces;
using MobileDeliveryGeneral.Settings;
using MobileDeliverySettings;

namespace MobileDeliveryMVVM.BaseClasses
{
    public class BaseViewModel<D> : ViewModelBase<D> where D : IMDMMessage
    {
        #region Fields  
        protected Dictionary<Guid, Request> dRequests = new Dictionary<Guid, Request>();

        public BaseViewModel(UMDAppConfig config) : base(config)
        {
            //Settings.Current.LogLevel = config.LogLevel.ToString();
            //Settings.Current.UMDPort = config.srvSet.srvport;
            //Settings.Current.UMDUrl = config.srvSet.srvurl;
            //Settings.Current.Url = config.srvSet.url;
            //Settings.Current.Port = config.srvSet.port;
            //Settings.Current.WinsysPort = config.srvSet.clientport;
            //Settings.Current.WinsysUrl = config.srvSet.clienturl;
        }

        private DelegateCommand _refreshCommand;
        private DelegateCommand _cleanupCommand;
        #endregion

        #region Properties  


        public DelegateCommand RefreshCommand
        {
            get
            {
                return _refreshCommand ?? (_refreshCommand = new DelegateCommand(this.Refresh));
            }
        }

        public DelegateCommand CleanupCommand
        {
            get
            {
                return _cleanupCommand ?? (_cleanupCommand = new DelegateCommand(this.Clear));
            }
        }

        #endregion

        #region Methods  

        //Action<>
        public override void Refresh(object obj)
        {
            base.Refresh(obj);
        }
        public override void Clear(object obj)
        {
            base.Clear(obj);
        }

        #endregion
    }
}