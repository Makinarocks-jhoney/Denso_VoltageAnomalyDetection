using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;


namespace MVP_Voltage.Model
{
    class FrameItem
    {
        public int Index { get; set; }
        public BitmapSource Thumbnail { get; set; } = default!;
        public string Label => $"Frame {Index}";
        public string Result { get; set; } = "";
        public double Voltage { get; set; }
        public bool IsDetected { get; set; } = false;
    }
    class DetectedFrameItem : FrameItem
    {
        public ObservableCollection<FrameItem> Frames { get; set; } = new ObservableCollection<FrameItem>();
    }
}
