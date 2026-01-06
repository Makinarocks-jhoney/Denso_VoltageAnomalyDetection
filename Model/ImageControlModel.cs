using CommunityToolkit.Mvvm.ComponentModel;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace MVP_Voltage.Model
{
    internal class ImageControlModel : ObservableObject
    {
        public ImageControlModel()
        {
            Image.Stretch = Stretch.None;
            ImageBrush.Stretch = Stretch.None;
        }

        #region ---［ Image ］---------------------------------------------------------------------

        public Image Image
        {
            get { return _image; }
            set { _image = value; OnPropertyChanged("Image"); }
        }
        private Image _image = new Image();

        public double ImageScaleX
        {
            get { return _imageScaleX; }
            set { _imageScaleX = value; OnPropertyChanged("ImageScaleX"); }
        }
        private double _imageScaleX = 1;

        public double ImageScaleY
        {
            get { return _imageScaleY; }
            set { _imageScaleY = value; OnPropertyChanged("ImageScaleY"); }
        }
        private double _imageScaleY = 1;

        public double Image_XDPI
        {
            get { return _image_XDPI; }
            set { _image_XDPI = value; OnPropertyChanged("Image_XDPI"); }
        }
        private double _image_XDPI = 96;

        public double Image_YDPI
        {
            get { return _image_YDPI; }
            set { _image_YDPI = value; OnPropertyChanged("Image_YDPI"); }
        }
        private double _image_YDPI = 96;

        public string ImagePath
        {
            get { return _imagePath; }
            set { _imagePath = value; ImageSourceUpdate(_imagePath, "Image"); OnPropertyChanged("ImagePath"); }
        }
        private string _imagePath = "";
        #endregion ---------------------------------------------------------------------------------

        #region ---［ ImageBrush ］---------------------------------------------------------------------

        public ImageBrush ImageBrush
        {
            get { return _imageBrush; }
            set { _imageBrush = value; OnPropertyChanged("ImageBrush"); }
        }
        private ImageBrush _imageBrush = new ImageBrush();

        public double ImageBrushScaleX
        {
            get { return _imageBrushScaleX; }
            set { _imageBrushScaleX = value; OnPropertyChanged("ImageBrushScaleX"); }
        }
        private double _imageBrushScaleX = 1;

        public double ImageBrushScaleY
        {
            get { return _imageBrushScaleY; }
            set { _imageBrushScaleY = value; OnPropertyChanged("ImageBrushScaleY"); }
        }
        private double _imageBrushScaleY = 1;

        public double ImageBrush_XDPI
        {
            get { return _imageBrush_XDPI; }
            set { _imageBrush_XDPI = value; OnPropertyChanged("ImageBrush_XDPI"); }
        }
        private double _imageBrush_XDPI = 96;

        public double ImageBrush_YDPI
        {
            get { return _imageBrush_YDPI; }
            set { _imageBrush_YDPI = value; OnPropertyChanged("ImageBrush_YDPI"); }
        }
        private double _imageBrush_YDPI = 96;

        public string ImageBrushPath
        {
            get { return _imageBrushPath; }
            set { _imageBrushPath = value; ImageSourceUpdate(_imageBrushPath, "ImageBrush"); OnPropertyChanged("ImageBrushPath"); }
        }
        private string _imageBrushPath = "";
        #endregion ---------------------------------------------------------------------------------

        public void ImageSourceUpdate(string ImagePath, string Target)
        {
            if (ImagePath == "" || ImagePath == null)
            {
                return;
            }

            BitmapImage b = new BitmapImage();
            b.UriSource = null;
            var stream = File.OpenRead(ImagePath);
            b.BeginInit();
            b.CacheOption = BitmapCacheOption.OnLoad;
            b.StreamSource = stream;
            b.EndInit();
            stream.Close();
            stream.Dispose();

            switch (Target)
            {
                case "Image":
                    Image.Source = b;
                    Image_XDPI = b.DpiX;
                    Image_YDPI = b.DpiY;
                    break;
                case "ImageBrush":
                    ImageBrush.ImageSource = b;
                    ImageBrush_XDPI = b.DpiX;
                    ImageBrush_YDPI = b.DpiY;
                    ImageBrush.Viewbox = new System.Windows.Rect(0, 0, b.PixelWidth, b.PixelHeight);
                    ImageBrush.ViewboxUnits = BrushMappingMode.Absolute;
                    ImageBrush.Stretch = Stretch.UniformToFill;
                    break;
            }
            RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(ImageBrush, BitmapScalingMode.NearestNeighbor);
        }

        public void ImageSourceUpdate(Mat MatImage, string Target)
        {
            if (MatImage == null || MatImage.Data.ToString() == "0")
            {
                if (Target == "Image")
                {
                    Image.Source = null;
                }
                else
                {
                    ImageBrush.ImageSource = null;
                }


                return;
            }

            Bitmap bitmap = null;
            if (MatImage.Type() == OpenCvSharp.MatType.CV_8UC1)
            {
                Mat tempImage = new Mat();
                Cv2.CvtColor(MatImage, tempImage, ColorConversionCodes.GRAY2BGR);
                bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(tempImage, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            }
            if (MatImage.Type() == OpenCvSharp.MatType.CV_8UC3)
            {
                bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(MatImage, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            }
            if (MatImage.Type() == OpenCvSharp.MatType.CV_8UC4)
            {
                bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(MatImage, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }

            BitmapImage b = SetThumbnailBitmap(bitmap);

            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                switch (Target)
                {
                    case "Image":
                        Image.Source = b;
                        Image_XDPI = b.DpiX;
                        Image_YDPI = b.DpiY;
                        break;
                    case "ImageBrush":
                        ImageBrush.ImageSource = b;
                        ImageBrush_XDPI = b.DpiX;
                        ImageBrush_YDPI = b.DpiY;
                        break;
                }
            });

        }

        BitmapImage SetThumbnailBitmap(Bitmap inputBitmap)
        {
            using (var memory = new MemoryStream())
            {
                inputBitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.DecodePixelWidth = inputBitmap.Width;
                bitmapImage.DecodePixelHeight = inputBitmap.Height;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }

        }
    }
}
