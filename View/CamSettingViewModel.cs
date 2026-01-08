using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvCameraControl;
using MVP_Voltage.Model;
using MVP_Voltage.Services;
using MVP_Voltage.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MVP_Voltage.View
{
    class CamSettingViewModel:ChildViewModelBase
    {

        #region 프로퍼티
        #region ---［ Cam1 ］---------------------------------------------------------------------
        private readonly object _renderLoopLock = new();
        private DispatcherTimer? _renderTimer;
        private int _activeGrabCount = 0;
        public HikCamera Cam1 { get; } = new HikCamera();
        private RawFrame? _latestCam1;

        
        public CommonCanvasViewModel Cam1ImageControl
        {
            get { return _cam1ImageControl; }
            set { _cam1ImageControl = value; OnPropertyChanged("Cam1ImageControl"); }
        }
        private CommonCanvasViewModel _cam1ImageControl = new CommonCanvasViewModel();
        public bool ConnectedCam1
        {
            get { return _connectedCam1; }
            set { _connectedCam1 = value; OnPropertyChanged("ConnectedCam1"); }
        }
        private bool _connectedCam1 = false;

        public ObservableCollection<IDeviceInfo> CamList
        {
            get { return _camList; }
            set { _camList = value; OnPropertyChanged("CamList"); }
        }
        private ObservableCollection<IDeviceInfo> _camList = new ObservableCollection<IDeviceInfo>();

        public IDeviceInfo SelectedCam1
        {
            get { return _selectedCam1; }
            set { _selectedCam1 = value; OnPropertyChanged("SelectedCam1"); }
        }
        private IDeviceInfo _selectedCam1;
        #endregion ---------------------------------------------------------------------------------

        #region ---［ Cam2 ］---------------------------------------------------------------------
        public HikCamera Cam2 { get; } = new HikCamera();
        private RawFrame? _latestCam2;
        public CommonCanvasViewModel Cam2ImageControl
        {
            get { return _cam2ImageControl; }
            set { _cam2ImageControl = value; OnPropertyChanged("Cam2ImageControl"); }
        }
        private CommonCanvasViewModel _cam2ImageControl = new CommonCanvasViewModel();
        public bool ConnectedCam2
        {
            get { return _connectedCam2; }
            set { _connectedCam2 = value; OnPropertyChanged("ConnectedCam2"); }
        }
        private bool _connectedCam2 = false;

        public IDeviceInfo SelectedCam2
        {
            get { return _selectedCam2; }
            set { _selectedCam2 = value; OnPropertyChanged("SelectedCam2"); }
        }
        private IDeviceInfo _selectedCam2;
        #endregion ---------------------------------------------------------------------------------
        #endregion
        #region 커맨드
        public RelayCommand<object> CommandRefreshCamList { get; private set; }

        public RelayCommand<object> CommandConnectCam { get; private set; }
        public RelayCommand<object> CommandDisConnectCam { get; private set; }

        public RelayCommand<object> CommandSnapCam { get; private set; }
        public RelayCommand<object> CommandGrabCam { get; private set; }

        public RelayCommand<object> CommandCalcBrightness { get; private set; }
        #endregion

        #region 초기화
        public CamSettingViewModel(MainWindowViewModel mainWindowViewModel) :base(mainWindowViewModel)
        {
            InitData();
            InitCommand();
            InitEvent();
        }

        void InitData()
        {

        }

        void InitCommand()
        {
            CommandRefreshCamList = new RelayCommand<object>((e) => OnCommandRefreshCamList(e));

            CommandConnectCam = new RelayCommand<object>((e) => OnCommandConnectCam(e));
            CommandDisConnectCam = new RelayCommand<object>((e) => OnCommandDisConnectCam(e));
            CommandSnapCam = new RelayCommand<object>((e) => OnCommandSnapCam(e));
            CommandGrabCam = new RelayCommand<object>((e) => OnCommandGrabCam(e));

            CommandCalcBrightness = new RelayCommand<object>((e) => OnCommandCalcBrightness(e));
        }

        void InitEvent()
        {

        }
        #endregion

        #region 이벤트
        private async Task OnCommandRefreshCamList(object? e)
        {
            CamList = new ObservableCollection<IDeviceInfo>(Cam1.GetDevices());
        }
        private void OnCamGrabStarted()
        {
            // 0 -> 1 로 바뀌는 순간에만 타이머를 켬
            if (Interlocked.Increment(ref _activeGrabCount) == 1)
                Application.Current.Dispatcher.Invoke(EnsureRenderLoopStarted);
        }
        private void OnCamGrabStopped()
        {
            // 1 -> 0 로 바뀌는 순간에만 타이머를 끔
            if (Interlocked.Decrement(ref _activeGrabCount) == 0)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // 최신 프레임 남아있으면 날려서 메모리/풀 반납까지 정리(옵션)
                    Interlocked.Exchange(ref _latestCam1, null)?.Dispose(); // IDisposable로 설계했다면
                    Interlocked.Exchange(ref _latestCam2, null)?.Dispose();

                    EnsureRenderLoopStopped();
                });
        }
        private void OnCommandConnectCam(object? e)
        {
            switch (e.ToString())
            {
                case "Cam1":
                    try
                    {
                        Cam1.ConnectCamera(SelectedCam1);
                        ConnectedCam1 = Cam1.IsConnected;
                        if (ConnectedCam1 == false)
                        {
                            _mainWindowViewModel.AddLog("Error : Cam1 Connection Fail");
                        }
                        else
                        {
                            _mainWindowViewModel.AddLog("Connected Cam1");
                        }
                    }
                    catch (Exception)
                    {

                        _mainWindowViewModel.AddLog("Error : Cam1 Connection Fail");
                    }

                    break;
                case "Cam2":
                    Cam2.ConnectCamera(SelectedCam2);
                    ConnectedCam2 = Cam2.IsConnected;
                    if (ConnectedCam2 == false)
                    {
                        _mainWindowViewModel.AddLog("Error : Cam2 Connection Fail");
                    }
                    else
                    {
                        _mainWindowViewModel.AddLog("Connected Cam2");
                    }
                    break;
            }
        }
        private async Task OnCommandDisConnectCam(object? e)
        {
            switch (e.ToString())
            {
                case "Cam1":
                    Cam1.DisconnectCamera();
                    ConnectedCam1 = Cam1.IsConnected;
                    OnCamGrabStopped();
                    break;
                case "Cam2":
                    Cam2.DisconnectCamera();
                    ConnectedCam2 = Cam2.IsConnected;
                    OnCamGrabStopped();
                    break;
            }

        }

        private async Task OnCommandSnapCam(object? e)
        {
            switch (e.ToString())
            {
                case "Cam1":
                    var test1 = Cam1.GetFrame(100);
                    Cam1ImageControl.UpdateImage(test1, false);
                    break;
                case "Cam2":
                    var test2 = Cam2.GetFrame(100);
                    Cam2ImageControl.UpdateImage(test2, false);
                    break;
            }

        }
        private void OnCommandGrabCam(object? e)
        {
            switch (e.ToString())
            {
                case "Cam1":
                    Cam1.FrameArrived -= Cam1_FrameArrived;
                    Cam1.FrameArrived += Cam1_FrameArrived;
                    var test1 = Cam1.GetFrame(1000);
                    if (test1.Width == 0 || test1.Height == 0)
                    {
                        _mainWindowViewModel._dialogCoordinator.ShowMessageAsync(this, "Error", "Camera Grab Failed");
                        return;
                    }
                    Cam1ImageControl.ImageShow.InitializeSource(test1.Width, test1.Height, test1.Channels());
                    Cam1.StartGrabbing();
                    OnCamGrabStarted();
                    break;
                case "Cam2":
                    Cam2.FrameArrived -= Cam2_FrameArrived;
                    Cam2.FrameArrived += Cam2_FrameArrived;
                    var test2 = Cam2.GetFrame(1000);
                    if (test2.Width == 0 || test2.Height == 0)
                    {
                        _mainWindowViewModel._dialogCoordinator.ShowMessageAsync(this, "Error", "Camera Grab Failed");
                        return;
                    }
                    Cam2ImageControl.ImageShow.InitializeSource(test2.Width, test2.Height, test2.Channels());
                    Cam2.StartGrabbing();
                    OnCamGrabStarted();
                    break;
            }
        }
        private void Cam1_FrameArrived(object? sender, RawFrame rawFrame)
        {
            var old = Interlocked.Exchange(ref _latestCam1, rawFrame);
            old?.Dispose();
            //Application.Current.Dispatcher.BeginInvoke(() => { Cam1ImageControl.UpdateFromRawFrame(rawFrame); });


        }
        private void Cam2_FrameArrived(object? sender, RawFrame rawFrame)
        {
            var old = Interlocked.Exchange(ref _latestCam2, rawFrame);
            old?.Dispose();
            //Application.Current.Dispatcher.BeginInvoke(() => { Cam2ImageControl.UpdateFromRawFrame(rawFrame); });


        }

        private void OnCommandCalcBrightness(object? e)
        {
            //var roiRect = new OpenCvSharp.Rect(Settings.MasterROI.X, Settings.MasterROI.Y, Settings.MasterROI.Width, Settings.MasterROI.Height);
            //Mat target = CamInformation.GetFrameImage(SelectedFrame);
            //Cv2.CvtColor(target, target, ColorConversionCodes.BGR2GRAY);
            //using (var roi = new Mat(target, roiRect))
            //{
            //    var mean = Cv2.Mean(roi);
            //    Brightness = mean.Val0;
            //}
            //target.Dispose();
        }
        private void EnsureRenderLoopStarted()
        {
            lock (_renderLoopLock)
            {
                if (_renderTimer != null) return;

                _renderTimer = new DispatcherTimer(DispatcherPriority.Render)
                {
                    Interval = TimeSpan.FromMilliseconds(33) // 30fps
                };
                _renderTimer.Tick += RenderTimer_Tick;
                _renderTimer.Start();
            }
        }

        private void EnsureRenderLoopStopped()
        {
            lock (_renderLoopLock)
            {
                if (_renderTimer == null) return;

                _renderTimer.Stop();
                _renderTimer.Tick -= RenderTimer_Tick;
                _renderTimer = null;
            }
        }
        private void RenderTimer_Tick(object? sender, EventArgs e)
        {
            // Cam1
            var f1 = Interlocked.Exchange(ref _latestCam1, null);
            if (f1 != null)
            {
                Cam1ImageControl.ImageShow.UpdateFromRawFrame(f1);
                f1.Dispose();
            }

            // Cam2
            var f2 = Interlocked.Exchange(ref _latestCam2, null);
            if (f2 != null)
            {
                Cam2ImageControl.ImageShow.UpdateFromRawFrame(f2);
                f2.Dispose();

            }
        }
        #endregion

    }
}
