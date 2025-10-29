using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CPUDoc.Windows
{
    public partial class OkDialog : Window
    {
        public static RoutedCommand OkCommand { get; } = new RoutedCommand("OkCommand", typeof(MainWindow));

        public OkDialog(object contentViewModel)
        {
            InitializeComponent();

            var okCommandBinding = new CommandBinding(OkDialog.OkCommand, ExecuteOkCommand, CanExecuteOkCommand);
            _ = this.CommandBindings.Add(okCommandBinding);

            this.DataContext = contentViewModel;
            this.Content = contentViewModel;

            this.DataContextChanged += OnDataContextChanged;
        }

        // If there is no explicit Content, use the DataContext
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) => this.Content ??= e.NewValue;

        // If the content view model doesn't implement the optional IOkDialogViewModel just enable the command source.
        private void CanExecuteOkCommand(object sender, CanExecuteRoutedEventArgs e)
          => e.CanExecute = (this.Content as IOkDialogViewModel)?.CanExecuteOkCommand() ?? true;

        private void ExecuteOkCommand(object sender, ExecutedRoutedEventArgs e)
          => this.DialogResult = true;
    }
}
