using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUDoc
{
    interface IOkDialogViewModel : INotifyPropertyChanged
    {
        // The title of the dialog
        string Title { get; }

        // Use this to validate the current view model state/data.
        // Return 'false' to disable the "Ok" button.
        // This method is invoked by the OkDialog before executing the OkCommand.
        bool CanExecuteOkCommand();

        // Called after the dialog was successfully closed
        void ExecuteOkCommand();
    }
}
