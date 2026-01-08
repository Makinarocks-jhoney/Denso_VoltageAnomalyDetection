using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVP_Voltage.View
{
    class ChildViewModelBase:ViewModelBase
    {
        public MainWindowViewModel _mainWindowViewModel { get; }
        public ChildViewModelBase(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }
    }
}
