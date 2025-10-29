using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUDoc
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public RebootAppViewModel RebootAppViewModel { get; }
        private Dictionary<Type, object> ViewModels { get; }
        
        //this.RebootAppViewModel.= OnRebootApp;
        
        public MainViewModel()
        {

            this.ViewModels = new Dictionary<Type, object>
            {
                { typeof(MainViewModel), this },
                { typeof(RebootAppViewModel), this.RebootAppViewModel },
            };
        }

        public bool TryGetViewModel(Type viewModelType, out object viewModel)
          => this.ViewModels.TryGetValue(viewModelType, out viewModel);

        private void OnRebootApp(object? sender, RebootAppEventArgs e)
        {
            Reason newReboot = e.ReasonGiven;
        }

    }
}
