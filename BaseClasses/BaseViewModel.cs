using MobileDeliveryGeneral.Interfaces;
using MobileDeliveryMVVM.Command;

namespace MobileDeliveryMVVM.BaseClasses
{
    public abstract class BaseViewModel<D> : ViewModelDataCommunication
    {
        #region Fields  

        public BaseViewModel() : base(){ }

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
        protected override void Clear(object obj)
        {
            base.Clear(obj);
        }

        #endregion
    }
}