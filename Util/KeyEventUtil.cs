using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MVP_Voltage.Util
{
    class KeyEventUtil
    {
        public RelayCommand<KeyEventArgs> EventKeyDown { get; set; }
        public RelayCommand<KeyEventArgs> EventKeyUp { get; set; }
        public bool IsLeftCtrlDown { get; private set; }
        public bool IsLeftShiftDonw { get; private set; }
        public KeyEventUtil()
        {
            EventKeyDown = new RelayCommand<KeyEventArgs>(EventKeyDownOnImage);
            EventKeyUp = new RelayCommand<KeyEventArgs>(EventKeyUpOnImage);
        }

        #region ---［ 키보드이벤트 ］---------------------------------------------------------------------

        public void EventKeyDownOnImage(KeyEventArgs obj)
        {
            if (Key.LeftCtrl == obj.Key)
            {
                IsLeftCtrlDown = true;
            }
            else if (Key.LeftShift == obj.Key)
            {
                IsLeftShiftDonw = true;
            }
            else
            { }
        }

        public void EventKeyUpOnImage(KeyEventArgs obj)
        {
            IsLeftShiftDonw = false;
            IsLeftCtrlDown = false;
        }
        #endregion ---------------------------------------------------------------------------------
    }
}
