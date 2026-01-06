using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MVP_Voltage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private bool _allowClose = false;
        public MainWindow()
        {
            InitializeComponent();
            this.Closing += OnClosingAsync;
        }
        private async void OnClosingAsync(object? sender, CancelEventArgs e)
        {
            if (_allowClose) { return; }

            if (DataContext is MainWindowViewModel vm)
            {
                // 닫기 시도를 잠시 보류
                e.Cancel = true;

                bool canClose = await vm.TrySaveAndCloseAsync();
                if (canClose)
                {
                    _allowClose = true;
                    // 핸들러를 잠시 떼고 실제로 닫기 (무한루프 방지)
                    this.Closing -= OnClosingAsync;
                    await Dispatcher.BeginInvoke(new Action(() => {
                        this.Close();
                    }), System.Windows.Threading.DispatcherPriority.Background);

                }
                // canClose == false면 그냥 머무름 (취소)
            }
        }
    }
}