using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVP_Voltage.Model;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MVP_Voltage.Util
{
    class CommonCanvasViewModel : ObservableObject
    {
        #region 프로퍼티
        private System.Windows.Point _lastMousePosition;
        private System.Windows.Point ScrolledPosition;
        private System.Windows.Point CurrentPosition;
        private bool IsDragWithRightButton = false;
        private double StartHorizonScrollOffset = 0;
        private double StartVerticalScrollOffset = 0;

        private System.Windows.Point InformationMoveStartPoint = new System.Windows.Point(0, 0);
        private System.Windows.Point CurrentInformationPoint = new System.Windows.Point(0, 0);
        public Mat LastMat
        {
            get { return _lastMat; }
            set { _lastMat = value; OnPropertyChanged("LastMat"); }
        }
        private Mat _lastMat = new Mat();
        public double ImageOpacity
        {
            get { return _imageOpacity; }
            set { _imageOpacity = value; OnPropertyChanged("ImageOpacity"); }
        }
        private double _imageOpacity = 0.5;
        public double MouseXonCanvas
        {
            get { return _mouseXonCanvas; }
            set { _mouseXonCanvas = value; OnPropertyChanged("MouseXonCanvas"); }
        }
        private double _mouseXonCanvas = 0;
        public double MouseYonCanvas
        {
            get { return _mouseYonCanvas; }
            set { _mouseYonCanvas = value; OnPropertyChanged("MouseYonCanvas"); }
        }
        private double _mouseYonCanvas = 0;

        public SolidColorBrush ColorBrush
        {
            get { return _colorBrush; }
            set { _colorBrush = value; OnPropertyChanged("ColorBrush"); }
        }
        private SolidColorBrush _colorBrush = new SolidColorBrush();
        public double RValue
        {
            get { return _rValue; }
            set { _rValue = value; RBrush.Color = System.Windows.Media.Color.FromRgb((byte)RValue, 0, 0); OnPropertyChanged("RValue"); }
        }
        private double _rValue = 0;
        public SolidColorBrush RBrush
        {
            get { return _rBrush; }
            set { _rBrush = value; OnPropertyChanged("RBrush"); }
        }
        private SolidColorBrush _rBrush = new SolidColorBrush();
        public double GValue
        {
            get { return _gValue; }
            set { _gValue = value; GBrush.Color = System.Windows.Media.Color.FromRgb(0, (byte)GValue, 0); OnPropertyChanged("GValue"); }
        }
        private double _gValue = 0;
        public SolidColorBrush GBrush
        {
            get { return _gBrush; }
            set { _gBrush = value; OnPropertyChanged("GBrush"); }
        }
        private SolidColorBrush _gBrush = new SolidColorBrush();
        public double BValue
        {
            get { return _bValue; }
            set { _bValue = value; BBrush.Color = System.Windows.Media.Color.FromRgb(0, 0, (byte)BValue); OnPropertyChanged("BValue"); }
        }
        private double _bValue = 0;
        public SolidColorBrush BBrush
        {
            get { return _bBrush; }
            set { _bBrush = value; OnPropertyChanged("BBrush"); }
        }
        private SolidColorBrush _bBrush = new SolidColorBrush();
        public double AValue
        {
            get { return _aValue; }
            set { _aValue = value; AValuePercent = (AValue / 255); ABrush.Color = System.Windows.Media.Color.FromRgb((byte)RValue, (byte)GValue, (byte)BValue); OnPropertyChanged("AValue"); }
        }
        private double _aValue = 0;
        public SolidColorBrush ABrush
        {
            get { return _aBrush; }
            set { _aBrush = value; OnPropertyChanged("ABrush"); }
        }
        private SolidColorBrush _aBrush = new SolidColorBrush();
        public double AValuePercent
        {
            get { return _aValuePercent; }
            set { _aValuePercent = value; OnPropertyChanged("AValuePercent"); }
        }
        private double _aValuePercent = 0;

        public bool ImageVisibilityBool
        {
            get { return _imageVisibilityBool; }
            set { _imageVisibilityBool = value; OnPropertyChanged("ImageVisibilityBool"); }
        }
        private bool _imageVisibilityBool = true;

        public Canvas CanvasInfo
        {
            get { return _CanvasInfo; }
            set { _CanvasInfo = value; OnPropertyChanged("CanvasInfo"); }
        }
        private Canvas _CanvasInfo;
        public ImageControlModel ImageShow
        {
            get { return _imageShow; }
            set { _imageShow = value; OnPropertyChanged("ImageShow"); }
        }
        private ImageControlModel _imageShow = new ImageControlModel();
        public KeyEventUtil ImageConvertViewModelKeyEvent
        {
            get { return _imageConvertViewModelKeyEvent; }
            set { _imageConvertViewModelKeyEvent = value; OnPropertyChanged("ImageConvertViewModelKeyEvent"); }
        }
        private KeyEventUtil _imageConvertViewModelKeyEvent = new KeyEventUtil();


        #region ---［ ROI ］---------------------------------------------------------------------
        private ROIModel? _roi;
        public ROIModel? Roi
        {
            get => _roi;
            set => SetProperty(ref _roi, value);
        }

        private bool _isRoiVisible;
        public bool IsRoiVisible
        {
            get => _isRoiVisible;
            set => SetProperty(ref _isRoiVisible, value);
        }

        private int _roiHandleSize = 12;
        public int RoiHandleSize
        {
            get => _roiHandleSize;
            set => SetProperty(ref _roiHandleSize, value);
        }

        private double _roiDimOpacity = 0.3;
        public double RoiDimOpacity
        {
            get => _roiDimOpacity;
            set => SetProperty(ref _roiDimOpacity, value);
        }

        #endregion ---------------------------------------------------------------------------------



        #endregion
        #region 커맨드
        public Action<RoutedEventArgs> OnLoadedExtended { get; set; }
        public RelayCommand<RoutedEventArgs> CommandLoaded { get; private set; }
        public RelayCommand<MouseWheelEventArgs> CanvasEventMouseWheel { get; private set; }
        public Action<System.Windows.Input.MouseEventArgs> OnCanvasEventPreviewMouseMoveExtended { get; set; }
        public RelayCommand<System.Windows.Input.MouseEventArgs> CanvasEventPreviewMouseMove { get; private set; }
        public Action<MouseButtonEventArgs> OnCanvasEventPreviewMouseDownExtended { get; set; }
        public RelayCommand<MouseButtonEventArgs> CanvasEventPreviewMouseDown { get; private set; }
        public Action<MouseButtonEventArgs> OnCanvasEventPreviewMouseUpExtended { get; set; }
        public RelayCommand<MouseButtonEventArgs> CanvasEventPreviewMouseUp { get; private set; }
        public RelayCommand<MouseButtonEventArgs> CommandInformationBoardMouseDown { get; private set; }
        public RelayCommand<MouseButtonEventArgs> CommandInformationBoardMouseUp { get; private set; }
        public RelayCommand<System.Windows.Input.MouseEventArgs> CommandInformationBoardMouseMove { get; private set; }

        #endregion

        #region 초기화
        public CommonCanvasViewModel()
        {
            InitData();
            InitCommand();
            InitEvent();
        }

        void InitData()
        {
            RenderOptions.SetBitmapScalingMode(ImageShow.Image, BitmapScalingMode.HighQuality);
            RenderOptions.SetBitmapScalingMode(ImageShow.ImageBrush, BitmapScalingMode.HighQuality);
        }

        void InitCommand()
        {
            CommandLoaded = new RelayCommand<RoutedEventArgs>((e) => OnCommandLoaded(e));
            CanvasEventMouseWheel = new RelayCommand<MouseWheelEventArgs>((e) => OnCanvasEventMouseWheel(e));
            CanvasEventPreviewMouseMove = new RelayCommand<System.Windows.Input.MouseEventArgs>((e) => OnCanvasEventPreviewMouseMove(e));
            CanvasEventPreviewMouseDown = new RelayCommand<MouseButtonEventArgs>((e) => OnCanvasEventPreviewMouseDown(e));
            CanvasEventPreviewMouseUp = new RelayCommand<MouseButtonEventArgs>((e) => OnCanvasEventPreviewMouseUp(e));
            CommandInformationBoardMouseDown = new RelayCommand<MouseButtonEventArgs>((e) => OnCommandInformationBoardMouseDown(e));
            CommandInformationBoardMouseUp = new RelayCommand<MouseButtonEventArgs>((e) => OnCommandInformationBoardMouseUp(e));
            CommandInformationBoardMouseMove = new RelayCommand<System.Windows.Input.MouseEventArgs>((e) => OnCommandInformationBoardMouseMove(e));
        }



        void InitEvent()
        {

        }
        #endregion

        #region 이벤트
        private void OnCommandLoaded(RoutedEventArgs e)
        {
            if (e.OriginalSource.GetType().Name == "Canvas")
            {
                if (CanvasInfo != null)
                {
                    CanvasInfo = null;
                    CanvasInfo = e.OriginalSource as Canvas;
                    return;
                }
                CanvasInfo = e.OriginalSource as Canvas;
            }
            OnLoadedExtended?.Invoke(e);
        }
        protected virtual void OnCanvasEventPreviewMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && ImageConvertViewModelKeyEvent.IsLeftCtrlDown)
            {
                if (IsDragWithRightButton)
                {
                    ScrolledPosition = e.GetPosition(null);
                    (CanvasInfo.Parent as ScrollViewer).ScrollToHorizontalOffset(StartHorizonScrollOffset - ScrolledPosition.X + _lastMousePosition.X);
                    (CanvasInfo.Parent as ScrollViewer).ScrollToVerticalOffset(StartVerticalScrollOffset - ScrolledPosition.Y + _lastMousePosition.Y);
                    return;
                }

            }

            CurrentPosition = e.GetPosition(CanvasInfo);
            MouseXonCanvas = CurrentPosition.X;
            MouseYonCanvas = CurrentPosition.Y;

            if (LastMat != null)
            {
                CurrentPosition = e.GetPosition(CanvasInfo);
                if (CurrentPosition.X < LastMat.Width && CurrentPosition.Y < LastMat.Height && CurrentPosition.X > 0 && CurrentPosition.Y > 0)
                {

                    if (LastMat.Type() == MatType.CV_8UC1)
                    {
                        var colorValue = LastMat.At<byte>((int)MouseYonCanvas, (int)MouseXonCanvas);
                        RValue = GValue = BValue = colorValue;
                        AValue = 255;
                        ColorBrush.Color = System.Windows.Media.Color.FromArgb((byte)AValue, (byte)RValue, (byte)GValue, (byte)BValue);
                    }
                    else if (LastMat.Type() == MatType.CV_8UC3)
                    {
                        var colorValue = LastMat.At<Vec3b>((int)MouseYonCanvas, (int)MouseXonCanvas);
                        RValue = colorValue.Item2;
                        GValue = colorValue.Item1;
                        BValue = colorValue.Item0;
                        AValue = 255;
                        ColorBrush.Color = System.Windows.Media.Color.FromArgb((byte)AValue, (byte)RValue, (byte)GValue, (byte)BValue);
                    }
                    else if (LastMat.Type() == MatType.CV_8UC4)
                    {
                        var colorValue = LastMat.At<Vec4b>((int)MouseYonCanvas, (int)MouseXonCanvas);
                        RValue = colorValue.Item2;
                        GValue = colorValue.Item1;
                        BValue = colorValue.Item0;
                        AValue = colorValue.Item3;
                        ColorBrush.Color = System.Windows.Media.Color.FromArgb((byte)AValue, (byte)RValue, (byte)GValue, (byte)BValue);
                    }
                    else
                    {

                    }


                }
            }
            OnCanvasEventPreviewMouseMoveExtended?.Invoke(e);
        }
        public void OnCanvasEventPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (LastMat == null)
            {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed && ImageConvertViewModelKeyEvent.IsLeftCtrlDown)
            {
                StartHorizonScrollOffset = (CanvasInfo.Parent as ScrollViewer).HorizontalOffset;
                StartVerticalScrollOffset = (CanvasInfo.Parent as ScrollViewer).VerticalOffset;
                _lastMousePosition = e.GetPosition(null);
                IsDragWithRightButton = true;
                CanvasInfo.CaptureMouse();
                return;
            }
            OnCanvasEventPreviewMouseDownExtended?.Invoke(e);
        }
        private void OnCanvasEventPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (IsDragWithRightButton)
            {
                IsDragWithRightButton = false;
                CanvasInfo.ReleaseMouseCapture();
                return;
            }
            OnCanvasEventPreviewMouseUpExtended?.Invoke(e);
        }
        private void OnCanvasEventMouseWheel(MouseWheelEventArgs param)
        {
            //if (ImageShow.Image.Source == null)
            //{
            //    return;
            //}
            if (LastMat == null)
            {
                return;
            }

            if (ImageConvertViewModelKeyEvent.IsLeftCtrlDown)
            {

                if (param.Delta > 0)
                {

                    ImageShow.ImageScaleX *= 1.1;
                    ImageShow.ImageScaleY *= 1.1;
                }
                else if (param.Delta < 0)
                {
                    ImageShow.ImageScaleX /= 1.1;
                    ImageShow.ImageScaleY /= 1.1;
                }
                ImageShow.ImageSourceUpdate(LastMat, "Image");
            }
            
        }

        public void UpdateImage(string ImagePath, bool IsBackground)
        {
            if (ImagePath == "")
            {
                LastMat = new Mat();
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ImageShow.Image.Source = null;
                    ImageShow.ImageBrush.ImageSource = null;
                });
                return;
            }

            if (File.Exists(ImagePath)) { LastMat = new Mat(ImagePath, ImreadModes.Unchanged); } else { return; }
            if (IsBackground)
            {
                ImageShow.ImageSourceUpdate(LastMat, "ImageBrush");
            }
            else
            {
                ImageShow.ImageSourceUpdate(LastMat, "Image");
            }
        }
        public void UpdateImage(Mat MatImage, bool IsBackground)
        {
            if (MatImage == null)
            {
                LastMat = new Mat();
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ImageShow.Image.Source = null;
                    ImageShow.ImageBrush.ImageSource = null;
                });
                return;
            }

            if (IsBackground)
            {
                LastMat = MatImage.Clone();
                ImageShow.ImageSourceUpdate(LastMat, "ImageBrush");
            }
            else
            {
                ImageShow.ImageSourceUpdate(LastMat, "Image");
            }
        }
        private void OnCommandInformationBoardMouseUp(MouseButtonEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(Border))
            {
                (e.OriginalSource as Border).ReleaseMouseCapture();
            }
        }

        private void OnCommandInformationBoardMouseDown(MouseButtonEventArgs e)
        {
            Border tempBorder = null;
            if (e.OriginalSource.GetType() == typeof(StackPanel))
            {
                if ((e.OriginalSource as StackPanel).Parent.GetType() == typeof(Border))
                {
                    tempBorder = ((e.OriginalSource as StackPanel).Parent as Border);
                }
            }

            if (e.OriginalSource.GetType() == typeof(Border))
            {
                tempBorder = (e.OriginalSource as Border);


            }

            if (tempBorder != null)
            {
                InformationMoveStartPoint = e.GetPosition(null);
                CurrentInformationPoint.X = tempBorder.Margin.Left;
                CurrentInformationPoint.Y = tempBorder.Margin.Top;
                tempBorder.CaptureMouse();
            }

        }

        private void OnCommandInformationBoardMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(Border))
            {
                if ((e.OriginalSource as Border).IsMouseCaptured)
                {

                    double deltaX = e.GetPosition(null).X - InformationMoveStartPoint.X + CurrentInformationPoint.X;
                    double deltaY = e.GetPosition(null).Y - InformationMoveStartPoint.Y + CurrentInformationPoint.Y;

                    if (deltaX < 0)
                    {
                        deltaX = 0;
                    }
                    if (deltaY < 0)
                    {
                        deltaY = 0;
                    }
                    if (deltaX > ((e.OriginalSource as Border).Parent as Grid).ActualWidth - (e.OriginalSource as Border).ActualWidth - 15)
                    {
                        deltaX = (((e.OriginalSource as Border).Parent as Grid).ActualWidth) - (e.OriginalSource as Border).ActualWidth - 15;
                    }
                    if (deltaY > ((e.OriginalSource as Border).Parent as Grid).ActualHeight - (e.OriginalSource as Border).ActualHeight - 15)
                    {
                        deltaY = (((e.OriginalSource as Border).Parent as Grid).ActualHeight) - (e.OriginalSource as Border).ActualHeight - 15;
                    }

                    (e.OriginalSource as Border).Margin = new Thickness(deltaX, deltaY, 0, 0);
                    InformationMoveStartPoint = e.GetPosition(null);
                    CurrentInformationPoint.X = (e.OriginalSource as Border).Margin.Left;
                    CurrentInformationPoint.Y = (e.OriginalSource as Border).Margin.Top;
                }
            }
        }
        #endregion
    }
}
