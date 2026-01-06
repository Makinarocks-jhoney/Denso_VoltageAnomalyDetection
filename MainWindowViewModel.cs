using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MVP_Voltage.Model;
using MVP_Voltage.Services;
using MVP_Voltage.Util;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace MVP_Voltage
{
    class MainWindowViewModel:ObservableObject
    {

        #region 프로퍼티
        public Settings Settings { get; } = new Settings();
        public CamInformation CamInformation { get; } = new CamInformation();
        public Detection Detection { get; } = new Detection();
        private CancellationTokenSource? realtimeCts;
        private Task? _voltageTask;
        private Task? _playTask;
        public ImageControlModel ImageControl
        {
            get { return _imageControl; }
            set { _imageControl = value; OnPropertyChanged("ImageControl"); }
        }
        private ImageControlModel _imageControl = new ImageControlModel();

        public ImageControlModel InspectionImageControl
        {
            get { return _inspectionImageControl; }
            set { _inspectionImageControl = value; OnPropertyChanged("InspectionImageControl"); }
        }
        private ImageControlModel _inspectionImageControl = new ImageControlModel();

        public string VideoPath
        {
            get { return CamInformation.VideoPath; }
            set { CamInformation.VideoPath= value; OnPropertyChanged("VideoPath"); }
        }
        public bool IsOpenVideo
        {
            get { return CamInformation.IsOpened; }
            set { CamInformation.IsOpened = value; OnPropertyChanged("IsOpenVideo"); }
        }
        public int MaxFrame
        {
            get { return CamInformation.TotalFrame; }
            set { CamInformation.TotalFrame = value; OnPropertyChanged("MaxFrame"); }
        }
        
        public int SelectedFrame
        {
            get { return _selectedFrame; }
            set { _selectedFrame = value; OnPropertyChanged("SelectedFrame"); }
        }
        private int _selectedFrame = 0;
        public ObservableCollection<DetectedFrameItem> DetectedFrameItem
        {
            get { return _detectedFrameItem; }
            set { _detectedFrameItem = value; OnPropertyChanged("DetectedFrameItem"); }
        }
        private ObservableCollection<DetectedFrameItem> _detectedFrameItem = new ObservableCollection<DetectedFrameItem>();

        public bool IsInspection
        {
            get { return _isInspection; }
            set { _isInspection = value; OnPropertyChanged("IsInspection"); }
        }
        private bool _isInspection = false;

        #region ---［ DetectedROI ］---------------------------------------------------------------------
        public Visibility DetectionROIVisibility
        {
            get { return _detectionROIVisibility; }
            set { _detectionROIVisibility = value; OnPropertyChanged("DetectionROIVisibility"); }
        }
        private Visibility _detectionROIVisibility = Visibility.Collapsed;

        public int ROIWidth
        {
            get { return _rOIWidth; }
            set { _rOIWidth = value; OnPropertyChanged("ROIWidth"); }
        }
        private int _rOIWidth = 0;
        public int ROIHeight
        {
            get { return _rOIHeight; }
            set { _rOIHeight = value; OnPropertyChanged("ROIHeight"); }
        }
        private int _rOIHeight = 0;

        public int ROIX
        {
            get { return _rOIX; }
            set { _rOIX = value; OnPropertyChanged("ROIX"); }
        }
        private int _rOIX = 0;
        public int ROIY
        {
            get { return _rOIY; }
            set { _rOIY = value; OnPropertyChanged("ROIY"); }
        }
        private int _rOIY = 0;
        #endregion ---------------------------------------------------------------------------------

        #region ---［ Voltage ］---------------------------------------------------------------------
        public double StartVolt
        {
            get { return _startVolt; }
            set { _startVolt = value; OnPropertyChanged("StartVolt"); }
        }
        private double _startVolt = 0;

        public double EndVolt
        {
            get { return _endVolt; }
            set { _endVolt = value; OnPropertyChanged("EndVolt"); }
        }
        private double _endVolt = 20;

        public double CurrentVolt
        {
            get { return _currentVolt; }
            set { _currentVolt = value; OnPropertyChanged("CurrentVolt"); }
        }
        private double _currentVolt = 0;

        public double UpdateIntervalMs
        {
            get { return _updateIntervalMs; }
            set { _updateIntervalMs = value; OnPropertyChanged("UpdateIntervalMs"); }
        }
        private double _updateIntervalMs = 100;

        public double VoltStep
        {
            get { return _voltStep; }
            set { _voltStep = value; OnPropertyChanged("VoltStep"); }
        }
        private double _voltStep = 0.1;

        public double SpecInMaxVolt
        {
            get { return _specInMaxVolt; }
            set { _specInMaxVolt = value; OnPropertyChanged("SpecInMaxVolt"); }
        }
        private double _specInMaxVolt = 16;

        public double SpecInMinVolt
        {
            get { return _specInMinVolt; }
            set { _specInMinVolt = value; OnPropertyChanged("SpecInMinVolt"); }
        }
        private double _specInMinVolt = 5;

        public bool IsSaveSpecOut
        {
            get { return _isSaveSpecOut; }
            set { _isSaveSpecOut = value; OnPropertyChanged("IsSaveSpecOut"); }
        }
        private bool _isSaveSpecOut = true;

        public ObservableCollection<ISeries> VoltageGraph { get; set; }
        public Axis[] EpochsAxis
        {
            get { return _epochsAxis; }
            set { _epochsAxis = value; OnPropertyChanged("EpochsAxis"); }
        }
        private Axis[] _epochsAxis = new Axis[] { new Axis { Name = "Time", MinLimit = 0, MinStep=0.001, UnitWidth=0.1, Labeler = value => value.ToString("F2") } };
        public Axis[] YAxes
        {
            get { return _yAxes; }
            set { _yAxes = value; OnPropertyChanged("YAxes"); }
        }
        private Axis[] _yAxes = new Axis[] {
            new Axis { Name = "Voltage", Position=LiveChartsCore.Measure.AxisPosition.Start, NamePaint=new SolidColorPaint(SKColors.Green),
                MinLimit=0, MaxLimit=100, Labeler=value=>value.ToString("F2") }};
        public LineSeries<double> VoltageSeries
        {
            get { return _voltageSeries; }
            set { _voltageSeries = value; OnPropertyChanged("VoltageSeries"); }
        }
        private LineSeries<double> _voltageSeries = new LineSeries<double>()
        {
            Name = "Voltage",
            Stroke = new SolidColorPaint(SKColors.Green),
            GeometrySize = 10,
            Values = new ObservableCollection<double>(),
            ScalesYAt = 0,
        };
        #endregion ---------------------------------------------------------------------------------



        public double ProgressValue
        {
            get { return _progressValue; }
            set { _progressValue = value; OnPropertyChanged("ProgressValue"); }
        }
        private double _progressValue = 0;
        public double ProgressMax
        {
            get { return _progressMax; }
            set { _progressMax = value; OnPropertyChanged("ProgressMax"); }
        }
        private double _progressMax = 0;


        #endregion
        #region 커맨드
        public RelayCommand<object> CommandOpenVideoPath { get; private set; }
        public RelayCommand<object> CommandSelectedFrameChanged { get; private set; }
        public RelayCommand<object> CommandInspect { get; private set; }
        public RelayCommand<object> CommandStop { get; private set; }

        public RelayCommand<RoiDragArgs> RoiDragCommand { get; private set; }
        public RelayCommand<DragDeltaEventArgs> CommandResizeROI { get; private set; }
        
        #endregion

        #region 초기화
        public MainWindowViewModel()
        {
            InitData();
            InitCommand();
            InitEvent();

            VoltageGraph = new ObservableCollection<ISeries>
            {
                VoltageSeries
            };
        }

        void InitData()
        {
            Detection.Initialize("best.onnx");
        }

        void InitCommand()
        {
            CommandOpenVideoPath = new RelayCommand<object>((param) => OnCommandOpenVideoPath(param));
            CommandSelectedFrameChanged = new RelayCommand<object>((e) => OnCommandSelectedFrameChanged(e));
            CommandInspect = new RelayCommand<object>((e) => OnCommandInspect(e));
            CommandStop = new RelayCommand<object>((e) => OnCommandStop(e));

            RoiDragCommand = new RelayCommand<RoiDragArgs>((e) => OnRoiDragCommand(e));
            CommandResizeROI = new RelayCommand<DragDeltaEventArgs>((e) => OnCommandResizeROI(e));
        }

        

        void InitEvent()
        {

        }
        #endregion

        #region 이벤트

        private void OnCommandOpenVideoPath(object param)
        {
            Microsoft.Win32.OpenFileDialog Dialog = new Microsoft.Win32.OpenFileDialog();
            Dialog.DefaultExt = ".txt";
            Dialog.Filter = "MP4(*.mp4)|*.mp4|All Files (*.*)|*.*";
            bool? result = Dialog.ShowDialog();

            if (result == true)
            {
                VideoPath = Dialog.FileName;
                CamInformation.Open(VideoPath);
                IsOpenVideo = CamInformation.IsOpened;
                MaxFrame = CamInformation.TotalFrame;
                SelectedFrame = 0;
                ImageControl.ImageSourceUpdate(CamInformation.GetFrameImage(SelectedFrame), "Image");

                InspectionImageControl.ImageSourceUpdate(CamInformation.GetFrameImage(0), "Image");
            }
        }
        private void OnCommandSelectedFrameChanged(object? e)
        {
            ImageControl.ImageSourceUpdate(CamInformation.GetFrameImage(SelectedFrame),"Image");
        }
        private async Task OnCommandInspect(object? e)
        {
            ProgressValue = 0;
            ProgressMax = CamInformation.TotalFrame;
            IsInspection = true;
            YAxes[0].MinLimit = StartVolt;
            YAxes[0].MaxLimit = EndVolt;
            VoltageSeries.Values = new ObservableCollection<double>();
            DetectedFrameItem.Clear();
            realtimeCts?.Cancel();
            realtimeCts?.Dispose();
            realtimeCts = new CancellationTokenSource();
            try
            {
                //_voltageTask = GetVoltageRealtimeAsync(realtimeCts.Token);
                //_playTask = PlayAndAnalyzeRealtimeAsync(realtimeCts.Token);
                Task.Run(async () => await GetVoltageRealtimeAsync(realtimeCts.Token));
                Task.Run(async () => await PlayAndAnalyzeRealtimeAsync(realtimeCts.Token));
                //await Task.WhenAll(_voltageTask, _playTask);
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                
                
            }
            
            
        }
        private async Task OnCommandStop(object? e)
        {
            try
            {
                realtimeCts.Cancel();
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {

            }
        }

        private async Task OnRoiDragCommand(RoiDragArgs? e)
        {
            if(ImageControl.Image.Source==null)
            {
                return;
            }



            var dx = (int)Math.Round(e.Dx);
            var dy = (int)Math.Round(e.Dy);
            var right = Settings.MasterROI.X + Settings.MasterROI.Width;
            var bottom = Settings.MasterROI.Y + Settings.MasterROI.Height;

            switch (e.Handle)
            {
                case RoiHandle.Move:
                    Settings.MasterROI.X += dx;
                    Settings.MasterROI.Y += dy;
                    break;
                case RoiHandle.E:
                    Settings.MasterROI.Width += dx;
                    break;

                case RoiHandle.S:
                    Settings.MasterROI.Height += dy;
                    break;

                case RoiHandle.W:
                    Settings.MasterROI.X += dx;
                    Settings.MasterROI.Width = right - Settings.MasterROI.X;
                    break;

                case RoiHandle.N:
                    Settings.MasterROI.Y += dy;
                    Settings.MasterROI.Height = bottom - Settings.MasterROI.Y;
                    break;

                case RoiHandle.SE:
                    Settings.MasterROI.Width += dx;
                    Settings.MasterROI.Height+= dy;
                    break;

                case RoiHandle.NE:
                    Settings.MasterROI.Y += dy;
                    Settings.MasterROI.Height = bottom - Settings.MasterROI.Y;
                    Settings.MasterROI.Width += dx;
                    break;

                case RoiHandle.SW:
                    Settings.MasterROI.X += dx;
                    Settings.MasterROI.Width = right - Settings.MasterROI.X;
                    Settings.MasterROI.Height += dy;
                    break;

                case RoiHandle.NW:
                    Settings.MasterROI.X += dx;
                    Settings.MasterROI.Width = right - Settings.MasterROI.X;
                    Settings.MasterROI.Y+= dy;
                    Settings.MasterROI.Height= bottom - Settings.MasterROI.Y;
                    break;
                default:
                    break;
            }

            

            if(Settings.MasterROI.X< 0)
            { Settings.MasterROI.X = 0; }
            if(Settings.MasterROI.Y < 0)
            { Settings.MasterROI.Y = 0; }

            if(Settings.MasterROI.X + Settings.MasterROI.Width > ImageControl.Image.Source.Width)
            { Settings.MasterROI.X = (int)Math.Round(ImageControl.Image.Source.Width,MidpointRounding.ToZero) - Settings.MasterROI.Width; }

            if (Settings.MasterROI.Y + Settings.MasterROI.Height> ImageControl.Image.Source.Height)
            { Settings.MasterROI.Y = (int)Math.Round(ImageControl.Image.Source.Height, MidpointRounding.ToZero) - Settings.MasterROI.Height; }

        }
        private void OnCommandResizeROI(DragDeltaEventArgs? e)
        {
            if (ImageControl.Image.Source == null)
            {
                return;
            }

        }
        private async Task PlayAndAnalyzeRealtimeAsync(CancellationToken ct)
        {            
            double frameIntervalMs = 1000.0 / CamInformation.FPS;

            var frame = new Mat();
            using var gray = new Mat();

            int index = 0;
            double prevIntensity = -1.0;
            double runningMean = 0.0;
            int count = 0;
            int framecount = 0;
            // threshold는 실제 영상 보면서 조절
            double frameRatioThreshold = 0.03;   // 3% 이상 변화 시 깜빡임 후보
            var lastFrames = new Queue<FrameItem>();
            int preBufferCount = 2; // 전 2프레임까지 같이 넣기
            var added = new HashSet<int>();      // 중복 추가 방지

            Stopwatch playWatch = new Stopwatch();
            playWatch.Start();
            int skipCount = 0;

            try
            {
                while (!ct.IsCancellationRequested)
                {

                    frame = CamInformation.GetFrameImage(index);
                    double elapsedMs = playWatch.Elapsed.TotalMilliseconds;
                    int idealIndex = (int)(elapsedMs / frameIntervalMs);
                    frame.SaveImage("frame/" + index.ToString("D4") +  @".png");
                    //if (idealIndex > index)
                    //{
                    //    skipCount++;
                    //    Debug.WriteLine("FrameSkip:" + skipCount);
                    //    int framesToSkip = idealIndex - index;
                    //    for (int i = 0; i < framesToSkip; i++)
                    //    {
                    //        if (!CamInformation.cap.Grab())
                    //            break;
                    //        index++;
                    //        framecount++;
                    //    }
                    //}
                    framecount++;
                    ProgressValue = framecount;
                    InspectionImageControl.ImageSourceUpdate(frame, "Image");


                    //await Dispatcher.InvokeAsync(() =>
                    //{
                    //    txt_timestamp.Text = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", _stopwatch.Elapsed.Hours, _stopwatch.Elapsed.Minutes, _stopwatch.Elapsed.Seconds, _stopwatch.Elapsed.Milliseconds / 10);
                    //    prs_frame.Value = framecount; 
                    //});

                    // 2) ROI 계산 (detectionOnnx 사용)
                    var detected = Detection.Run(frame, false, true);
                    if (detected == null || !detected.Any())
                    {
                        //DetectionROIVisibility = Visibility.Collapsed;
                        index++;
                        continue;
                    }

                    Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

                    int w = gray.Cols;
                    int h = gray.Rows;
                    var d0 = detected.First(); // (x,y,w,h) 라고 가정

                    int roiX = d0.Item1;
                    int roiY = d0.Item2;
                    int roiW = d0.Item3;
                    int roiH = d0.Item4;

                    // 영상 범위 벗어나지 않게 보정
                    if (roiX < 0) roiX = 0;
                    if (roiY < 0) roiY = 0;
                    if (roiX + roiW > w) roiW = w - roiX;
                    if (roiY + roiH > h) roiH = h - roiY;

                    if (roiW <= 0 || roiH <= 0)
                    {
                        index++;
                        continue;
                    }

                    var roiRect = new OpenCvSharp.Rect(roiX, roiY, roiW, roiH);
                    using (var roi = new Mat(gray, roiRect))
                    {
                        var mean = Cv2.Mean(roi);
                        double intensity = mean.Val0;

                        // 3) running mean 업데이트 (실시간이라 과거 전체는 못 봄 → 근사치)
                        count++;
                        runningMean += (intensity - runningMean) / count;

                        double diffPrev = (prevIntensity < 0) ? 0.0 : Math.Abs(intensity - prevIntensity);
                        double ratioPrev = (runningMean > 0) ? diffPrev / (runningMean + 1e-6) : 0.0;

                        // 4) 썸네일 버퍼 업데이트
                        var thumb = CreateThumbnail(frame, 240, 180);
                        thumb.Freeze();



                        lastFrames.Enqueue(new FrameItem() { Index = index, Thumbnail = thumb });
                        while (lastFrames.Count > preBufferCount + 1)
                        {
                            lastFrames.Dequeue();
                        }
                        // 5) 깜빡임으로 판단되면 직전 프레임들과 함께 ListBox에 추가
                        if (ratioPrev > frameRatioThreshold)
                        {
                            DetectedFrameItem newDetect = new DetectedFrameItem();
                            newDetect.Index = index;
                            newDetect.Thumbnail = thumb;
                            newDetect.Voltage = CurrentVolt;
                            if (CurrentVolt > SpecInMaxVolt || CurrentVolt < SpecInMinVolt)
                            {
                                newDetect.Result = "OK";
                            }
                            else if (CurrentVolt < SpecInMaxVolt && CurrentVolt > SpecInMinVolt)
                            {
                                newDetect.Result = "NG";
                            }
                            else
                            {
                                Console.WriteLine("what?");
                            }
                            var framesToAdd = lastFrames.ToList();
                            lastFrames.Dequeue();
                            
                            if(IsSaveSpecOut)
                            {
                                await Application.Current.Dispatcher.InvokeAsync(() =>
                                {
                                    foreach (var item in framesToAdd)
                                    {
                                        newDetect.Frames.Add(item);
                                    }
                                    DetectedFrameItem.Add(newDetect);
                                });
                            }
                            else
                            {
                                if (newDetect.Result == "OK")
                                {

                                }
                                else
                                {
                                    await Application.Current.Dispatcher.InvokeAsync(() =>
                                    {
                                        foreach (var item in framesToAdd)
                                        {
                                            newDetect.Frames.Add(item);
                                        }
                                        DetectedFrameItem.Add(newDetect);
                                    });
                                }
                            }
                                
                            
                        }

                        prevIntensity = intensity;
                    }

                    index++;
                    //InspectionImageControl.Image.Source = bmp;
                    UpdateRoiRectFromDetection(frame, roiX, roiY, roiW, roiH);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine();
            }
            finally 
            {
                
            }
            
            IsInspection = false;
        }
        private async Task GetVoltageRealtimeAsync(CancellationToken ct)
        {
            Stopwatch sw = Stopwatch.StartNew();
            CurrentVolt = StartVolt;
            int direction = +1;
            long lastMs = 0;
            List<double> VoltageHistory = new List<double>();
            VoltageHistory.Add(CurrentVolt);
            VoltageGraph[0].Values = VoltageHistory;
            while (!ct.IsCancellationRequested)
            {
                if(IsInspection==false)
                {
                    break;
                }
                long nowMs = sw.ElapsedMilliseconds;
                long elapsedMs = nowMs - lastMs;

                // UpdateIntervalMs 단위로 몇 tick이 "쌓였는지"
                long ticks = elapsedMs / (long)UpdateIntervalMs;

                if (ticks <= 0)
                {
                    // 아직 한 tick도 안 쌓였으면 CPU 점유 줄이기
                    // (너무 촘촘하면 1~5ms 정도로 조절)
                    await Task.Delay(1, ct);
                    continue;
                }

                // 처리한 tick만큼 시간을 "소모" 처리
                lastMs += ticks * (long)UpdateIntervalMs;

                // 쌓인 tick 수만큼 step을 여러 번 적용 (catch-up)
                for (long i = 0; i < ticks; i++)
                {
                    CurrentVolt += direction * VoltStep;
                    VoltageHistory.Add(CurrentVolt);
                    VoltageGraph[0].Values = VoltageHistory;
                    // EndVolt 도달하면 방향 반전 + 클램프
                    if (direction > 0 && CurrentVolt >= EndVolt)
                    {
                        CurrentVolt = EndVolt;
                        direction = -1;
                    }
                    // StartVolt 도달하면 방향 반전 + 클램프
                    else if (direction < 0 && CurrentVolt <= StartVolt)
                    {
                        CurrentVolt = StartVolt;
                        direction = +1;
                    }
                }
            }
        }
        private void UpdateRoiRectFromDetection(Mat frame, int roiX, int roiY, int roiW, int roiH)
        {
            // frame: OpenCV Mat (원본 프레임)
            // roiX, roiY, roiW, roiH: detectionOnnx가 준 ROI (frame 좌표계)

            //if (InspectionImageControl.Image.ActualWidth <= 0 || InspectionImageControl.Image.ActualHeight <= 0)
            //    return;

            double imgW = frame.Width;
            double imgH = frame.Height;

            double frameW = frame.Cols;
            double frameH = frame.Rows;

            // Image.Stretch = Uniform 가정
            double scale = Math.Min(imgW / frameW, imgH / frameH);

            double drawFrameW = frameW * scale;
            double drawFrameH = frameH * scale;

            // 레터박스 보정 (가운데 정렬)
            double offsetX = (imgW - drawFrameW) / 2.0;
            double offsetY = (imgH - drawFrameH) / 2.0;

            double rectX = offsetX + roiX * scale;
            double rectY = offsetY + roiY * scale;
            double rectW = roiW * scale;
            double rectH = roiH * scale;

            ROIX = (int)rectX;
            ROIY = (int)rectY;
            
            ROIWidth = (int)rectW;
            ROIHeight = (int)rectH;
            DetectionROIVisibility = Visibility.Visible;
        }
        private BitmapSource CreateThumbnail(Mat src, int maxWidth, int maxHeight)
        {
            double scaleX = (double)maxWidth / src.Cols;
            double scaleY = (double)maxHeight / src.Rows;
            double scale = Math.Min(scaleX, scaleY);
            if (scale > 1.0) scale = 1.0;

            int w = (int)(src.Cols * scale);
            int h = (int)(src.Rows * scale);

            using var resized = new Mat();
            Cv2.Resize(src, resized, new OpenCvSharp.Size(w, h));
            return BitmapSourceConverter.ToBitmapSource(resized);
        }
        #endregion

    }
}
