using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CPUDoc
{
    // Because this view model wants to be explicitly notified by the dialog when it closes,
    // it implements the optional IOkDialogViewModel interface
    public class RebootAppViewModel :
      IOkDialogViewModel,
      INotifyPropertyChanged
      //INotifyDataErrorInfo
    {
        // GivenReason binds to a TextBox in the dialog's DataTemplate. (that targets RebootAppViewModel)
        private string givenReason;
        public string GivenReason
        {
            get => this.givenReason;
            set
            {
                this.givenReason = value;
                OnPropertyChanged();
            }
        }

        public string Title => "Reboot App?";
        private DatabaseRepository Repository { get; } = new DatabaseRepository();

        bool IOkDialogViewModel.CanExecuteOkCommand() => true;

        void IOkDialogViewModel.ExecuteOkCommand()
        {
            var givenReason = new Reason() { GivenReason = this.GivenReason };

            OnRebootRequested(givenReason);
        }

        public event EventHandler<RebootAppEventArgs> OnGivenReason;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnRebootRequested(Reason givenReason)
          => this.OnGivenReason?.Invoke(this, new RebootAppEventArgs(givenReason));

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
          => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
