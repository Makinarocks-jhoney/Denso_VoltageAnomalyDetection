using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MahApps.Metro.Controls.Dialogs;
using MvCameraControl;
using MVP_Voltage.Model;
using MVP_Voltage.Services;
using MVP_Voltage.Util;
using MVP_Voltage.View;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace MVP_Voltage
{
    class MainWindowViewModel:ObservableObject
    {

        #region 프로퍼티
        public IDialogCoordinator _dialogCoordinator;
        public Settings Settings { get; } = new Settings();
        
        public ObservableCollection<string> Logs
        {
            get { return _logs; }
            set { _logs = value; OnPropertyChanged("Logs"); }
        }
        private ObservableCollection<string> _logs = new ObservableCollection<string>();
        public string LogsText => string.Join(Environment.NewLine, Logs);

        #region ---［ ViewModels ］---------------------------------------------------------------------

        public CamSettingViewModel CamSettingViewModel
        {
            get { return _camSettingViewModel; }
            set { _camSettingViewModel = value; OnPropertyChanged("CamSettingViewModel"); }
        }
        private CamSettingViewModel _camSettingViewModel;

        public VideoFileSettingViewModel VideoFileSettingViewModel
        {
            get { return _videoFileSettingViewModel; }
            set { _videoFileSettingViewModel = value; OnPropertyChanged("VideoFileSettingViewModel"); }
        }
        private VideoFileSettingViewModel _videoFileSettingViewModel = new VideoFileSettingViewModel();

        #endregion ---------------------------------------------------------------------------------

        

        #endregion
        #region 커맨드
        

        
        
        #endregion

        #region 초기화
        public MainWindowViewModel()
        {
            InitData();
            InitCommand();
            InitEvent();
            _dialogCoordinator = DialogCoordinator.Instance;
            if (File.Exists("settings.json"))
            {
                SaveLoadJson.LoadData<Settings>("settings.json", Settings);
            }
                
            CamSettingViewModel = new CamSettingViewModel(this);
        }

        void InitData()
        {
            
            Logs.CollectionChanged += (_, __) => { OnPropertyChanged(nameof(LogsText)); };
        }

        void InitCommand()
        {
            
            
        }

        

        void InitEvent()
        {

        }
        #endregion

        #region 이벤트
        

        


        




        public async Task<bool> TrySaveAndCloseAsync()
        {
            var result = await _dialogCoordinator.ShowMessageAsync(this, "Close", "", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                //if (SystemSettingViewModel.IsConnectedCam)
                //{
                //    await SystemSettingViewModel._camera.CloseAsync();
                //}
                CamSettingViewModel.Cam1.DisconnectCamera();
                await JsonSaveLoadModel.SaveAsync("settings.json", Settings);
                
                
                
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool AddLog(string LogContent)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Logs.Count > 1000)
                    {
                        Logs.RemoveAt(0);
                    }
                    Logs.Insert(0, $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] " + LogContent);
                });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

    }
}
