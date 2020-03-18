namespace MobileDeliveryMVVM.Command
{
    using System.Windows.Input;

    public static class CommandRouter
    {
        static CommandRouter()
        {
            IncrementCounter = new DelegateCommand();
            DecrementCounter = new DelegateCommand();
        }

        public static DelegateCommand IncrementCounter { get; private set; }
        public static DelegateCommand DecrementCounter { get; private set; }

        public static void WireMainView(MainView view, MainViewModel viewModel)
        {
            if (view == null || viewModel == null) return;

            view.CommandBindings.Add(
                new DelegateCommand(
                    IncrementCounter,
                    (λ1, λ2) => viewModel.IncrementCounter(),
                    (λ1, λ2) =>
                    {
                        λ2.CanExecute = true;
                        λ2.Handled = true;
                    }));
            view.CommandBindings.Add(
                new CommandBinding(
                    DecrementCounter,
                    (λ1, λ2) => viewModel.DecrementCounter(),
                    (λ1, λ2) =>
                    {
                        λ2.CanExecute = true;
                        λ2.Handled = true;
                    }));
        }
    }
}