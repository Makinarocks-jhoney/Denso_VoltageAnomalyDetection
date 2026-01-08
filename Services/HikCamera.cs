using CommunityToolkit.Mvvm.ComponentModel;
using MvCameraControl;
using OpenCvSharp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MVP_Voltage.Services
{
    internal class HikCamera:IDisposable
    {
        IDevice? _device;
        private CancellationTokenSource? _cts;
        private Task? _grabTask;
        private readonly object _lock = new();
        public event EventHandler<RawFrame>? FrameArrived;


        public bool IsConnected = false;
        public bool IsGrabbing => _grabTask is not null && !_grabTask.IsCompleted;


        public List<IDeviceInfo> GetDevices()
        {
            DeviceEnumerator.EnumDevices(DeviceTLayerType.MvGigEDevice, out List<IDeviceInfo> DeviceList);
            return DeviceList;
        }

        public bool ConnectCamera(IDeviceInfo deviceInfo)
        {
            try
            {
                _device = DeviceFactory.CreateDevice(deviceInfo);
                _device.Open();
                _device.Parameters.SetFloatValue("AcquisitionFrameRate", 10);
                _device.Parameters.SetIntValue("GevSCPSPacketSize", 1500);
                _device.Parameters.SetIntValue("GevSCPD", 10000);
                IsConnected = true;
                return true;
            }
            catch (Exception)
            {
                _device?.Dispose();
                _device = null;
                return false;
            }
        }

        public Mat GetFrame(uint timeout)
        {
            IFrameOut frameOut;
            Mat frame;
            
            try
            {
                _device.StreamGrabber.StartGrabbing();
                _device.StreamGrabber.GetImageBuffer(timeout, out frameOut);
                if (frameOut == null)
                    throw new Exception("frameOut null");
                frame = OpenCvSharp.Extensions.BitmapConverter.ToMat(frameOut.Image.ToBitmap());
                frameOut.Dispose();
            }
            catch (Exception ex)
            {
                frame = new Mat();
            }
            finally
            {
                _device.StreamGrabber.StopGrabbing();
            }
            
            return frame;
        }

        public void StartGrabbing(uint timeoutMs = 1000)
        {
            if (IsGrabbing) return;

            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            _device!.StreamGrabber.StartGrabbing();

            _grabTask = Task.Run(() =>
            {
                int status = -1;
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        status = _device.StreamGrabber.GetImageBuffer(timeoutMs, out IFrameOut frameOut);
                        if (status != 0){ continue; }
                        var raw = CopyToRawFrame(frameOut.Image, frameOut.Image.PixelType);
                        
                        frameOut.Dispose();
                        FrameArrived?.Invoke(this, raw);
                    }
                    catch
                    {
                        // TODO: 로그/재시도 정책
                    }
                }
            }, ct);
        }

        public void StopGrabbing()
        {
            if (!IsGrabbing) return;

            try
            {
                _cts?.Cancel();
                _grabTask?.Wait(300);
            }
            catch { }
            finally
            {
                _grabTask = null;
                _cts?.Dispose();
                _cts = null;

                if (_device is not null)
                {
                    try { _device.StreamGrabber.StopGrabbing(); } catch { }
                }
            }
        }

        public async Task DisconnectCamera()
        {
            IsConnected = false;
            if(_cts!=null)
            {
                _cts?.Cancel();
                if(_grabTask!=null)
                {
                    _grabTask.Wait();
                    _grabTask.Dispose();
                    _grabTask = null;
                }
            }
            if(_device!=null)
            {
                _device.Close();
                _device.Dispose();
                _device = null;
            }
            

        }

        public void Dispose()
        {
            DisconnectCamera();
        }
        private static RawFrame CopyToRawFrame(IImage img, MvGvspPixelType fmt)
        {
            // 1) 폭/높이
            int w = (int)img.Width;
            int h = (int)img.Height;

            // 2) 포맷 결정 (여기가 SDK마다 다름)
            //    일단 가장 흔한 2가지: Mono8 또는 BGR8(=Bgr24)
            
            int bpp = 0;

            if(fmt==MvGvspPixelType.PixelType_Gvsp_BayerBG8)
            {
                bpp = 3;
            }
            else
            {
                bpp = 1;
            }

            int stride = w * bpp;
            int bytes = img.PixelData.Length;

            IntPtr ptr = img.PixelDataPtr;

            byte[] rented = ArrayPool<byte>.Shared.Rent(bytes);
            Marshal.Copy(ptr, rented, 0, bytes);

            // rent 했으니 실제 사용 길이만 담고, 나중에 UI에서 Return 하는 구조도 가능
            // MVP는 그냥 새 배열로 Trim해서 씀(간단)
            
            var frame = new RawFrame
            {
                Buffer = rented,
                Width = w,
                Height = h,
                Stride = stride,
                Format = fmt,
                Length = bytes,
                Timestamp = Environment.TickCount64
            };
            frame.SetReturn(b=> ArrayPool<byte>.Shared.Return(b));
            return frame;
        }
    }
}
