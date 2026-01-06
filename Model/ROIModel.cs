using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVP_Voltage.Model
{
    public sealed class ROIModel : ObservableObject
    {
        #region ---［ SettingROI ］---------------------------------------------------------------------

        public int Width
        {
            get { return _width; }
            set { _width = value; OnPropertyChanged("Width"); OnPropertyChanged("Right"); OnPropertyChanged("MarginRight"); }
        }
        private int _width = 100;
        public int Height
        {
            get { return _height; }
            set { _height = value; OnPropertyChanged("Height"); OnPropertyChanged("Bottom"); OnPropertyChanged("MarginBottom"); }
        }
        private int _height = 100;

        public int X
        {
            get { return _x; }
            set { _x = value; OnPropertyChanged("X"); OnPropertyChanged("Right"); OnPropertyChanged("MarginRight"); }
        }
        private int _x = 0;

        public int Y
        {
            get { return _y; }
            set { _y = value; OnPropertyChanged("Y"); OnPropertyChanged("Bottom"); OnPropertyChanged("MarginBottom"); }
        }
        private int _y = 0;


        public int Right => X + Width;
        public int Bottom => Y + Height;
        

        #endregion ---------------------------------------------------------------------------------
    }
}
