using LiveChartsCore.Measure;
using MVP_Voltage.Model;
using MVP_Voltage.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MVP_Voltage.Themes
{
    public sealed class ROISelectorControl:Control
    {
        static ROISelectorControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ROISelectorControl),
                new FrameworkPropertyMetadata(typeof(ROISelectorControl)));
        }

        // ====== Public DPs ======

        public ROIModel Roi
        {
            get => (ROIModel)GetValue(RoiProperty);
            set => SetValue(RoiProperty, value);
        }
        public static readonly DependencyProperty RoiProperty =
            DependencyProperty.Register(nameof(Roi), typeof(ROIModel), typeof(ROISelectorControl),
                new PropertyMetadata(null, OnRoiChanged));

        public double BoundsW
        {
            get => (double)GetValue(BoundsWProperty);
            set => SetValue(BoundsWProperty, value);
        }
        public static readonly DependencyProperty BoundsWProperty =
            DependencyProperty.Register(nameof(BoundsW), typeof(double), typeof(ROISelectorControl),
                new PropertyMetadata(0.0, OnBoundsChanged));

        public double BoundsH
        {
            get => (double)GetValue(BoundsHProperty);
            set => SetValue(BoundsHProperty, value);
        }
        public static readonly DependencyProperty BoundsHProperty =
            DependencyProperty.Register(nameof(BoundsH), typeof(double), typeof(ROISelectorControl),
                new PropertyMetadata(0.0, OnBoundsChanged));

        public int HandleSize
        {
            get => (int)GetValue(HandleSizeProperty);
            set => SetValue(HandleSizeProperty, value);
        }
        public static readonly DependencyProperty HandleSizeProperty =
            DependencyProperty.Register(nameof(HandleSize), typeof(int), typeof(ROISelectorControl),
                new PropertyMetadata(12, OnHandleSizeChanged));

        public double DimOpacity
        {
            get => (double)GetValue(DimOpacityProperty);
            set => SetValue(DimOpacityProperty, value);
        }
        public static readonly DependencyProperty DimOpacityProperty =
            DependencyProperty.Register(nameof(DimOpacity), typeof(double), typeof(ROISelectorControl),
                new PropertyMetadata(0.5));

        // ====== Derived DPs (읽기 전용처럼 사용) ======
        // HalfHandle / DimRightW / DimBottomH
        public int HalfHandle
        {
            get => (int)GetValue(HalfHandleProperty);
            private set => SetValue(HalfHandleProperty, value);
        }
        public static readonly DependencyProperty HalfHandleProperty =
            DependencyProperty.Register(nameof(HalfHandle), typeof(int), typeof(ROISelectorControl),
                new PropertyMetadata(6));

        public int DimRightW
        {
            get => (int)GetValue(DimRightWProperty);
            private set => SetValue(DimRightWProperty, value);
        }
        public static readonly DependencyProperty DimRightWProperty =
            DependencyProperty.Register(nameof(DimRightW), typeof(int), typeof(ROISelectorControl),
                new PropertyMetadata(0));

        public int DimBottomH
        {
            get => (int)GetValue(DimBottomHProperty);
            private set => SetValue(DimBottomHProperty, value);
        }
        public static readonly DependencyProperty DimBottomHProperty =
            DependencyProperty.Register(nameof(DimBottomH), typeof(int), typeof(ROISelectorControl),
                new PropertyMetadata(0));

        // 핸들 좌표(정중앙 보정 포함)
        public int HandleNLeft { get => (int)GetValue(HandleNLeftProperty); private set => SetValue(HandleNLeftProperty, value); }
        public int HandleNTop { get => (int)GetValue(HandleNTopProperty); private set => SetValue(HandleNTopProperty, value); }
        public int HandleSLeft { get => (int)GetValue(HandleSLeftProperty); private set => SetValue(HandleSLeftProperty, value); }
        public int HandleSTop { get => (int)GetValue(HandleSTopProperty); private set => SetValue(HandleSTopProperty, value); }
        public int HandleWLeft { get => (int)GetValue(HandleWLeftProperty); private set => SetValue(HandleWLeftProperty, value); }
        public int HandleWTop { get => (int)GetValue(HandleWTopProperty); private set => SetValue(HandleWTopProperty, value); }
        public int HandleELeft { get => (int)GetValue(HandleELeftProperty); private set => SetValue(HandleELeftProperty, value); }
        public int HandleETop { get => (int)GetValue(HandleETopProperty); private set => SetValue(HandleETopProperty, value); }

        public int HandleNWLeft { get => (int)GetValue(HandleNWLeftProperty); private set => SetValue(HandleNWLeftProperty, value); }
        public int HandleNWTop { get => (int)GetValue(HandleNWTopProperty); private set => SetValue(HandleNWTopProperty, value); }
        public int HandleNELeft { get => (int)GetValue(HandleNELeftProperty); private set => SetValue(HandleNELeftProperty, value); }
        public int HandleNETop { get => (int)GetValue(HandleNETopProperty); private set => SetValue(HandleNETopProperty, value); }
        public int HandleSWLeft { get => (int)GetValue(HandleSWLeftProperty); private set => SetValue(HandleSWLeftProperty, value); }
        public int HandleSWTop { get => (int)GetValue(HandleSWTopProperty); private set => SetValue(HandleSWTopProperty, value); }
        public int HandleSELeft { get => (int)GetValue(HandleSELeftProperty); private set => SetValue(HandleSELeftProperty, value); }
        public int HandleSETop { get => (int)GetValue(HandleSETopProperty); private set => SetValue(HandleSETopProperty, value); }

        public static readonly DependencyProperty HandleNLeftProperty = DependencyProperty.Register(nameof(HandleNLeft), typeof(int), typeof(ROISelectorControl), new PropertyMetadata(0));
        public static readonly DependencyProperty HandleNTopProperty = DependencyProperty.Register(nameof(HandleNTop), typeof(int), typeof(ROISelectorControl), new PropertyMetadata(0));
        public static readonly DependencyProperty HandleSLeftProperty = DependencyProperty.Register(nameof(HandleSLeft), typeof(int), typeof(ROISelectorControl), new PropertyMetadata(0));
        public static readonly DependencyProperty HandleSTopProperty = DependencyProperty.Register(nameof(HandleSTop), typeof(int), typeof(ROISelectorControl), new PropertyMetadata(0));
        public static readonly DependencyProperty HandleWLeftProperty = DependencyProperty.Register(nameof(HandleWLeft), typeof(int), typeof(ROISelectorControl), new PropertyMetadata(0));
        public static readonly DependencyProperty HandleWTopProperty = DependencyProperty.Register(nameof(HandleWTop), typeof(int), typeof(ROISelectorControl), new PropertyMetadata(0));
        public static readonly DependencyProperty HandleELeftProperty = DependencyProperty.Register(nameof(HandleELeft), typeof(int), typeof(ROISelectorControl), new PropertyMetadata(0));
        public static readonly DependencyProperty HandleETopProperty = DependencyProperty.Register(nameof(HandleETop), typeof(int), typeof(ROISelectorControl), new PropertyMetadata(0));

        public static readonly DependencyProperty HandleNWLeftProperty = DependencyProperty.Register(nameof(HandleNWLeft), typeof(int), typeof(ROISelectorControl), new PropertyMetadata(0));
        public static readonly DependencyProperty HandleNWTopProperty = DependencyProperty.Register(nameof(HandleNWTop), typeof(int), typeof(ROISelectorControl), new PropertyMetadata(0));
        public static readonly DependencyProperty HandleNELeftProperty = DependencyProperty.Register(nameof(HandleNELeft), typeof(int), typeof(ROISelectorControl), new PropertyMetadata(0));
        public static readonly DependencyProperty HandleNETopProperty = DependencyProperty.Register(nameof(HandleNETop), typeof(int), typeof(ROISelectorControl), new PropertyMetadata(0));
        public static readonly DependencyProperty HandleSWLeftProperty = DependencyProperty.Register(nameof(HandleSWLeft), typeof(int), typeof(ROISelectorControl), new PropertyMetadata(0));
        public static readonly DependencyProperty HandleSWTopProperty = DependencyProperty.Register(nameof(HandleSWTop), typeof(int), typeof(ROISelectorControl), new PropertyMetadata(0));
        public static readonly DependencyProperty HandleSELeftProperty = DependencyProperty.Register(nameof(HandleSELeft), typeof(int), typeof(ROISelectorControl), new PropertyMetadata(0));
        public static readonly DependencyProperty HandleSETopProperty = DependencyProperty.Register(nameof(HandleSETop), typeof(int), typeof(ROISelectorControl), new PropertyMetadata(0));

        // ====== Thumb wiring ======
        private const int MinW = 8;
        private const int MinH = 8;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            HookThumb("PART_Move", RoiHandle.Move);

            HookThumb("PART_N", RoiHandle.N);
            HookThumb("PART_S", RoiHandle.S);
            HookThumb("PART_W", RoiHandle.W);
            HookThumb("PART_E", RoiHandle.E);

            HookThumb("PART_NW", RoiHandle.NW);
            HookThumb("PART_NE", RoiHandle.NE);
            HookThumb("PART_SW", RoiHandle.SW);
            HookThumb("PART_SE", RoiHandle.SE);

            UpdateDerived();
        }

        private void HookThumb(string name, RoiHandle handle)
        {
            if (GetTemplateChild(name) is not Thumb t) return;
            t.DragDelta -= OnThumbDragDelta;
            t.DragDelta += OnThumbDragDelta;
            t.Tag = handle;
        }

        private void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (Roi is null) return;
            if (sender is not Thumb t || t.Tag is not RoiHandle h) return;

            int dx = (int)Math.Round(e.HorizontalChange);
            int dy = (int)Math.Round(e.VerticalChange);

            ApplyDrag(h, dx, dy);
            UpdateDerived();
        }

        private void ApplyDrag(RoiHandle h, int dx, int dy)
        {
            var roi = Roi!;
            int right = roi.Right;
            int bottom = roi.Bottom;

            switch (h)
            {
                case RoiHandle.Move:
                    roi.X += dx; roi.Y += dy;
                    break;

                case RoiHandle.E: roi.Width += dx; break;
                case RoiHandle.S: roi.Height += dy; break;
                case RoiHandle.W:
                    roi.X += dx;
                    roi.Width = right - roi.X;
                    break;
                case RoiHandle.N:
                    roi.Y += dy;
                    roi.Height = bottom - roi.Y;
                    break;

                case RoiHandle.SE: roi.Width += dx; roi.Height += dy; break;
                case RoiHandle.NE:
                    roi.Y += dy; roi.Height = bottom - roi.Y;
                    roi.Width += dx;
                    break;
                case RoiHandle.SW:
                    roi.X += dx; roi.Width = right - roi.X;
                    roi.Height += dy;
                    break;
                case RoiHandle.NW:
                    roi.X += dx; roi.Width = right - roi.X;
                    roi.Y += dy; roi.Height = bottom - roi.Y;
                    break;
            }

            // 최소 크기
            if (roi.Width < MinW)
            {
                roi.Width = MinW;
                if (h is RoiHandle.W or RoiHandle.SW or RoiHandle.NW)
                    roi.X = right - roi.Width;
            }
            if (roi.Height < MinH)
            {
                roi.Height = MinH;
                if (h is RoiHandle.N or RoiHandle.NE or RoiHandle.NW)
                    roi.Y = bottom - roi.Height;
            }

            // 경계 Clamp (BoundsW/H가 0이면 제한 없음)
            if (roi.X < 0) roi.X = 0;
            if (roi.Y < 0) roi.Y = 0;

            if (BoundsW > 0 && roi.Right > BoundsW)
            {
                if (h is RoiHandle.E or RoiHandle.SE or RoiHandle.NE)
                    roi.Width = Math.Max(MinW, (int)BoundsW - roi.X);
                else
                    roi.X = Math.Max(0, (int)BoundsW - roi.Width);
            }

            if (BoundsH > 0 && roi.Bottom > BoundsH)
            {
                if (h is RoiHandle.S or RoiHandle.SE or RoiHandle.SW)
                    roi.Height = Math.Max(MinH, (int)BoundsH - roi.Y);
                else
                    roi.Y = Math.Max(0, (int)BoundsH - roi.Height);
            }
        }

        // ====== Derived update ======
        private INotifyPropertyChanged? _roiInpc;

        private static void OnRoiChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (ROISelectorControl)d;

            if (c._roiInpc != null)
                c._roiInpc.PropertyChanged -= c.OnRoiPropertyChanged;

            c._roiInpc = e.NewValue as INotifyPropertyChanged;

            if (c._roiInpc != null)
                c._roiInpc.PropertyChanged += c.OnRoiPropertyChanged;

            c.UpdateDerived();
        }

        private void OnRoiPropertyChanged(object? sender, PropertyChangedEventArgs e) => UpdateDerived();

        private static void OnBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((ROISelectorControl)d).UpdateDerived();

        private static void OnHandleSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((ROISelectorControl)d).UpdateDerived();

        private void UpdateDerived()
        {
            HalfHandle = Math.Max(0, HandleSize / 2);

            if (Roi is null)
            {
                DimRightW = 0; DimBottomH = 0;
                return;
            }

            // Dim 계산
            DimRightW = Math.Max(0, (int)BoundsW - Roi.Right);
            DimBottomH = Math.Max(0, (int)BoundsH - Roi.Bottom);

            // 핸들 좌표(핸들 중심이 ROI 에지/코너에 걸치도록)
            int cx = Roi.X + Roi.Width / 2;
            int cy = Roi.Y + Roi.Height / 2;

            // Corners
            HandleNWLeft = Roi.X - HalfHandle; HandleNWTop = Roi.Y - HalfHandle;
            HandleNELeft = Roi.Right - HalfHandle; HandleNETop = Roi.Y - HalfHandle;
            HandleSWLeft = Roi.X - HalfHandle; HandleSWTop = Roi.Bottom - HalfHandle;
            HandleSELeft = Roi.Right - HalfHandle; HandleSETop = Roi.Bottom - HalfHandle;

            // Edges (centered)
            HandleNLeft = cx - HalfHandle; HandleNTop = Roi.Y - HalfHandle;
            HandleSLeft = cx - HalfHandle; HandleSTop = Roi.Bottom - HalfHandle;
            HandleWLeft = Roi.X - HalfHandle; HandleWTop = cy - HalfHandle;
            HandleELeft = Roi.Right - HalfHandle; HandleETop = cy - HalfHandle;
        }
    }
}
