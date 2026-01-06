using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MVP_Voltage.Model
{
    public class CamInformation
    {
        public VideoCapture cap;
        public int TotalFrame { get; set; } = 0;
        public double FPS { get; set; } = 0;
        public string VideoPath { get; set; }
        public bool IsOpened { get; set; } = false;
        public CamInformation()
        {

        }

        public void Open(string videoPath)
        {
            if(cap!=null)
            {
                cap.Dispose();
            }
            cap = new VideoCapture(videoPath);
            TotalFrame = cap.FrameCount;
            FPS = cap.Fps;
            IsOpened = true;

        }

        public Mat GetFrameImage(int frame)
        {
            if(cap==null)
            {
                return new Mat();
            }
            if(frame<0 || frame>cap.FrameCount)
            {
                return new Mat();
            }
            cap.PosFrames = frame;
            var frameImage = new Mat();
            if (!cap.Read(frameImage) || frameImage.Empty())
            {
                Console.WriteLine("Failed to read frame");
                return null;
            }
            
            return frameImage;
        }

    }
}
