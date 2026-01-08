using MVP_Voltage.Model;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MVP_Voltage.Util
{
    /// <summary>
    /// CommonCanvasView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CommonCanvasView : UserControl
    {
        public CommonCanvasView()
        {
            InitializeComponent();
        }
        public ROIModel Roi
        {
            get => (ROIModel)GetValue(RoiProperty);
            set => SetValue(RoiProperty, value);
        }

        public static readonly DependencyProperty RoiProperty =
            DependencyProperty.Register(nameof(Roi), typeof(ROIModel), typeof(CommonCanvasView),
                new PropertyMetadata(null));

        public bool IsRoiVisible
        {
            get => (bool)GetValue(IsRoiVisibleProperty);
            set => SetValue(IsRoiVisibleProperty, value);
        }

        public static readonly DependencyProperty IsRoiVisibleProperty =
            DependencyProperty.Register(nameof(IsRoiVisible), typeof(bool), typeof(CommonCanvasView),
                new PropertyMetadata(false));

        public int RoiHandleSize
        {
            get => (int)GetValue(RoiHandleSizeProperty);
            set => SetValue(RoiHandleSizeProperty, value);
        }

        public static readonly DependencyProperty RoiHandleSizeProperty =
            DependencyProperty.Register(nameof(RoiHandleSize), typeof(int), typeof(CommonCanvasView),
                new PropertyMetadata(12));

        public double RoiDimOpacity
        {
            get => (double)GetValue(RoiDimOpacityProperty);
            set => SetValue(RoiDimOpacityProperty, value);
        }

        public static readonly DependencyProperty RoiDimOpacityProperty =
            DependencyProperty.Register(nameof(RoiDimOpacity), typeof(double), typeof(CommonCanvasView),
                new PropertyMetadata(0.3));
    }
}
