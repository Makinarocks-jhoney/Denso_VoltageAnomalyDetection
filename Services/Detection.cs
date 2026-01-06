using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MVP_Voltage.Services
{
    class Detection
    {
        private InferenceSession _session;
        private string _inputName;
        private const string LogitsoutputName = "logits";
        private const string FeaturesoutputName = "features";
        private readonly float[,] _fcWeights;
        DisposableNamedOnnxValue? logitsItem = null;
        DisposableNamedOnnxValue? featItem = null;
        private int _deviceId;
        private string _onnxPath = "";
        bool _useCuda = true;
        public Detection()
        {

        }
        public bool Initialize(string onnxPath, bool useCuda = true, int deviceId = 0)
        {
            try
            {
                _onnxPath = onnxPath;
                _useCuda = useCuda;
                _deviceId = deviceId;
                ReCreateSession();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool DeInitialize()
        {
            try
            {

                _session.Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public void ReCreateSession()
        {
            SessionOptions opt = new SessionOptions
            {
                LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_WARNING,
                GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
                IntraOpNumThreads = Environment.ProcessorCount
            };
            if (_useCuda)
            {
                // ✅ ORT 1.18+에선 이 오버로드가 가장 단순/호환 잘 됨
                opt.AppendExecutionProvider_CUDA(_deviceId);
            }
            // else: CPU EP만 사용

            _session = new InferenceSession(_onnxPath, opt);
            _inputName = _session.InputMetadata.Keys.First();
        }

        public KeyValuePair<string, string> GetModelMetaData()
        {
            return _session.ModelMetadata.CustomMetadataMap.First();
        }

        public List<(int, int, int, int, float, int)> Run(Mat bgrU8, bool UsingCam, bool IsContinuous)
        {
            if (bgrU8.Empty() || bgrU8.Type() != MatType.CV_8UC3)
                throw new ArgumentException("Input must be non-empty CV_8UC3 (BGR).");

            Mat? rgb = null;
            NamedOnnxValue? input = null;
            IDisposableReadOnlyCollection<DisposableNamedOnnxValue>? results = null;
            DisposableNamedOnnxValue? camItem = null;
            float[]? rentedChw = null;

            try
            {
                // 1) BGR -> RGB, float32, [0,1], 리사이즈
                rgb = new Mat();
                float scale = 0;
                int padX = 0, padY = 0;
                //Cv2.CvtColor(bgrU8, rgb, ColorConversionCodes.BGR2RGB);
                (rgb, scale, padX, padY) = Letterbox(bgrU8);
                //Cv2.Resize(bgrU8, rgb, new Size(640, 360));
                rgb.ConvertTo(rgb, MatType.CV_32FC3, 1.0 / 255.0);
                //Cv2.Resize(rgb, rgb, new OpenCvSharp.Size(224, 224));

                // 2) HWC -> CHW
                int H = rgb.Rows, W = rgb.Cols;
                int plane = H * W;
                int len = 3 * plane;
                rentedChw = System.Buffers.ArrayPool<float>.Shared.Rent(len);
                FillChw(rgb, rentedChw, H, W);

                // 3) 입력 Tensor
                var inputShape = new int[] { 1, 3, H, W };
                var inputTensor = new DenseTensor<float>(rentedChw.AsMemory(0, len), inputShape);
                input = NamedOnnxValue.CreateFromTensor(_inputName, inputTensor);

                //bgrU8.SaveImage(@"C:\JHoney\Test\1.png");
                // 속도를 위해 logits만 요청
                results = _session.Run(new[] { input }, new[] { "output0" });
                var output = results.First().AsEnumerable<float>().ToArray();
                List<(int, int, int, int, float, int)> detections = new List<(int, int, int, int, float, int)>();
                detections.Add(new(
                    (int)((output[0] - padX) / scale),
                    (int)((output[1] - padY) / scale),
                    (int)(output[2]) + padY,
                    (int)(output[3]) - padY,
                    output[4],
                    (int)output[4]));
                return detections;
            }
            catch (Exception ex)
            {
                _session.Dispose();
                ReCreateSession();
                return new List<(int, int, int, int, float, int)>();
            }
            finally
            {
                featItem?.Dispose();
                logitsItem?.Dispose();
                results?.Dispose();
                input = null;

                if (rentedChw != null)
                    System.Buffers.ArrayPool<float>.Shared.Return(rentedChw, clearArray: true);

                rgb?.Dispose();

                // ⛔ 분류용이면 여기서 _session.Dispose() 하면 안됩니다.
                // 한 번 만든 세션을 계속 재사용하는 게 일반적이에요.
                if (IsContinuous == false)
                {
                    _session.Dispose();
                    ReCreateSession();
                }
            }
        }
        public (Mat resized, float scale, int padX, int padY) Letterbox(Mat src, int newSize = 640)
        {
            int w = src.Width;
            int h = src.Height;

            float scale = Math.Min((float)newSize / w, (float)newSize / h);

            int newW = (int)(w * scale);
            int newH = (int)(h * scale);

            // 1) aspect ratio 유지 리사이즈
            Mat resized = new Mat();
            Cv2.Resize(src, resized, new OpenCvSharp.Size(newW, newH));

            // 2) zero padding
            int padX = (newSize - newW) / 2;
            int padY = (newSize - newH) / 2;

            Mat padded = new Mat(newSize, newSize, MatType.CV_8UC3, Scalar.All(0));
            resized.CopyTo(new Mat(padded, new OpenCvSharp.Rect(padX, padY, newW, newH)));

            return (padded, scale, padX, padY);
        }
        
        private void FillChw(Mat hwcFloatRgb, float[] dst, int h, int w)
        {
            var idx = hwcFloatRgb.GetGenericIndexer<Vec3f>();
            int plane = h * w;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int offset = y * w + x;
                    var v = idx[y, x]; // R,G,B
                    dst[0 * plane + offset] = v.Item0;
                    dst[1 * plane + offset] = v.Item1;
                    dst[2 * plane + offset] = v.Item2;
                }
            }
        }
        public void Dispose() => _session?.Dispose();
    }
}
